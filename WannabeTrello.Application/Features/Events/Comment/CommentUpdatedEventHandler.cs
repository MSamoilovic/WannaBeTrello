using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Events.Comment_Events;
using WannabeTrello.Domain.Interfaces.Repositories;

namespace WannabeTrello.Application.Features.Events.Comment;

public class CommentUpdatedEventHandler(
    ITaskNotificationService taskNotificationService,
    IBoardTaskRepository boardTaskRepository) : INotificationHandler<CommentUpdatedEvent>
{
    public async Task Handle(CommentUpdatedEvent notification, CancellationToken cancellationToken)
    {
        var boardId = await boardTaskRepository.GetBoardIdByTaskIdAsync(notification.TaskId, cancellationToken);
        await taskNotificationService.NotifyCommentUpdated(
            notification.TaskId,
            boardId,
            notification.CommentId,
            notification.ModifyingUserId,
            notification.OldContent,
            notification.NewContent,
            cancellationToken);
    }
}

