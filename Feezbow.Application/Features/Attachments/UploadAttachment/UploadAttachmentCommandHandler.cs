using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Application.Features.Attachments.Common;
using Feezbow.Domain.Entities.Common;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Attachments.UploadAttachment;

public class UploadAttachmentCommandHandler(
    IAttachmentService attachmentService,
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<UploadAttachmentCommand, UploadAttachmentCommandResponse>
{
    public async Task<UploadAttachmentCommandResponse> Handle(
        UploadAttachmentCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var userId = currentUserService.UserId ?? 0;

        var attachment = await attachmentService.UploadAsync(
            request.OwnerType,
            request.OwnerId,
            userId,
            request.FileName,
            request.ContentType,
            request.SizeBytes,
            request.Content,
            request.Description,
            cancellationToken);

        await cacheService.RemoveAsync(
            CacheKeys.AttachmentsByOwner(request.OwnerType, request.OwnerId), cancellationToken);

        return new UploadAttachmentCommandResponse(
            Result<AttachmentDto>.Success(AttachmentDto.FromEntity(attachment), "Attachment uploaded successfully."));
    }
}
