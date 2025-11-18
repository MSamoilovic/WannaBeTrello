using MediatR;

namespace WannabeTrello.Application.Features.Auth.ForgotPassword;

public record ForgotPasswordCommand(string Email): IRequest<ForgotPasswordCommandResponse>;
