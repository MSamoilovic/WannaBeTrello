using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Enums;
using WannabeTrello.Domain.Events.Comment_Events;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Events.Comment;

public class CommentRestoredEventHandler(
    ITaskNotificationService taskNotificationService,
    IActivityTrackerService activityTrackerService) : INotificationHandler<CommentRestoredEvent>
{
    public async Task Handle(CommentRestoredEvent notification, CancellationToken cancellationToken)
    {
        // Send notification via SignalR
        await taskNotificationService.NotifyCommentRestored(
            notification.TaskId,
            notification.CommentId,
            notification.ModifyingUserId,
            cancellationToken);

        // Create activity tracker record
        var activity = ActivityTracker.Create(
            type: ActivityType.CommentRestored,
            description: $"Comment '{notification.CommentId}' was restored on task '{notification.TaskId}'.",
            userId: notification.ModifyingUserId,
            relatedEntityId: notification.CommentId,
            relatedEntityType: nameof(Domain.Entities.Comment));

        await activityTrackerService.AddActivityAsync(activity, cancellationToken);
    }
}

