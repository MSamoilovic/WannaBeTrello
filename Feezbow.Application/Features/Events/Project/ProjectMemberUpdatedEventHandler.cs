using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Events.Project_Events;

namespace WannabeTrello.Application.Features.Events.Project;

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