namespace Feezbow.Application.Features.Auth.RegisterUser;

public record RegisterUserCommandResponse(string Email, bool EmailConfirmed);