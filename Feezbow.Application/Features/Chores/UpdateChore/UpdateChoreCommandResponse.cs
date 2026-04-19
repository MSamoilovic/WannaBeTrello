using Feezbow.Domain.Entities.Common;

namespace Feezbow.Application.Features.Chores.UpdateChore;

public record UpdateChoreCommandResponse(Result<bool> Result);
