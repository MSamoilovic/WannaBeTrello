using MediatR;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Events.TaskEvents;
using Feezbow.Domain.Interfaces.Repositories;

namespace Feezbow.Application.Features.Events.BoardTask;

public class TaskMovedEventHandler(
    ITaskNotificationService taskNotificationService,
    IBoardTaskRepository boardTaskRepository)
    : INotificationHandler<TaskMovedEvent>
{
    public async Task Handle(TaskMovedEvent notification, CancellationToken cancellationToken)
    {
        var boardId = await boardTaskRepository.GetBoardIdByTaskIdAsync(notification.TaskId, cancellationToken);
        await taskNotificationService.NotifyTaskMoved(notification.TaskId, boardId, notification.NewColumnId,
            notification.PerformedByUserId, cancellationToken);
    }
}