using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Events.Project_Events;

namespace WannabeTrello.Application.Features.Events.Project;

public class ProjectMemberRemovedEventHandler(
    IProjectNotificationService projectNotificationService) : INotificationHandler<ProjectMemberRemovedEvent>
{
    public async Task Handle(ProjectMemberRemovedEvent notification, CancellationToken cancellationToken)
    {
        await projectNotificationService.NotifyProjectMemberRemoved(
            notification.ProjectId,
            notification.RemovedUserId,
            notification.RemovingUserId
        );
    }
}