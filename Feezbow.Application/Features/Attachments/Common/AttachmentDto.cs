using Feezbow.Domain.Entities;
using Feezbow.Domain.Enums;

namespace Feezbow.Application.Features.Attachments.Common;

public record AttachmentDto(
    long Id,
    AttachmentOwnerType OwnerType,
    long OwnerId,
    string FileName,
    string ContentType,
    long SizeBytes,
    string? Description,
    long? UploadedBy,
    DateTime UploadedAt)
{
    public static AttachmentDto FromEntity(Attachment a) => new(
        a.Id,
        a.OwnerType,
        a.OwnerId,
        a.FileName,
        a.ContentType,
        a.SizeBytes,
        a.Description,
        a.CreatedBy,
        a.CreatedAt);
}
