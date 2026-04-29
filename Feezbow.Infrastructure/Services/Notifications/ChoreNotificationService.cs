using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Polly;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Infrastructure.SignalR.Contracts;
using Feezbow.Infrastructure.SignalR.Hubs;
using Feezbow.Infrastructure.SignalR.Resilience;

namespace Feezbow.Infrastructure.Services.Notifications;

public class ChoreNotificationService(
    IHubContext<ProjectHub, IProjectHubClient> projectHub,
    ResiliencePipeline pipeline,
    ILogger<ChoreNotificationService> logger)
    : ResilientNotificationBase(pipeline, logger), IChoreNotificationService
{
    private static string Group(long projectId) => $"Project:{projectId}";

    public Task NotifyChoreCreated(long choreId, long projectId, string title, long? assignedToUserId, long createdBy, CancellationToken cancellationToken = default)
        => SendAsync(_ => new ValueTask(projectHub.Clients.Group(Group(projectId))
            .ChoreCreated(new ChoreCreatedNotification
            {
                ChoreId = choreId, ProjectId = projectId, Title = title, AssignedToUserId = assignedToUserId, CreatedBy = createdBy
            })), cancellationToken);

    public Task NotifyChoreUpdated(long choreId, long projectId, long modifiedBy, CancellationToken cancellationToken = default)
        => SendAsync(_ => new ValueTask(projectHub.Clients.Group(Group(projectId))
            .ChoreUpdated(new ChoreUpdatedNotification
            {
                ChoreId = choreId, ProjectId = projectId, ModifiedBy = modifiedBy
            })), cancellationToken);

    public Task NotifyChoreAssigned(long choreId, long projectId, long? assignedToUserId, long? previousUserId, long assignedBy, CancellationToken cancellationToken = default)
        => SendAsync(_ => new ValueTask(projectHub.Clients.Group(Group(projectId))
            .ChoreAssigned(new ChoreAssignedNotification
            {
                ChoreId = choreId, ProjectId = projectId, AssignedToUserId = assignedToUserId, PreviousUserId = previousUserId, AssignedBy = assignedBy
            })), cancellationToken);

    public Task NotifyChoreCompleted(long choreId, long projectId, long completedBy, CancellationToken cancellationToken = default)
        => SendAsync(_ => new ValueTask(projectHub.Clients.Group(Group(projectId))
            .ChoreCompleted(new ChoreCompletedNotification
            {
                ChoreId = choreId, ProjectId = projectId, CompletedBy = completedBy
            })), cancellationToken);

    public Task NotifyChoreDeleted(long choreId, long projectId, long deletedBy, CancellationToken cancellationToken = default)
        => SendAsync(_ => new ValueTask(projectHub.Clients.Group(Group(projectId))
            .ChoreDeleted(new ChoreDeletedNotification
            {
                ChoreId = choreId, ProjectId = projectId, DeletedBy = deletedBy
            })), cancellationToken);
}
