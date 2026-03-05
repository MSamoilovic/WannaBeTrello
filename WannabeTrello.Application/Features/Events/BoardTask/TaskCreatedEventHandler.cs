using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Events.TaskEvents;
using WannabeTrello.Domain.Interfaces.Repositories;

namespace WannabeTrello.Application.Features.Events.BoardTask
{
    internal class TaskCreatedEventHandler(
        ITaskNotificationService taskNotificationService,
        IBoardTaskRepository boardTaskRepository)
        : INotificationHandler<TaskCreatedEvent>
    {
        public async Task Handle(TaskCreatedEvent notification, CancellationToken cancellationToken)
        {
            var boardId = await boardTaskRepository.GetBoardIdByTaskIdAsync(notification.TaskId, cancellationToken);
            await taskNotificationService.NotifyTaskCreated(notification.TaskId, boardId, notification.TaskTitle,
                notification.CreatorUserId, notification.AssigneeId);
        }
    }
}