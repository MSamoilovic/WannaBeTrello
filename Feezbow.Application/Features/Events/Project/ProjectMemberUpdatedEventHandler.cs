using MediatR;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Events.Project_Events;

namespace Feezbow.Application.Features.Events.Project;

public class ProjectMemberUpdatedEventHandler(
    IProjectNotificationService projectNotificationService) : INotificationHandler<ProjectMemberUpdatedEvent>
{
    public async Task Handle(ProjectMemberUpdatedEvent notification, CancellationToken cancellationToken)
    {
        await projectNotificationService.NotifyProjectMemberUpdated(
            notification.Id,
            notification.UpdatedMemberId,
            notification.InviterUserId
        );
    }
}