using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Enums;
using WannabeTrello.Domain.Events.Project_Events;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Events.Project;

public class ProjectMemberUpdatedEventHandler(
    IProjectNotificationService projectNotificationService,
    IActivityTrackerService activityTrackerService) : INotificationHandler<ProjectMemberUpdatedEvent>
{
    public async Task Handle(ProjectMemberUpdatedEvent notification, CancellationToken cancellationToken)
    {
        await projectNotificationService.NotifyProjectMemberUpdated(
            notification.Id,
            notification.UpdatedMemberId,
            notification.InviterUserId
        );

        var activity = ActivityTracker.Create(
            type: ActivityType.ProjectMemberRoleUpdated,
            description:
            $"Role for Member {notification.UpdatedMemberId} is updated.",
            userId: notification.InviterUserId,
            relatedEntityId: notification.Id,
            relatedEntityType: "Project",
            oldValue: new Dictionary<string, object?> { { "OldRole", notification.OldRole } },
            newValue: new Dictionary<string, object?> { { "NewRole", notification.NewRole } }
        );

        await activityTrackerService.AddActivityAsync(activity, cancellationToken);
    }
}