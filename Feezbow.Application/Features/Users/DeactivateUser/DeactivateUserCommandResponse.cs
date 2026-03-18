using WannabeTrello.Domain.Entities.Common;

namespace WannabeTrello.Application.Features.Users.DeactivateUser;

public record DeactivateUserCommandResponse(Result<long> Result);
