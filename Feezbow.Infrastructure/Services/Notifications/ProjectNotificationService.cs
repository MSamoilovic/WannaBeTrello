using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Polly;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Infrastructure.SignalR.Contracts;
using Feezbow.Infrastructure.SignalR.Hubs;
using Feezbow.Infrastructure.SignalR.Resilience;

namespace Feezbow.Infrastructure.Services.Notifications;

public class ProjectNotificationService(
    IHubContext<ProjectHub, IProjectHubClient> projectHub,
    ResiliencePipeline pipeline,
    ILogger<ProjectNotificationService> logger)
    : ResilientNotificationBase(pipeline, logger), IProjectNotificationService
{
    public async Task NotifyProjectCreated(long createdProjectId, string? projectName, long creatorUserId)
    {
        await SendAsync(_ => new ValueTask(projectHub.Clients
            .Group($"Project:{createdProjectId}")
            .ProjectCreated(new ProjectCreatedNotification
            {
                ProjectId = createdProjectId,
                ProjectName = projectName ?? string.Empty,
                CreatedBy = creatorUserId
            })));
    }

    public async Task NotifyProjectUpdated(long modifiedProjectId, long modifierUserId)
    {
        await SendAsync(_ => new ValueTask(projectHub.Clients
            .Group($"Project:{modifiedProjectId}")
            .ProjectUpdated(new ProjectUpdatedNotification
            {
                ProjectId = modifiedProjectId,
                ModifiedBy = modifierUserId
            })));
    }

    public async Task NotifyProjectArchived(long projectId, long modifierUserId)
    {
        await SendAsync(_ => new ValueTask(projectHub.Clients
            .Group($"Project:{projectId}")
            .ProjectArchived(new ProjectArchivedNotification
            {
                ProjectId = projectId,
                ArchivedBy = modifierUserId
            })));
    }

    public async Task NotifyProjectMemberAdded(long projectId, long newMemberId, string? projectName, long inviterUserId)
    {
        await SendAsync(_ => new ValueTask(projectHub.Clients
            .Group($"Project:{projectId}")
            .MemberAdded(new ProjectMemberAddedNotification
            {
                ProjectId = projectId,
                MemberId = newMemberId,
                AddedBy = inviterUserId
            })));
    }

    public async Task NotifyProjectMemberRemoved(long projectId, long memberId, long modifierUserId)
    {
        await SendAsync(_ => new ValueTask(projectHub.Clients
            .Group($"Project:{projectId}")
            .MemberRemoved(new ProjectMemberRemovedNotification
            {
                ProjectId = projectId,
                MemberId = memberId,
                RemovedBy = modifierUserId
            })));
    }

    public async Task NotifyProjectMemberUpdated(long modifiedProjectId, long modifiedMemberId, long modifierUserId)
    {
        await SendAsync(_ => new ValueTask(projectHub.Clients
            .Group($"Project:{modifiedProjectId}")
            .MemberUpdated(new ProjectMemberUpdatedNotification
            {
                ProjectId = modifiedProjectId,
                MemberId = modifiedMemberId,
                ModifiedBy = modifierUserId
            })));
    }
}
