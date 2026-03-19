using MediatR;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Events.Project_Events;

namespace Feezbow.Application.Features.Events.Project;

public class ProjectUpdatedEventHandler(
    IProjectNotificationService projectNotificationService)
    : INotificationHandler<ProjectUpdatedEvent>
{
    public async Task Handle(ProjectUpdatedEvent notification, CancellationToken cancellationToken)
    {
        await projectNotificationService.NotifyProjectUpdated(
            notification.Id,
            notification.ModifierId
        );
    }
}