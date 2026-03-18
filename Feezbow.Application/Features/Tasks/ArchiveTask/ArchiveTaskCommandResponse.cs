using WannabeTrello.Domain.Entities.Common;

namespace WannabeTrello.Application.Features.Tasks.ArchiveTask;

public record ArchiveTaskCommandResponse(Result<long> Result);