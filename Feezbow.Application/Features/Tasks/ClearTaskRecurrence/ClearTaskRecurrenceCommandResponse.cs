using Feezbow.Domain.Entities.Common;

namespace Feezbow.Application.Features.Tasks.ClearTaskRecurrence;

public record ClearTaskRecurrenceCommandResponse(Result<bool> Result);
