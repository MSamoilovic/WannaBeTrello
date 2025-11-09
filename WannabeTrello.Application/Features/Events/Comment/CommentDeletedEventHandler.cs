using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Enums;
using WannabeTrello.Domain.Events.Comment_Events;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Events.Comment;

public class CommentDeletedEventHandler(
    ITaskNotificationService taskNotificationService,
    IActivityTrackerService activityTrackerService) : INotificationHandler<CommentDeletedEvent>
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