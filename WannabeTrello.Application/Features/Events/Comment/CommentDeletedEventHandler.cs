using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Events.Comment_Events;
using WannabeTrello.Domain.Interfaces.Repositories;

namespace WannabeTrello.Application.Features.Events.Comment;

public class CommentDeletedEventHandler(
    ITaskNotificationService taskNotificationService,
    IBoardTaskRepository boardTaskRepository) : INotificationHandler<CommentDeletedEvent>
{
    public async Task Handle(CommentDeletedEvent notification, CancellationToken cancellationToken)
    {
        var boardId = await boardTaskRepository.GetBoardIdByTaskIdAsync(notification.TaskId, cancellationToken);
        await taskNotificationService.NotifyCommentDeleted(
            notification.TaskId,
            boardId,
            notification.CommentId,
            notification.ModifyingUserId,
            cancellationToken);
    }
}