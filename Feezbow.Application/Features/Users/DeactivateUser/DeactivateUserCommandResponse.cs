using Feezbow.Domain.Entities.Common;

namespace Feezbow.Application.Features.Users.DeactivateUser;

public record DeactivateUserCommandResponse(Result<long> Result);
