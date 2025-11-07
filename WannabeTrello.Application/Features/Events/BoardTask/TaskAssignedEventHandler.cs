using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Events.TaskEvents;
using WannabeTrello.Domain.Interfaces.Repositories;

namespace WannabeTrello.Application.Features.Events.BoardTask;

internal class TaskAssignedEventHandler(
    ITaskNotificationService taskNotificationService,
    IBoardTaskRepository boardTaskRepository)
    : INotificationHandler<TaskAssignedEvent>
{
    public async Task Handle(TaskAssignedEvent notification, CancellationToken cancellationToken)
    {
        var task = await boardTaskRepository.GetTaskDetailsByIdAsync(notification.TaskId, cancellationToken);
        if (task?.Column == null)
        {
            return;
        }

        var boardId = task.Column.BoardId;

        await taskNotificationService.NotifyTaskAssigned(
            boardId,
            notification.TaskId,
            notification.OldAssigneeId,
            notification.NewAssigneeId,
            notification.AssignedByUserId,
            cancellationToken);
    }
}

