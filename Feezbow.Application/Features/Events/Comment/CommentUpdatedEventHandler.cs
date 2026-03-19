using MediatR;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Events.Comment_Events;
using Feezbow.Domain.Interfaces.Repositories;

namespace Feezbow.Application.Features.Events.Comment;

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

