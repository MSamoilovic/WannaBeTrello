using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Feezbow.Infrastructure.SignalR.Authorization;
using Feezbow.Infrastructure.SignalR.Contracts;
using Feezbow.Infrastructure.SignalR.Hubs.Base;
using Feezbow.Infrastructure.SignalR.Services;

namespace Feezbow.Infrastructure.SignalR.Hubs;

/// <summary>
/// Hub for project-level real-time communication.
/// Clients join project groups to receive project and member events.
/// </summary>
public class ProjectHub(
    ILogger<ProjectHub> logger,
    IAuthorizationService authorizationService,
    IConnectionManager connectionManager,
    IHubGroupManager groupManager,
    IPresenceTracker presenceTracker)
    : AuthorizedHub<IProjectHubClient>(logger, connectionManager, groupManager, presenceTracker)
{
    /// <summary>
    /// Subscribes the caller to all real-time events for a project.
    /// Only project members are allowed to join.
    /// </summary>
    [HubMethod(RequiresAudit = true, Description = "Join a project's real-time group")]
    public async Task JoinProjectAsync(long projectId)
    {
        var result = await authorizationService.AuthorizeAsync(
            Context.User!, null, new ProjectAccessRequirement(projectId));

        if (!result.Succeeded)
            throw new HubException("Not authorized to join this project group.");

        var group = ProjectGroup(projectId);
        await Groups.AddToGroupAsync(Context.ConnectionId, group);
        await GroupManager.TrackGroupMembershipAsync(Context.ConnectionId, group);

        logger.LogInformation(
            "User {UserId} joined {Group} ({Count} viewers)",
            GetCurrentUserId(), group,
            await GroupManager.GetConnectionCountInGroupAsync(group));
    }

    /// <summary>
    /// Unsubscribes the caller from a project's real-time events.
    /// </summary>
    [HubMethod(Description = "Leave a project's real-time group")]
    public async Task LeaveProjectAsync(long projectId)
    {
        var group = ProjectGroup(projectId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, group);
        await GroupManager.UntrackGroupMembershipAsync(Context.ConnectionId, group);

        if (TryGetCurrentUserId(out long userId))
            logger.LogInformation("User {UserId} left {Group}", userId, group);
    }
}
