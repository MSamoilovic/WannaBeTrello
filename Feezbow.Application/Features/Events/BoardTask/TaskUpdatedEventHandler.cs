using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Events.TaskEvents;
using WannabeTrello.Domain.Interfaces.Repositories;

namespace WannabeTrello.Application.Features.Events.BoardTask;

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

