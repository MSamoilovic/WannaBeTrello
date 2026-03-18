using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Events.Project_Events;


namespace WannabeTrello.Application.Features.Events.Project;

public class ProjectCreatedEventHandler(
    IProjectNotificationService projectNotificationService)
    : INotificationHandler<ProjectCreatedEvent>
{
    public async Task Handle(ProjectCreatedEvent notification, CancellationToken cancellationToken)
    {
        await projectNotificationService.NotifyProjectCreated(notification.ProjectId,
            notification.ProjectName, notification.OwnerId);
    }
}