using MediatR;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Events.Comment_Events;
using Feezbow.Domain.Interfaces.Repositories;

namespace Feezbow.Application.Features.Events.Comment;

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