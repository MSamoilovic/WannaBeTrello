using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Enums;
using WannabeTrello.Domain.Events.Project_Events;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Events.Project;

public class ProjectCreatedEventHandler(
    IProjectNotificationService projectNotificationService,
    IActivityTrackerService activityTrackerService)
    : INotificationHandler<ProjectCreatedEvent>
{
    public async Task Handle(ProjectCreatedEvent notification, CancellationToken cancellationToken)
    {
        await projectNotificationService.NotifyProjectCreated(notification.ProjectId,
            notification.ProjectName, notification.OwnerId);
        
        var initialValues = new Dictionary<string, object?>
        {
            { "Name", notification.ProjectName },
            { "Description", notification.ProjectDescription },
            { "OwnerId", notification.OwnerId },
            { "ProjectStatus", ProjectStatus.Active },
            { "IsArchived", false }
        };

        var activity = ActivityTracker.Create(
            type: ActivityType.ProjectCreated,
            description: $"Project '{notification.ProjectName}' is created.",
            userId: notification.OwnerId,
            relatedEntityId: notification.ProjectId,
            relatedEntityType: nameof(WannabeTrello.Domain.Entities.Project),
            newValue: initialValues
        );

        await activityTrackerService.AddActivityAsync(activity, cancellationToken);
    }
}