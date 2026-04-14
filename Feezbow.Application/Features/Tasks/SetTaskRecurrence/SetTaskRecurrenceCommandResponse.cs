using Feezbow.Domain.Entities.Common;

namespace Feezbow.Application.Features.Tasks.SetTaskRecurrence;

public record SetTaskRecurrenceCommandResponse(Result<DateTime?> Result);
