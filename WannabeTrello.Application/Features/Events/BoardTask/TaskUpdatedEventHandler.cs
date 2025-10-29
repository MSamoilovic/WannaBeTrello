using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Events.TaskEvents;

namespace WannabeTrello.Application.Features.Events.BoardTask;

internal class TaskUpdatedEventHandler(ITaskNotificationService taskNotificationService)
    : INotificationHandler<TaskUpdatedEvent>
{
    public async Task Handle(TaskUpdatedEvent notification, CancellationToken cancellationToken)
    {
        await taskNotificationService.NotifyTaskUpdated(
            notification.TaskId,
            notification.TaskName,
            notification.ModifierUserId,
            notification.OldValues,
            notification.NewValues);
    }
}

