using Feezbow.Domain.Entities.Common;

namespace Feezbow.Application.Features.Users.ReactivateUser;

public record ReactivateUserCommandResponse(Result<long> Result);
