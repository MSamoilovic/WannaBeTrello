using MediatR;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Events.Project_Events;

namespace Feezbow.Application.Features.Events.Project;

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