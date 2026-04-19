using Feezbow.Domain.Entities.Common;

namespace Feezbow.Application.Features.Chores.DeleteChore;

public record DeleteChoreCommandResponse(Result<bool> Result);
