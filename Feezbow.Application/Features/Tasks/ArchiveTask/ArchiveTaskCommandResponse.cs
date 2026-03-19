using Feezbow.Domain.Entities.Common;

namespace Feezbow.Application.Features.Tasks.ArchiveTask;

public record ArchiveTaskCommandResponse(Result<long> Result);