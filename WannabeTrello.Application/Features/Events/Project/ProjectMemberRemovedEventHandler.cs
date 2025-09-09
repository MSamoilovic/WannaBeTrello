using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Enums;
using WannabeTrello.Domain.Events.Project_Events;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Events.Project;

public class ProjectMemberRemovedEventHandler(
    IProjectNotificationService projectNotificationService,
    IActivityTrackerService activityTrackerService) : INotificationHandler<ProjectMemberRemovedEvent>
{
    public async Task Handle(ProjectMemberRemovedEvent notification, CancellationToken cancellationToken)
    {
        await projectNotificationService.NotifyProjectMemberRemoved(
            notification.ProjectId,
            notification.RemovedUserId,
            notification.RemovingUserId
        );
        
        var activity = ActivityTracker.Create(
            type: ActivityType.ProjectMemberRemoved,
            description: $"User {notification.RemovedUserId} is removed.",
            userId: notification.RemovingUserId,
            relatedEntityId: notification.ProjectId,
            relatedEntityType: "Project",
            oldValue: new Dictionary<string, object?> { { "RemovedUserId", notification.RemovedUserId }, { "RemovedUserRole", notification.RemovedUserRole } }
        );
        
        await activityTrackerService.AddActivityAsync(activity, cancellationToken);
    }
}