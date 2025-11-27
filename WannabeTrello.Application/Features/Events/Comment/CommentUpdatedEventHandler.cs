using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Events.Comment_Events;

namespace WannabeTrello.Application.Features.Events.Comment;

public class CommentUpdatedEventHandler(
    ITaskNotificationService taskNotificationService) : INotificationHandler<CommentUpdatedEvent>
{
    public async Task Handle(CommentUpdatedEvent notification, CancellationToken cancellationToken)
    {
        await taskNotificationService.NotifyCommentUpdated(
            notification.TaskId,
            notification.CommentId,
            notification.ModifyingUserId,
            notification.OldContent,
            notification.NewContent,
            cancellationToken);
    }
}

