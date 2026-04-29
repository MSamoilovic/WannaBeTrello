using MediatR;

namespace Feezbow.Application.Features.Attachments.DeleteAttachment;

public record DeleteAttachmentCommand(long AttachmentId) : IRequest<DeleteAttachmentCommandResponse>;
