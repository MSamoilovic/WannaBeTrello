using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Events.TaskEvents;

namespace WannabeTrello.Application.Features.Events.BoardTask;

public class TaskMovedEventHandler(ITaskNotificationService taskNotificationService)
    : INotificationHandler<TaskMovedEvent>
{
    public async Task Handle(TaskMovedEvent notification, CancellationToken cancellationToken)
    {
        await taskNotificationService.NotifyTaskMoved(notification.TaskId, notification.NewColumnId,
            notification.PerformedByUserId, cancellationToken);
    }
}