using Feezbow.Domain.Entities.Common;

namespace Feezbow.Application.Features.Tasks.MoveTask;

public record MoveTaskCommandResponse(Result<long> Result);