using MediatR;
using Feezbow.Application.Features.Attachments.Common;
using Feezbow.Domain.Enums;

namespace Feezbow.Application.Features.Attachments.GetAttachmentsByOwner;

public record GetAttachmentsByOwnerQuery(AttachmentOwnerType OwnerType, long OwnerId)
    : IRequest<IReadOnlyList<AttachmentDto>>;
