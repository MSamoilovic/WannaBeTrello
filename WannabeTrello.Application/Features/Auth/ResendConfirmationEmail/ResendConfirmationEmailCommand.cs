using MediatR;

namespace WannabeTrello.Application.Features.Auth.ResendConfirmationEmail
{
    public record ResendConfirmationEmailCommand(string Email)
    : IRequest<ResendConfirmationEmailCommandResponse>;
}
