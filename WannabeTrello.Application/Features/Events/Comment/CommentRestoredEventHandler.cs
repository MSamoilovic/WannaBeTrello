using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Events.Comment_Events;
using WannabeTrello.Domain.Interfaces.Repositories;

namespace WannabeTrello.Application.Features.Events.Comment;

public class CommentRestoredEventHandler(
    ITaskNotificationService taskNotificationService,
    IBoardTaskRepository boardTaskRepository) : INotificationHandler<CommentRestoredEvent>
{
    public async Task Handle(CommentRestoredEvent notification, CancellationToken cancellationToken)
    {
        var boardId = await boardTaskRepository.GetBoardIdByTaskIdAsync(notification.TaskId, cancellationToken);
        await taskNotificationService.NotifyCommentRestored(
            notification.TaskId,
            boardId,
            notification.CommentId,
            notification.ModifyingUserId,
            cancellationToken);
    }
}

