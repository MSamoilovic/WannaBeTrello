using MediatR;

namespace WannabeTrello.Application.Features.Auth.ChangePassword;

public record ChangePasswordCommand(string OldPassword, string NewPassword, string NewPasswordConfirmed): IRequest<ChangePasswordCommandResponse>;
