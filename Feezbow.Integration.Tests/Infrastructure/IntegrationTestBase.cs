using System.Net.Http.Headers;
using Feezbow.Application.Features.Auth.ConfirmEmail;
using Feezbow.Application.Features.Auth.LoginUser;
using Feezbow.Application.Features.Auth.RegisterUser;
using Feezbow.Application.Features.Users.GetCurrentUserProfile;
using Feezbow.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Feezbow.Integration.Tests.Infrastructure;

public abstract class IntegrationTestBase(IntegrationTestFixture fixture)
{
    protected HttpClient Client { get; } = fixture.CreateClient();
    protected IServiceProvider Services { get; } = fixture.Services;

    protected T GetService<T>() where T : notnull =>
        Services.CreateScope().ServiceProvider.GetRequiredService<T>();

    /// <summary>
    /// Registers a new user, confirms their email via UserManager, and logs in.
    /// Returns a valid JWT bearer token.
    /// </summary>
    protected async Task<string> RegisterAndLoginAsync(
        string? username = null,
        string? email = null,
        string password = "Test@1234!")
    {
        var id = Guid.NewGuid().ToString("N")[..8];
        username ??= $"user_{id}";
        email ??= $"{id}@example.com";

        var registerResponse = await Client.PostAsJsonAsync("/api/v1/auth/register",
            new RegisterUserCommand(username, email, password));
        registerResponse.EnsureSuccessStatusCode();

        using var scope = Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var user = await userManager.FindByEmailAsync(email)
            ?? throw new InvalidOperationException($"User '{email}' not found after registration.");
        var confirmationToken = await userManager.GenerateEmailConfirmationTokenAsync(user);

        var confirmResponse = await Client.PostAsJsonAsync("/api/v1/auth/confirm-email",
            new ConfirmEmailCommand(email, confirmationToken));
        confirmResponse.EnsureSuccessStatusCode();

        var loginResponse = await Client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginUserCommand(email, password));
        loginResponse.EnsureSuccessStatusCode();

        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginUserCommandResponse>()
            ?? throw new InvalidOperationException("Login response was null.");

        return loginResult.Token;
    }

    protected void SetAuthToken(string token) =>
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

    protected void ClearAuthToken() =>
        Client.DefaultRequestHeaders.Authorization = null;

    /// <summary>
    /// Fetches the current authenticated user's ID via GET /api/v1/user/me.
    /// Requires a valid token to be set with SetAuthToken before calling.
    /// </summary>
    protected async Task<long> GetCurrentUserIdAsync()
    {
        var response = await Client.GetAsync("/api/v1/user/me");
        response.EnsureSuccessStatusCode();
        var profile = await response.Content.ReadFromJsonAsync<GetCurrentUserProfileQueryResponse>()
            ?? throw new InvalidOperationException("Profile response was null.");
        return profile.Id;
    }
}
