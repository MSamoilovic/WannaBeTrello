using MediatR;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Events.Comment_Events;
using Feezbow.Domain.Interfaces.Repositories;

namespace Feezbow.Application.Features.Events.Comment;

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

