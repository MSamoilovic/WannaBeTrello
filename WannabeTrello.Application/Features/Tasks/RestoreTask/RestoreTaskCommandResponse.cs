using WannabeTrello.Domain.Entities.Common;

namespace WannabeTrello.Application.Features.Tasks.RestoreTask;

public record RestoreTaskCommandResponse(Result<long> Result);