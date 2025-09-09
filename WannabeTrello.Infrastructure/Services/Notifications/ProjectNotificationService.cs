using Microsoft.AspNetCore.SignalR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Infrastructure.SignalR;

namespace WannabeTrello.Infrastructure.Services.Notifications;

public class ProjectNotificationService(IHubContext<TrellyHub, ITrellyHub> hubContext): IProjectNotificationService
{
    public async Task NotifyProjectCreated(long createdProjectId, string? projectName, long creatorUserId)
    {
        await hubContext.Clients.All.ProjectCreated(createdProjectId, projectName, creatorUserId);
    }

    public async Task NotifyProjectUpdated(long modifiedProjectId, long modifierUserId)
    {
        await hubContext.Clients.All.ProjectUpdated(modifiedProjectId, modifierUserId);
    }

    public async Task NotifyProjectArchived(long projectId, long modifierUserId)
    {
        await hubContext.Clients.All.ProjectArchived(projectId, modifierUserId);
    }

    public async Task NotifyProjectMemberAdded(long projectId, long newMemberId, string? projectName, long inviterUserId)
    {
        await hubContext.Clients.All.AddedProjectMember(projectId, newMemberId,  inviterUserId);
    }

    public async Task NotifyProjectMemberRemoved(long projectId, long memberId, long modifierUserId)
    {
        await hubContext.Clients.All.RemovedProjectMember(projectId, memberId, modifierUserId);
    }

    public async Task NotifyProjectMemberUpdated(long modifiedProjectId, long modifiedMemberId, long modifierUserId)
    {
        await hubContext.Clients.All.UpdatedProjectMember(modifiedProjectId, modifiedMemberId, modifierUserId);
    }
}