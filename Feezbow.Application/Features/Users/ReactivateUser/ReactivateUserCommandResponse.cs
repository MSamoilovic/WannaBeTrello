using WannabeTrello.Domain.Entities.Common;

namespace WannabeTrello.Application.Features.Users.ReactivateUser;

public record ReactivateUserCommandResponse(Result<long> Result);
