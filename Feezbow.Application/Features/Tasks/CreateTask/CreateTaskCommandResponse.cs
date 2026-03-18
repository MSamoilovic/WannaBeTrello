using WannabeTrello.Domain.Entities.Common;

namespace WannabeTrello.Application.Features.Tasks.CreateTask;

public record CreateTaskCommandResponse(Result<long> Result);
