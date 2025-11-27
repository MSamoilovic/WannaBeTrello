using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Events.Project_Events;

namespace WannabeTrello.Application.Features.Events.Project;

public class ProjectArchivedEventHandler(
    IProjectNotificationService projectNotificationService
): INotificationHandler<ProjectArchivedEvent>
{
    public async Task Handle(ProjectArchivedEvent notification, CancellationToken cancellationToken)
    {
        await projectNotificationService.NotifyProjectArchived(notification.ProjectId, notification.ModifierId);
    }
}