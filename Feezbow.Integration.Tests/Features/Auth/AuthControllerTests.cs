using System.Net;
using System.Net.Http.Json;
using Feezbow.Application.Features.Auth.LoginUser;
using Feezbow.Application.Features.Auth.RegisterUser;
using Feezbow.Integration.Tests.Infrastructure;

namespace Feezbow.Integration.Tests.Features.Auth;

[Collection("Integration")]
public class AuthControllerTests(IntegrationTestFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task Register_WithValidData_ReturnsSuccess()
    {
        // Arrange — avoid conflict with seed data user (username: testuser, email: test@example.com)
        var request = new RegisterUserCommand("newuser_register", "newuser_register@example.com", "Test@1234!");

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var request = new LoginUserCommand("nonexistent_xyz@example.com", "WrongPassword1!");

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/auth/login", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task HealthCheck_ReturnsHealthy()
    {
        // Act
        var response = await Client.GetAsync("/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
