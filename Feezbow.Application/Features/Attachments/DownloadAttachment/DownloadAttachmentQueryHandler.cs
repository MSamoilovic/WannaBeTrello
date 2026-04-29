using MediatR;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Attachments.DownloadAttachment;

public class DownloadAttachmentQueryHandler(
    IAttachmentService attachmentService,
    ICurrentUserService currentUserService)
    : IRequestHandler<DownloadAttachmentQuery, DownloadAttachmentResult>
{
    public async Task<DownloadAttachmentResult> Handle(
        DownloadAttachmentQuery request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var userId = currentUserService.UserId ?? 0;

        var (metadata, stream) = await attachmentService.OpenAsync(
            request.AttachmentId, userId, cancellationToken);

        return new DownloadAttachmentResult(stream, metadata.FileName, metadata.ContentType, metadata.SizeBytes);
    }
}
