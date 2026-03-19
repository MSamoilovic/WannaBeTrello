using MediatR;

namespace Feezbow.Application.Features.Auth.ResendConfirmationEmail
{
    public record ResendConfirmationEmailCommand(string Email)
    : IRequest<ResendConfirmationEmailCommandResponse>;
}
