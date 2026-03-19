using Feezbow.Domain.Entities.Common;

namespace Feezbow.Application.Features.Tasks.AssignTaskToUser;

public record AssignTaskToUserCommandResponse(Result<long> Result);