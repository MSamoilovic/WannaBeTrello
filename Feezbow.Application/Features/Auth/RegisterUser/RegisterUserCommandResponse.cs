namespace Feezbow.Application.Features.Auth.RegisterUser;

public record RegisterUserCommandResponse(string Token, string Email, bool EmailConfirmed, string RefreshToken, DateTime RefreshTokenExpiresAt);