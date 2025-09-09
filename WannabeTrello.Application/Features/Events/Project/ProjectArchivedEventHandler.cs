using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Enums;
using WannabeTrello.Domain.Events.Project_Events;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Events.Project;

public class ProjectArchivedEventHandler(
    IProjectNotificationService projectNotificationService,
    IActivityTrackerService  activityTrackerService
): INotificationHandler<ProjectArchivedEvent>
{
    public async Task Handle(ProjectArchivedEvent notification, CancellationToken cancellationToken)
    {
        await projectNotificationService.NotifyProjectArchived(notification.ProjectId, notification.ModifierId);
        
        var activity = ActivityTracker.Create(
            type: ActivityType.ProjectArchived,
            description: $"Project '{notification.ProjectName}' is archived.",
            userId: notification.ModifierId,
            relatedEntityId: notification.ProjectId,
            relatedEntityType: "Project");
        
        await activityTrackerService.AddActivityAsync(activity, cancellationToken);
    }
}