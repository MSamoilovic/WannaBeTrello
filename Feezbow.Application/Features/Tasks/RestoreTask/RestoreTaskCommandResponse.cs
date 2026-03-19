using Feezbow.Domain.Entities.Common;

namespace Feezbow.Application.Features.Tasks.RestoreTask;

public record RestoreTaskCommandResponse(Result<long> Result);