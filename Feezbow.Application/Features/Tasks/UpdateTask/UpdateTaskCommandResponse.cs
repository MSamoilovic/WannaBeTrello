using Feezbow.Domain.Entities.Common;

namespace Feezbow.Application.Features.Tasks.UpdateTask;

public record UpdateTaskCommandResponse(Result<long> Result);