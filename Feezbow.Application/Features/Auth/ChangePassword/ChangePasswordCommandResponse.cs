using WannabeTrello.Domain.Entities.Common;

namespace WannabeTrello.Application.Features.Auth.ChangePassword;

public record ChangePasswordCommandResponse(Result<long> Result);
