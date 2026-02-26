using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using WannabeTrello.Domain.Interfaces.Repositories;
using WannabeTrello.Infrastructure.SignalR.Contracts;
using WannabeTrello.Infrastructure.SignalR.Hubs.Base;

namespace WannabeTrello.Infrastructure.SignalR.Hubs;

/// <summary>
/// Hub for board-level real-time communication.
/// Clients join board groups to receive board, column, task, and comment events.
/// </summary>
public class BoardHub(ILogger<BoardHub> logger, IBoardRepository boardRepository)
    : AuthorizedHub<IBoardHubClient>(logger)
{
    /// <summary>
    /// Subscribes the caller to all real-time events for a board.
    /// Only board members are allowed to join.
    /// </summary>
    [HubMethod(RequiresAudit = true, Description = "Join a board's real-time group")]
    public async Task JoinBoardAsync(long boardId)
    {
        var userId = GetCurrentUserId();

        var board = await boardRepository.GetBoardWithDetailsAsync(boardId);
        if (board == null || board.BoardMembers.All(bm => bm.UserId != userId))
        {
            throw new HubException("Not authorized to join this board group.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, BoardGroup(boardId));

        logger.LogInformation(
            "User {UserId} joined {Group}", userId, BoardGroup(boardId));
    }

    /// <summary>
    /// Unsubscribes the caller from a board's real-time events.
    /// </summary>
    [HubMethod(Description = "Leave a board's real-time group")]
    public async Task LeaveBoardAsync(long boardId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, BoardGroup(boardId));

        if (TryGetCurrentUserId(out long userId))
        {
            logger.LogInformation(
                "User {UserId} left {Group}", userId, BoardGroup(boardId));
        }
    }
}
