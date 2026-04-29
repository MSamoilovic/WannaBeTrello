using Feezbow.Domain.Enums;
using Feezbow.Domain.Events.Attachment_Events;
using Feezbow.Domain.Exceptions;

namespace Feezbow.Domain.Entities;

/// <summary>
/// Polymorphic attachment metadata. The actual binary lives in object storage at <see cref="StorageKey"/>.
/// OwnerType + OwnerId identify the linked entity (Bill, Household-as-Project, …) without DB-level FK.
/// </summary>
public class Attachment : AuditableEntity
{
    public AttachmentOwnerType OwnerType { get; private set; }
    public long OwnerId { get; private set; }

    /// <summary>Original file name as supplied by the uploader.</summary>
    public string FileName { get; private set; } = null!;

    /// <summary>Opaque key understood by the storage backend (e.g. relative path on disk, S3 object key).</summary>
    public string StorageKey { get; private set; } = null!;

    public string ContentType { get; private set; } = null!;
    public long SizeBytes { get; private set; }
    public string? Description { get; private set; }

    private Attachment() { }

    public static Attachment Create(
        AttachmentOwnerType ownerType,
        long ownerId,
        string fileName,
        string storageKey,
        string contentType,
        long sizeBytes,
        long uploadedBy,
        string? description = null)
    {
        if (ownerId <= 0)
            throw new BusinessRuleValidationException("OwnerId must be a positive number.");

        if (string.IsNullOrWhiteSpace(fileName))
            throw new BusinessRuleValidationException("File name cannot be empty.");

        if (string.IsNullOrWhiteSpace(storageKey))
            throw new BusinessRuleValidationException("Storage key cannot be empty.");

        if (string.IsNullOrWhiteSpace(contentType))
            throw new BusinessRuleValidationException("Content type cannot be empty.");

        if (sizeBytes <= 0)
            throw new BusinessRuleValidationException("File size must be greater than zero.");

        if (uploadedBy <= 0)
            throw new BusinessRuleValidationException("UploadedBy must be a positive number.");

        var attachment = new Attachment
        {
            OwnerType = ownerType,
            OwnerId = ownerId,
            FileName = fileName.Trim(),
            StorageKey = storageKey,
            ContentType = contentType.Trim(),
            SizeBytes = sizeBytes,
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = uploadedBy
        };

        attachment.AddDomainEvent(new AttachmentUploadedEvent(
            attachment.Id, ownerType, ownerId, attachment.FileName, sizeBytes, uploadedBy));

        return attachment;
    }

    public void MarkDeleted(long deletedBy)
    {
        if (deletedBy <= 0)
            throw new BusinessRuleValidationException("DeletedBy must be a positive number.");

        AddDomainEvent(new AttachmentDeletedEvent(Id, OwnerType, OwnerId, deletedBy));
    }
}
