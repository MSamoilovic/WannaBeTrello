namespace WannabeTrello.Application.Features.Auth.ConfirmEmail;

public record ConfirmEmailCommandResponse(
    bool Success,
    string Message,
    string Token
);
