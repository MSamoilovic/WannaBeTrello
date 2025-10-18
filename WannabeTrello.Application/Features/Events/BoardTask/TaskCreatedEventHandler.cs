using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Events.TaskEvents;

namespace WannabeTrello.Application.Features.Events.BoardTask
{
    internal class TaskCreatedEventHandler(ITaskNotificationService taskNotificationService)
        : INotificationHandler<TaskCreatedEvent>
    {
        public async Task Handle(TaskCreatedEvent notification, CancellationToken cancellationToken)
        {
            await taskNotificationService.NotifyTaskCreated(notification.TaskId, notification.TaskTitle,
                notification.CreatorUserId, notification.AssigneeId);
        }
    }
}