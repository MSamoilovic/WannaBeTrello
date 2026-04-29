using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Polly;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Enums;
using Feezbow.Infrastructure.SignalR.Contracts;
using Feezbow.Infrastructure.SignalR.Hubs;
using Feezbow.Infrastructure.SignalR.Resilience;

namespace Feezbow.Infrastructure.Services.Notifications;

public class HouseholdNotificationService(
    IHubContext<ProjectHub, IProjectHubClient> projectHub,
    ResiliencePipeline pipeline,
    ILogger<HouseholdNotificationService> logger)
    : ResilientNotificationBase(pipeline, logger), IHouseholdNotificationService
{
    private static string Group(long projectId) => $"Project:{projectId}";

    public Task NotifyProfileCreated(long householdId, long projectId, long createdBy, CancellationToken cancellationToken = default)
        => SendAsync(_ => new ValueTask(projectHub.Clients.Group(Group(projectId))
            .HouseholdProfileCreated(new HouseholdProfileCreatedNotification
            {
                HouseholdId = householdId, ProjectId = projectId, CreatedBy = createdBy
            })), cancellationToken);

    public Task NotifyProfileUpdated(long householdId, long projectId, long modifiedBy, IReadOnlyDictionary<string, object?> changes, CancellationToken cancellationToken = default)
        => SendAsync(_ => new ValueTask(projectHub.Clients.Group(Group(projectId))
            .HouseholdProfileUpdated(new HouseholdProfileUpdatedNotification
            {
                HouseholdId = householdId, ProjectId = projectId, ModifiedBy = modifiedBy, Changes = changes
            })), cancellationToken);

    public Task NotifyMemberRoleAssigned(long projectId, long memberId, HouseholdRole role, HouseholdRole? previousRole, long assignedBy, CancellationToken cancellationToken = default)
        => SendAsync(_ => new ValueTask(projectHub.Clients.Group(Group(projectId))
            .HouseholdMemberRoleAssigned(new HouseholdMemberRoleAssignedNotification
            {
                ProjectId = projectId, MemberId = memberId, Role = role, PreviousRole = previousRole, AssignedBy = assignedBy
            })), cancellationToken);

    public Task NotifyMemberRoleRemoved(long projectId, long memberId, HouseholdRole previousRole, long removedBy, CancellationToken cancellationToken = default)
        => SendAsync(_ => new ValueTask(projectHub.Clients.Group(Group(projectId))
            .HouseholdMemberRoleRemoved(new HouseholdMemberRoleRemovedNotification
            {
                ProjectId = projectId, MemberId = memberId, PreviousRole = previousRole, RemovedBy = removedBy
            })), cancellationToken);
}
