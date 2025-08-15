using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Events.Project_Events;

namespace WannabeTrello.Application.Features.Events.Project;

public class ProjectUpdatedEventHandler(IProjectNotificationService projectNotificationService)
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