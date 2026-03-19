using Feezbow.Domain.Entities.Common;

namespace Feezbow.Application.Features.Auth.ChangePassword;

public record ChangePasswordCommandResponse(Result<long> Result);
