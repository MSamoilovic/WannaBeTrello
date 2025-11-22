namespace WannabeTrello.Application.Features.Auth.RegisterUser;

public record RegisterUserCommandResponse(string Token, string Email, bool EmailConfirmed);