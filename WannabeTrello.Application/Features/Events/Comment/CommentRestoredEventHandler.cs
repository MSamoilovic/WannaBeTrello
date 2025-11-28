using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Events.Comment_Events;

namespace WannabeTrello.Application.Features.Events.Comment;

public class CommentRestoredEventHandler(
    ITaskNotificationService taskNotificationService) : INotificationHandler<CommentRestoredEvent>
{
    public async Task Handle(CommentRestoredEvent notification, CancellationToken cancellationToken)
    {
        // Send notification via SignalR
        await taskNotificationService.NotifyCommentRestored(
            notification.TaskId,
            notification.CommentId,
            notification.ModifyingUserId,
            cancellationToken);
    }
}

