using MediatR;

namespace Feezbow.Application.Features.Attachments.DownloadAttachment;

public record DownloadAttachmentQuery(long AttachmentId) : IRequest<DownloadAttachmentResult>;

public record DownloadAttachmentResult(
    Stream Content,
    string FileName,
    string ContentType,
    long SizeBytes);
