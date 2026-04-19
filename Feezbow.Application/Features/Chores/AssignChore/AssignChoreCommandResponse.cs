using Feezbow.Domain.Entities.Common;

namespace Feezbow.Application.Features.Chores.AssignChore;

public record AssignChoreCommandResponse(Result<bool> Result);
