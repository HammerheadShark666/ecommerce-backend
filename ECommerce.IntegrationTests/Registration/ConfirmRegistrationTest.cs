using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using ECommerce.IntegrationTests.Library;
using Xunit;

namespace ECommerce.IntegrationTests.Registration;

[Collection("Database")]
public class ConfirmRegistrationIntegrationTest : IAsyncLifetime
{
    private readonly SqlServerFixture _fixture;
    private readonly TestApplicationFactory _appFactory;    
    private readonly HttpClient _client;

    public ConfirmRegistrationIntegrationTest(SqlServerFixture fixture)
    {
        _fixture = fixture; 
        _appFactory = new TestApplicationFactory(_fixture.ConnectionString);
        _client = _appFactory.CreateClient();
    }

    [Fact]
    public async Task Email_WhenEmpty_ShouldHaveValidationError()
    {
        //Arrange
        string email = "";
        const string anyValidCode = "000000";

        //Act
        HttpResponseMessage resp = await PostConfirmRegisterRawAsync(email, anyValidCode);

        //Assert
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ValidationProblemDetails? problem = await resp.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        problem.Should().NotBeNull();
        problem!.Errors.Should().ContainKey("email");
        problem.Errors["email"]
               .Should()
               .Contain("Email is required.");
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("missing@")]
    [InlineData("@nodomain.com")]
    [InlineData("spaces in@email.com")]
    [InlineData("double@@domain.com")]
    public async Task Email_WhenInvalidFormat_ShouldHaveValidationError(string invalidEmail)
    {
        //Arrange
        const string anyValidCode = "000000";

        //Act
        HttpResponseMessage resp = await PostConfirmRegisterRawAsync(invalidEmail, anyValidCode);

        //Assert
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ValidationProblemDetails? problem = await resp.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        problem.Should().NotBeNull();
        problem!.Errors.Should().ContainKey("email");
        problem.Errors["email"]
               .Should()
               .Contain("Email is not valid.");
    }
     
    [Fact]
    public async Task Code_WhenEmpty_ShouldHaveValidationError()
    {
        //Arrange
        string email = "validemail@example.com";
        string code = "";

        //Act
        HttpResponseMessage resp = await PostConfirmRegisterRawAsync(email, code);

        //Assert
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ValidationProblemDetails? problem = await resp.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        problem.Should().NotBeNull();
        problem!.Errors.Should().ContainKey("code");
        problem.Errors["code"]
               .Should()
               .Contain("Code is required.");
    }

    [Theory]
    [InlineData("12345")]    // 5 chars — one below boundary
    [InlineData("1234567")]  // 7 chars — one above boundary
    [InlineData("1")]
    [InlineData("123456789012")]
    public async Task Code_WhenNotExactlySixCharacters_ShouldHaveValidationError(string badCode)
    {
        //Arrange
        string email = "validemail@example.com";

        //Act
        HttpResponseMessage resp = await PostConfirmRegisterRawAsync(email, badCode);

        //Assert
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ValidationProblemDetails? problem = await resp.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        problem.Should().NotBeNull();
        problem!.Errors.Should().ContainKey("code");
        problem.Errors["code"]
               .Should()
               .Contain("Code must be 6 characters.");
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => Task.CompletedTask; 
   
    private Task<HttpResponseMessage> PostConfirmRegisterRawAsync(string email, string code)
       => _client.PostAsJsonAsync("/register/verify", new { Email = email, Code = code });
}
