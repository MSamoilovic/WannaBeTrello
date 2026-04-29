using Feezbow.Domain.Entities;
using Feezbow.Domain.Enums;

namespace Feezbow.Domain.Interfaces.Services;

public interface IAttachmentService
{
    Task<Attachment> UploadAsync(
        AttachmentOwnerType ownerType,
        long ownerId,
        long userId,
        string fileName,
        string contentType,
        long sizeBytes,
        Stream content,
        string? description,
        CancellationToken cancellationToken = default);

    Task<Attachment> GetAsync(long attachmentId, long userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Attachment>> GetByOwnerAsync(
        AttachmentOwnerType ownerType,
        long ownerId,
        long userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns metadata + an open read stream. Caller owns the stream and must dispose it.
    /// </summary>
    Task<(Attachment Metadata, Stream Content)> OpenAsync(
        long attachmentId,
        long userId,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(long attachmentId, long userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes every attachment for the given owner — metadata + files. Skips per-user permission checks
    /// because this is invoked by domain event handlers in response to the owner itself being deleted.
    /// </summary>
    Task DeleteByOwnerAsync(
        AttachmentOwnerType ownerType,
        long ownerId,
        long systemUserId,
        CancellationToken cancellationToken = default);
}
