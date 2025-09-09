using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Enums;
using WannabeTrello.Domain.Events.Project_Events;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Events.Project;

public class ProjectMemberAddedEventHandler(
    IProjectNotificationService projectNotificationService,
    IActivityTrackerService activityTrackerService
): INotificationHandler<ProjectMemberAddedEvent>
{
    public async Task Handle(ProjectMemberAddedEvent notification, CancellationToken cancellationToken)
    {
        await projectNotificationService.NotifyProjectMemberAdded(
            notification.ProjectId,
            notification.NewMemberId,
            notification.ProjectName,
            notification.InviterUserId
        );
        
        var activity = ActivityTracker.Create(
            type: ActivityType.ProjectMemberAdded,
            description: $"User {notification.NewMemberId} was added to project '{notification.ProjectName}' with role {notification.Role}.",
            userId: notification.InviterUserId, 
            relatedEntityId: notification.ProjectId,
            relatedEntityType: "Project",
            newValue: new Dictionary<string, object?> { { "NewMemberId", notification.NewMemberId }, { "NewRole", notification.Role } }
        );
        
        await activityTrackerService.AddActivityAsync(activity, cancellationToken);
    }
}