namespace WannabeTrello.Application.Features.Auth.ResendConfirmationEmail;

public record ResendConfirmationEmailCommandResponse(
    bool Success,
    string Message
 );
