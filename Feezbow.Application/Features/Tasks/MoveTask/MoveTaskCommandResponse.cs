using WannabeTrello.Domain.Entities.Common;

namespace WannabeTrello.Application.Features.Tasks.MoveTask;

public record MoveTaskCommandResponse(Result<long> Result);