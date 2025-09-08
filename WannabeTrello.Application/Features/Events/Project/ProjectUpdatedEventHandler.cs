using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Enums;
using WannabeTrello.Domain.Events.Project_Events;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Events.Project;

public class ProjectUpdatedEventHandler(
    IActivityTrackerService activityTrackerService,
    IProjectNotificationService projectNotificationService)
    : INotificationHandler<ProjectUpdatedEvent>
{
    public async Task Handle(ProjectUpdatedEvent notification, CancellationToken cancellationToken)
    {
        await projectNotificationService.NotifyProjectUpdated(
            notification.Id,
            notification.ModifierId
        );

        var activity = ActivityTracker.Create(
            type: ActivityType.ProjectUpdated,
            description: $"Projekat '{notification.Name}' je ažuriran.",
            userId: notification.ModifierId,
            relatedEntityId: notification.Id,
            relatedEntityType: "Project",
            oldValue: notification.OldValue, 
            newValue: notification.NewValue
        );

        await activityTrackerService.AddActivityAsync(activity,cancellationToken );
    }
}