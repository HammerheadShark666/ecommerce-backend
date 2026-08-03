using System.Net.Http.Json;
using ECommerce.IntegrationTests.Library;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace ECommerce.IntegrationTests.Registration;

[Collection("Database")]
public class BeginRegistrationFailIntegrationTest : IAsyncLifetime
{
    private readonly SqlServerFixture _fixture;
    private readonly TestApplicationFactory _appFactory; 
    private readonly HttpClient _client;

    public BeginRegistrationFailIntegrationTest(SqlServerFixture fixture)
    {
        _fixture = fixture; 
        _appFactory = new TestApplicationFactory(_fixture.ConnectionString);
        _client = _appFactory.CreateClient();
    }
    
    [Fact]
    public async Task VerifyRegistration_WithInvalidCode_ReturnsUnauthorized()
    {
        // Arrange
        var email = "badcode@example.com";
        var password = "RegPass!1";

        // Act
        var beginResp = await PostRegisterRawAsync(email, password, password);
        beginResp.EnsureSuccessStatusCode();

        // Act
        var verifyResp = await PostConfirmRegisterRawAsync(email, "000000");

        // Assert
        verifyResp.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest); 
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
        var password = "RegPass!1";

        //Act
        var resp = await PostRegisterRawAsync(invalidEmail, password, password);

        //Assert
        resp.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

        var problem = await resp.Content.ReadFromJsonAsync<ValidationProblemDetails>();

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
        var email = "";
        var password = "";
        var confirmPassword = "";

        // Act
        var resp = await PostRegisterRawAsync(email, password, confirmPassword);

        // Assert
        resp.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

        var problem = await resp.Content.ReadFromJsonAsync<ValidationProblemDetails>();

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
        var email = "email@example.com";
        var password = "RegPass";
        var confirmPassword = "RegPass";

        // Act
        var resp = await PostRegisterRawAsync(email, password, confirmPassword);

        // Assert
        resp.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

        var problem = await resp.Content.ReadFromJsonAsync<ValidationProblemDetails>();

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
        var email = "badcode@example.com";
        var password = "RegPass!1";
        var confirmPassword = "RegPass!2";

        // Act
        var resp = await PostRegisterRawAsync(email, password, confirmPassword);

        // Assert
        resp.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

        var problem = await resp.Content.ReadFromJsonAsync<ValidationProblemDetails>();

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
       => _client.PostAsJsonAsync("/register/verify-email", new { Email = email, Code = code });
}
