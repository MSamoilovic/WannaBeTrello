using System.Net;
using System.Net.Http.Json;
using Feezbow.Application.Features.Auth.RegisterUser;
using Feezbow.Integration.Tests.Infrastructure;

namespace Feezbow.Integration.Tests.Features.Auth;

public class AuthControllerTests : IntegrationTestBase
{
    // [Fact]
    // public async Task Register_WithValidData_ReturnsSuccess()
    // {
    //     // Arrange
    //     var request = new RegisterUserCommand("testuser", "testuser@example.com", "Test@1234!");

    //     // Act
    //     var response = await Client.PostAsJsonAsync("/api/v1/auth/register", request);

    //     // Assert
    //     Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    // }

    // [Fact]
    // public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    // {
    //     // Arrange
    //     var request = new { Email = "nonexistent@example.com", Password = "WrongPassword1!" };

    //     // Act
    //     var response = await Client.PostAsJsonAsync("/api/v1/auth/login", request);

    //     // Assert
    //     Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    // }

    [Fact]
    public async Task HealthCheck_ReturnsHealthy()
    {
        // Act
        var response = await Client.GetAsync("/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
