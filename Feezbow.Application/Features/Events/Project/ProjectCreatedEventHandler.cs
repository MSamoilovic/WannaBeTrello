using MediatR;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Events.Project_Events;


namespace Feezbow.Application.Features.Events.Project;

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