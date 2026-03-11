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
        var boardId = await boardTaskRepository.GetBoardIdByTaskIdAsync(notification.TaskId, cancellationToken);
        await taskNotificationService.NotifyTaskAssigned(
            notification.TaskId,
            boardId,
            notification.OldAssigneeId,
            notification.NewAssigneeId,
            notification.AssignedByUserId,
            cancellationToken);
    }
}

