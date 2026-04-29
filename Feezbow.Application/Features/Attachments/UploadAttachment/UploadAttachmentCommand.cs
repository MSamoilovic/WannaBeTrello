using MediatR;
using Feezbow.Domain.Enums;

namespace Feezbow.Application.Features.Attachments.UploadAttachment;

public record UploadAttachmentCommand(
    AttachmentOwnerType OwnerType,
    long OwnerId,
    string FileName,
    string ContentType,
    long SizeBytes,
    Stream Content,
    string? Description = null) : IRequest<UploadAttachmentCommandResponse>;
