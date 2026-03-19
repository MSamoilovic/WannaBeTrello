using MediatR;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Events.Project_Events;

namespace Feezbow.Application.Features.Events.Project;

public class ProjectMemberAddedEventHandler(
    IProjectNotificationService projectNotificationService
): INotificationHandler<ProjectMemberAddedEvent>
{
    public async Task Handle(ProjectMemberAddedEvent notification, CancellationToken cancellationToken)
    {
        await projectNotificationService.NotifyProjectMemberAdded(
            notification.ProjectId,
            notification.NewMemberId,
            notification.ProjectName,
            notification.InviterUserId
        );
    }
}