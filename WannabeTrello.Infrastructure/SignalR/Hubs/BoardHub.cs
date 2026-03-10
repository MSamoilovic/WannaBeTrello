using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using WannabeTrello.Infrastructure.SignalR.Authorization;
using WannabeTrello.Infrastructure.SignalR.Contracts;
using WannabeTrello.Infrastructure.SignalR.Hubs.Base;
using WannabeTrello.Infrastructure.SignalR.Services;

namespace WannabeTrello.Infrastructure.SignalR.Hubs;

/// <summary>
/// Hub for board-level real-time communication.
/// Clients join board groups to receive board, column, task, and comment events.
/// </summary>
public class BoardHub(
    ILogger<BoardHub> logger,
    IAuthorizationService authorizationService,
    IConnectionManager connectionManager,
    IHubGroupManager groupManager,
    IPresenceTracker presenceTracker)
    : AuthorizedHub<IBoardHubClient>(logger, connectionManager, groupManager, presenceTracker)
{
    /// <summary>
    /// Subscribes the caller to all real-time events for a board.
    /// Only board members are allowed to join.
    /// </summary>
    [HubMethod(RequiresAudit = true, Description = "Join a board's real-time group")]
    public async Task JoinBoardAsync(long boardId)
    {
        var result = await authorizationService.AuthorizeAsync(
            Context.User!, null, new BoardAccessRequirement(boardId));

        if (!result.Succeeded)
            throw new HubException("Not authorized to join this board group.");

        var group = BoardGroup(boardId);
        await Groups.AddToGroupAsync(Context.ConnectionId, group);
        await GroupManager.TrackGroupMembershipAsync(Context.ConnectionId, group);

        logger.LogInformation(
            "User {UserId} joined {Group} ({Count} viewers)",
            GetCurrentUserId(), group,
            await GroupManager.GetConnectionCountInGroupAsync(group));
    }

    /// <summary>
    /// Unsubscribes the caller from a board's real-time events.
    /// </summary>
    [HubMethod(Description = "Leave a board's real-time group")]
    public async Task LeaveBoardAsync(long boardId)
    {
        var group = BoardGroup(boardId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, group);
        await GroupManager.UntrackGroupMembershipAsync(Context.ConnectionId, group);

        if (TryGetCurrentUserId(out long userId))
            logger.LogInformation("User {UserId} left {Group}", userId, group);
    }
}
