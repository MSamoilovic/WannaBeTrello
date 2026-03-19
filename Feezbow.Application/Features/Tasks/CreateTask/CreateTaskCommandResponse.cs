using Feezbow.Domain.Entities.Common;

namespace Feezbow.Application.Features.Tasks.CreateTask;

public record CreateTaskCommandResponse(Result<long> Result);
