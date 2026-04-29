using Feezbow.Domain.Enums;

namespace Feezbow.Domain.Events.Attachment_Events;

public class AttachmentUploadedEvent(
    long attachmentId,
    AttachmentOwnerType ownerType,
    long ownerId,
    string fileName,
    long sizeBytes,
    long uploadedBy) : DomainEvent
{
    public long AttachmentId => attachmentId;
    public AttachmentOwnerType OwnerType => ownerType;
    public long OwnerId => ownerId;
    public string FileName => fileName;
    public long SizeBytes => sizeBytes;
    public long UploadedBy => uploadedBy;
}
