namespace WannabeTrello.Infrastructure.SignalR.Hubs.Base;

/// <summary>
/// Decorates a hub method with metadata for audit logging and documentation.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class HubMethodAttribute : Attribute
{
    /// <summary>
    /// When true, all invocations are written to the audit log.
    /// </summary>
    public bool RequiresAudit { get; set; } = false;

    /// <summary>
    /// Human-readable description of what the hub method does.
    /// </summary>
    public string? Description { get; set; }
}
