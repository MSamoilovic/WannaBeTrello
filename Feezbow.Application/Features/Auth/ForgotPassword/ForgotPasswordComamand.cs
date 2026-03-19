using MediatR;

namespace Feezbow.Application.Features.Auth.ForgotPassword;

public record ForgotPasswordCommand(string Email): IRequest<ForgotPasswordCommandResponse>;
