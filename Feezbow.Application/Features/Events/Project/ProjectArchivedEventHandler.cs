using MediatR;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Events.Project_Events;

namespace Feezbow.Application.Features.Events.Project;

public class ProjectArchivedEventHandler(
    IProjectNotificationService projectNotificationService
): INotificationHandler<ProjectArchivedEvent>
{
    public async Task Handle(ProjectArchivedEvent notification, CancellationToken cancellationToken)
    {
        await projectNotificationService.NotifyProjectArchived(notification.ProjectId, notification.ModifierId);
    }
}