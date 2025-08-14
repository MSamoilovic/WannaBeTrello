using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Events.Project_Events;

namespace WannabeTrello.Application.Features.Events.Project;

public class ProjectMemberAddedEventHandler(
    IProjectNotificationService projectNotificationService
): INotificationHandler<ProjectMemberAddedEvent>
{
    public async Task Handle(ProjectMemberAddedEvent notification, CancellationToken cancellationToken)
    {
        await projectNotificationService.NotifyProjectMemberAdded(
            notification.ProjectId,
            notification.NewMemberId,
            "",
            notification.InviterUserId
        );
    }
}