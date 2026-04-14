using MediatR;

namespace Feezbow.Application.Features.Tasks.ClearTaskRecurrence;

public record ClearTaskRecurrenceCommand(long TaskId) : IRequest<ClearTaskRecurrenceCommandResponse>;
