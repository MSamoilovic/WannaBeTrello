using WannabeTrello.Domain.Entities.Common;

namespace WannabeTrello.Application.Features.Tasks.AssignTaskToUser;

public record AssignTaskToUserCommandResponse(Result<long> Result);