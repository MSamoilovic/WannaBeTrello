using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Events.Comment_Events;

namespace WannabeTrello.Application.Features.Events.Comment;

public class CommentDeletedEventHandler(
    ITaskNotificationService taskNotificationService) : INotificationHandler<CommentDeletedEvent>
{
    public async Task Handle(CommentDeletedEvent notification, CancellationToken cancellationToken)
    {
        await taskNotificationService.NotifyCommentDeleted(
            notification.TaskId,
            notification.CommentId,
            notification.ModifyingUserId,
            cancellationToken);
    }
}