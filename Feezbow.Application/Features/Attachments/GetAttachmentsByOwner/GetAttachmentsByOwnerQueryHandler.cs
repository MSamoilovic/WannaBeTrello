using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Application.Features.Attachments.Common;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Attachments.GetAttachmentsByOwner;

public class GetAttachmentsByOwnerQueryHandler(
    IAttachmentService attachmentService,
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<GetAttachmentsByOwnerQuery, IReadOnlyList<AttachmentDto>>
{
    public async Task<IReadOnlyList<AttachmentDto>> Handle(
        GetAttachmentsByOwnerQuery request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var userId = currentUserService.UserId ?? 0;

        var cached = await cacheService.GetOrSetAsync(
            CacheKeys.AttachmentsByOwner(request.OwnerType, request.OwnerId),
            async () =>
            {
                var attachments = await attachmentService.GetByOwnerAsync(
                    request.OwnerType, request.OwnerId, userId, cancellationToken);
                return attachments.Select(AttachmentDto.FromEntity).ToList();
            },
            CacheExpiration.Medium,
            cancellationToken);

        return cached ?? new List<AttachmentDto>();
    }
}
