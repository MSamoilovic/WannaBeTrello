using Microsoft.AspNetCore.Authorization;

namespace WannabeTrello.Infrastructure.SignalR.Authorization;

/// <summary>
/// Authorization requirement that checks whether the current user is a member of a specific project.
/// </summary>
public sealed class ProjectAccessRequirement(long projectId) : IAuthorizationRequirement
{
    public long ProjectId { get; } = projectId;
}
