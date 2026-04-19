using Feezbow.Domain.Entities.Common;

namespace Feezbow.Application.Features.Chores.CreateChore;

public record CreateChoreCommandResponse(Result<long> Result);
