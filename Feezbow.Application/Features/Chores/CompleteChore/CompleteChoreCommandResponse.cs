using Feezbow.Domain.Entities.Common;

namespace Feezbow.Application.Features.Chores.CompleteChore;

public record CompleteChoreCommandResponse(Result<long?> Result);
