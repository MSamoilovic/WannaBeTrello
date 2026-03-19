using MediatR;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Events.TaskEvents;
using Feezbow.Domain.Interfaces.Repositories;

namespace Feezbow.Application.Features.Events.BoardTask
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