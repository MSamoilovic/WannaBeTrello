namespace WannabeTrello.Application.Features.Auth.LoginUser;

public record LoginUserCommandResponse(string Token, string? Email, bool IsConfirmed);
