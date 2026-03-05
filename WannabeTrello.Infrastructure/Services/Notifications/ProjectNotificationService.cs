using Microsoft.AspNetCore.SignalR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Infrastructure.SignalR.Contracts;
using WannabeTrello.Infrastructure.SignalR.Hubs;

namespace WannabeTrello.Infrastructure.Services.Notifications;

public class ProjectNotificationService(
    IHubContext<ProjectHub, IProjectHubClient> projectHub) : IProjectNotificationService
{
    public async Task NotifyProjectCreated(long createdProjectId, string? projectName, long creatorUserId)
    {
        await projectHub.Clients
            .Group($"Project:{createdProjectId}")
            .ProjectCreated(new ProjectCreatedNotification
            {
                ProjectId = createdProjectId,
                ProjectName = projectName ?? string.Empty,
                CreatedBy = creatorUserId
            });
    }

    public async Task NotifyProjectUpdated(long modifiedProjectId, long modifierUserId)
    {
        await projectHub.Clients
            .Group($"Project:{modifiedProjectId}")
            .ProjectUpdated(new ProjectUpdatedNotification
            {
                ProjectId = modifiedProjectId,
                ModifiedBy = modifierUserId
            });
    }

    public async Task NotifyProjectArchived(long projectId, long modifierUserId)
    {
        await projectHub.Clients
            .Group($"Project:{projectId}")
            .ProjectArchived(new ProjectArchivedNotification
            {
                ProjectId = projectId,
                ArchivedBy = modifierUserId
            });
    }

    public async Task NotifyProjectMemberAdded(long projectId, long newMemberId, string? projectName, long inviterUserId)
    {
        await projectHub.Clients
            .Group($"Project:{projectId}")
            .MemberAdded(new ProjectMemberAddedNotification
            {
                ProjectId = projectId,
                MemberId = newMemberId,
                AddedBy = inviterUserId
            });
    }

    public async Task NotifyProjectMemberRemoved(long projectId, long memberId, long modifierUserId)
    {
        await projectHub.Clients
            .Group($"Project:{projectId}")
            .MemberRemoved(new ProjectMemberRemovedNotification
            {
                ProjectId = projectId,
                MemberId = memberId,
                RemovedBy = modifierUserId
            });
    }

    public async Task NotifyProjectMemberUpdated(long modifiedProjectId, long modifiedMemberId, long modifierUserId)
    {
        await projectHub.Clients
            .Group($"Project:{modifiedProjectId}")
            .MemberUpdated(new ProjectMemberUpdatedNotification
            {
                ProjectId = modifiedProjectId,
                MemberId = modifiedMemberId,
                ModifiedBy = modifierUserId
            });
    }
}