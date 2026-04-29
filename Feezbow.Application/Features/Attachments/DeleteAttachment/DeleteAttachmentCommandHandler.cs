using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities.Common;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Attachments.DeleteAttachment;

public class DeleteAttachmentCommandHandler(
    IAttachmentService attachmentService,
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<DeleteAttachmentCommand, DeleteAttachmentCommandResponse>
{
    public async Task<DeleteAttachmentCommandResponse> Handle(
        DeleteAttachmentCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var userId = currentUserService.UserId ?? 0;

        var attachment = await attachmentService.GetAsync(request.AttachmentId, userId, cancellationToken);
        await attachmentService.DeleteAsync(request.AttachmentId, userId, cancellationToken);

        await cacheService.RemoveAsync(
            CacheKeys.AttachmentsByOwner(attachment.OwnerType, attachment.OwnerId), cancellationToken);

        return new DeleteAttachmentCommandResponse(
            Result<bool>.Success(true, "Attachment deleted successfully."));
    }
}
