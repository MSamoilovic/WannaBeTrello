using MediatR;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Events.TaskEvents;
using Feezbow.Domain.Interfaces.Repositories;

namespace Feezbow.Application.Features.Events.BoardTask;

internal class TaskUpdatedEventHandler(
    ITaskNotificationService taskNotificationService,
    IBoardTaskRepository boardTaskRepository)
    : INotificationHandler<TaskUpdatedEvent>
{
    public async Task Handle(TaskUpdatedEvent notification, CancellationToken cancellationToken)
    {
        var boardId = await boardTaskRepository.GetBoardIdByTaskIdAsync(notification.TaskId, cancellationToken);
        await taskNotificationService.NotifyTaskUpdated(
            notification.TaskId,
            boardId,
            notification.TaskName,
            notification.ModifierUserId,
            notification.OldValues,
            notification.NewValues);
    }
}

