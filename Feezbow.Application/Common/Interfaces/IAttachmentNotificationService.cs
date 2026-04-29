using Feezbow.Domain.Enums;

namespace Feezbow.Application.Common.Interfaces;

public interface IAttachmentNotificationService
{
    Task NotifyUploaded(
        long attachmentId,
        long projectId,
        AttachmentOwnerType ownerType,
        long ownerId,
        string fileName,
        long sizeBytes,
        long uploadedBy,
        CancellationToken cancellationToken = default);

    Task NotifyDeleted(
        long attachmentId,
        long projectId,
        AttachmentOwnerType ownerType,
        long ownerId,
        long deletedBy,
        CancellationToken cancellationToken = default);
}
