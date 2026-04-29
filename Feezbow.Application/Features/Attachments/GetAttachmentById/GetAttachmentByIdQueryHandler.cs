using MediatR;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Application.Features.Attachments.Common;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Attachments.GetAttachmentById;

public class GetAttachmentByIdQueryHandler(
    IAttachmentService attachmentService,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetAttachmentByIdQuery, AttachmentDto>
{
    public async Task<AttachmentDto> Handle(GetAttachmentByIdQuery request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var userId = currentUserService.UserId ?? 0;

        var attachment = await attachmentService.GetAsync(request.AttachmentId, userId, cancellationToken);
        return AttachmentDto.FromEntity(attachment);
    }
}
