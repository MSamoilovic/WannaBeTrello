using MediatR;
using Feezbow.Application.Features.Attachments.Common;

namespace Feezbow.Application.Features.Attachments.GetAttachmentById;

public record GetAttachmentByIdQuery(long AttachmentId) : IRequest<AttachmentDto>;
