using MediatR;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities.Common;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces;
using Feezbow.Domain.Services;
using Feezbow.Domain.ValueObjects;

namespace Feezbow.Application.Features.Tasks.SetTaskRecurrence;

public class SetTaskRecurrenceCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService)
    : IRequestHandler<SetTaskRecurrenceCommand, SetTaskRecurrenceCommandResponse>
{
    public async Task<SetTaskRecurrenceCommandResponse> Handle(
        SetTaskRecurrenceCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var userId = currentUserService.UserId ?? 0;

        var task = await unitOfWork.Tasks.GetTaskDetailsByIdAsync(request.TaskId, cancellationToken)
            ?? throw new NotFoundException("Task", request.TaskId);

        var rule = RecurrenceRule.Create(
            request.Frequency,
            request.Interval,
            request.DaysOfWeek,
            request.EndDate);

        var firstOccurrence = task.DueDate.HasValue
            ? RecurringTaskScheduler.CalculateNext(task.DueDate.Value, rule)
            : RecurringTaskScheduler.CalculateNext(DateTime.UtcNow, rule);

        task.SetRecurrence(rule, firstOccurrence, userId);

        await unitOfWork.CompleteAsync(cancellationToken);

        var result = Result<DateTime?>.Success(firstOccurrence, "Recurrence set successfully.");
        return new SetTaskRecurrenceCommandResponse(result);
    }
}
