using Feezbow.Domain.Enums;

namespace Feezbow.Domain.Events.Attachment_Events;

public class AttachmentDeletedEvent(
    long attachmentId,
    AttachmentOwnerType ownerType,
    long ownerId,
    long deletedBy) : DomainEvent
{
    public long AttachmentId => attachmentId;
    public AttachmentOwnerType OwnerType => ownerType;
    public long OwnerId => ownerId;
    public long DeletedBy => deletedBy;
}
