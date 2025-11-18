using MediatR;

namespace WannabeTrello.Application.Features.Auth.ResetPassword;

public record ResetPasswordCommand(string Email, string Token, string NewPassword, string ConfirmPassword): IRequest<ResetPasswordCommandResponse>;
