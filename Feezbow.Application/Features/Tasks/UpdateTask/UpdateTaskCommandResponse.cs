using WannabeTrello.Domain.Entities.Common;

namespace WannabeTrello.Application.Features.Tasks.UpdateTask;

public record UpdateTaskCommandResponse(Result<long> Result);