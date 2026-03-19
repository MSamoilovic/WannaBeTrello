using MediatR;

namespace Feezbow.Application.Features.Auth.ChangePassword;

public record ChangePasswordCommand(string OldPassword, string NewPassword, string NewPasswordConfirmed): IRequest<ChangePasswordCommandResponse>;
