using MediatR;
using Feezbow.Domain.Enums;

namespace Feezbow.Application.Features.Tasks.SetTaskRecurrence;

public record SetTaskRecurrenceCommand(
    long TaskId,
    RecurrenceFrequency Frequency,
    int Interval,
    IEnumerable<DayOfWeek>? DaysOfWeek,
    DateTime? EndDate) : IRequest<SetTaskRecurrenceCommandResponse>;
