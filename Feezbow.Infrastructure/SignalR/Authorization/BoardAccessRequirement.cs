using Microsoft.AspNetCore.Authorization;

namespace WannabeTrello.Infrastructure.SignalR.Authorization;

/// <summary>
/// Authorization requirement that checks whether the current user is a member of a specific board.
/// </summary>
public sealed class BoardAccessRequirement(long boardId) : IAuthorizationRequirement
{
    public long BoardId { get; } = boardId;
}
