using MediatR;

namespace WannabeTrello.Application.Features.Auth.ConfirmEmail;

public record ConfirmEmailCommand(
string Email,
string Token
) : IRequest<ConfirmEmailCommandResponse>;
