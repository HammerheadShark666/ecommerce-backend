using System.Net.Http.Json;
using ECommerce.Application.Abstractions;
using ECommerce.Domain.Entities.User;
using ECommerce.Infrastructure.Persistence;
using ECommerce.IntegrationTests.Library;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace ECommerce.IntegrationTests.Registration;

[Collection("Database")]
public class BeginRegistrationIntegrationTest : IAsyncLifetime
{
    private readonly SqlServerFixture _fixture;
    private readonly TestApplicationFactory _appFactory; 
    private readonly HttpClient _client;

    public BeginRegistrationIntegrationTest(SqlServerFixture fixture)
    {
        _fixture = fixture; 
        _appFactory = new TestApplicationFactory(_fixture.ConnectionString);
        _client = _appFactory.CreateClient();
    }

    [Fact]
    public async Task EndToEnd_Registration_And_Verify_Successful()
    {
        // Arrange
        string email = "reg@example.com";
        string password = "RegPass!1"; 

        // Act: begin registration
        HttpResponseMessage beginResp = await PostRegisterRawAsync(email, password, password);
        beginResp.EnsureSuccessStatusCode();

        BeginRegistrationResponse? beginDto = await beginResp.Content.ReadFromJsonAsync<BeginRegistrationResponse>();
        beginDto.Should().NotBeNull();
        beginDto!.OtpAuthUri.Should().Contain("secret=");

        // parse secret from uri
        var uri = new Uri(beginDto.OtpAuthUri);
        string query = uri.Query.TrimStart('?'); 

        Dictionary<string, StringValues> queryParams = QueryHelpers.ParseQuery(uri.Query);
        string secret = queryParams["secret"].First()
                                ?? throw new InvalidOperationException("Failed to parse query string.");
        secret.Should().NotBeNull();  
         
        // resolve one-time code using registered IOneTimePasswordGenerator
        string oneTimeCode;
        using (IServiceScope scope = _appFactory.Services.CreateScope())
        {
            IOneTimePasswordGenerator totpGenerator = scope.ServiceProvider.GetRequiredService<IOneTimePasswordGenerator>();
            oneTimeCode = totpGenerator.GetCurrentCode(secret);
        }

        HttpResponseMessage confirmResp = await PostConfirmRegisterRawAsync(email, oneTimeCode);
        confirmResp.EnsureSuccessStatusCode();

        // Assert: user is updated in database
        ConfirmRegistrationResponse? confirmDto = await confirmResp.Content.ReadFromJsonAsync<ConfirmRegistrationResponse>();
        confirmDto.Should().NotBeNull();
        confirmDto!.Success.Should().BeTrue();  

        DbContextOptions<ECommerceDbContext> options = new DbContextOptionsBuilder<ECommerceDbContext>()
            .UseSqlServer(_fixture.ConnectionString)
            .Options; 

        await using var db = new ECommerceDbContext(options);
        User? user = db.Users.FirstOrDefault(u => u.Email == email);
        user.Should().NotBeNull();
        user!.IsTwoFactorEnabled.Should().BeTrue();
        user.OneTimePasswordSecret.Should().NotBeNullOrWhiteSpace();

        Assert.Single(_appFactory.Publisher.PublishedMessages);
    }

    [Fact]
    public async Task VerifyRegistration_WithInvalidCode_ReturnsUnauthorized()
    {
        // Arrange
        string email = "badcode@example.com";
        string password = "RegPass!1";

        // Act
        HttpResponseMessage beginResp = await PostRegisterRawAsync(email, password, password);
        beginResp.EnsureSuccessStatusCode();

        // Act
        HttpResponseMessage verifyResp = await PostConfirmRegisterRawAsync(email, "000000");

        // Assert
        verifyResp.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized); 
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("missing@")]
    [InlineData("@nodomain.com")]
    [InlineData("spaces in@email.com")]
    [InlineData("double@@domain.com")]
    public async Task Begin_Register_Email_WhenInvalidFormat_ShouldHaveValidationError(string invalidEmail)
    {
        //Arrange
        string password = "RegPass!1";

        //Act
        HttpResponseMessage resp = await PostRegisterRawAsync(invalidEmail, password, password);

        //Assert
        resp.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

        ValidationProblemDetails? problem = await resp.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        problem.Should().NotBeNull();
        problem!.Errors.Should().ContainKey("email");
        problem.Errors["email"]
               .Should()
               .Contain("Email is not valid.");
    }

    [Fact]
    public async Task Registration_Fails_With_Missing_Entries()
    {
        // Arrange
        string email = "";
        string password = "";
        string confirmPassword = "";

        // Act
        HttpResponseMessage resp = await PostRegisterRawAsync(email, password, confirmPassword);

        // Assert
        resp.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

        ValidationProblemDetails? problem = await resp.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        problem.Should().NotBeNull();
        problem!.Errors.Should().ContainKey("email");
        problem.Errors["email"]
               .Should()
               .Contain("Email is required.");
        problem!.Errors.Should().ContainKey("password");
        problem.Errors["password"]
               .Should()
               .Contain("Password is required.")
               .And
               .Contain("Password must be at least 8 characters.");
        problem!.Errors.Should().ContainKey("confirmpassword");                
        problem.Errors["confirmpassword"]
               .Should()
               .Contain("Confirm password is required.")
               .And
               .Contain("Confirm password must be at least 8 characters.");
    }

    [Fact]
    public async Task Registration_Fails_With_Password_ConfirmPassword_Too_Short()
    {
        // Arrange
        string email = "email@example.com";
        string password = "RegPass";
        string confirmPassword = "RegPass";

        // Act
        HttpResponseMessage resp = await PostRegisterRawAsync(email, password, confirmPassword);

        // Assert
        resp.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

        ValidationProblemDetails? problem = await resp.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        problem.Should().NotBeNull();
        problem!.Errors.Should().ContainKey("password");
        problem.Errors["password"]
               .Should()               
               .Contain("Password must be at least 8 characters.");
        problem!.Errors.Should().ContainKey("confirmpassword");
        problem.Errors["confirmpassword"]
               .Should()
               .Contain("Confirm password must be at least 8 characters.");
    }
    
    [Fact]
    public async Task Registration_Fails_With_Password_ConfirmPassword_Not_Matching()
    {
        // Arrange
        string email = "badcode@example.com";
        string password = "RegPass!1";
        string confirmPassword = "RegPass!2";

        // Act
        HttpResponseMessage resp = await PostRegisterRawAsync(email, password, confirmPassword);

        // Assert
        resp.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

        ValidationProblemDetails? problem = await resp.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        problem.Should().NotBeNull();
        problem!.Errors.Should().ContainKey("confirmpassword");
        problem.Errors["confirmpassword"]
               .Should()
               .Contain("Passwords do not match.");
    }      

    public Task InitializeAsync() => _fixture.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private record BeginRegistrationResponse(string QrCodeBase64, string OtpAuthUri);
    private record ConfirmRegistrationResponse(bool Success, string Message);

    private Task<HttpResponseMessage> PostRegisterRawAsync(string email, string password, string confirmPassword)
       => _client.PostAsJsonAsync("/register", new { Email = email, Password = password, ConfirmPassword = confirmPassword, LastName = "Smith", FirstName = "John", PhoneNumber = "01924 4323432" });

    private Task<HttpResponseMessage> PostConfirmRegisterRawAsync(string email, string code)
       => _client.PostAsJsonAsync("/register/verify", new { Email = email, Code = code });
}
