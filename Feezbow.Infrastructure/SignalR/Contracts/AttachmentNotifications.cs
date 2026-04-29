using Feezbow.Domain.Enums;

namespace Feezbow.Infrastructure.SignalR.Contracts;

public record AttachmentUploadedNotification
{
    public required long AttachmentId { get; init; }
    public required long ProjectId { get; init; }
    public required AttachmentOwnerType OwnerType { get; init; }
    public required long OwnerId { get; init; }
    public required string FileName { get; init; }
    public required long SizeBytes { get; init; }
    public required long UploadedBy { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
}

public record AttachmentDeletedNotification
{
    public required long AttachmentId { get; init; }
    public required long ProjectId { get; init; }
    public required AttachmentOwnerType OwnerType { get; init; }
    public required long OwnerId { get; init; }
    public required long DeletedBy { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
}
