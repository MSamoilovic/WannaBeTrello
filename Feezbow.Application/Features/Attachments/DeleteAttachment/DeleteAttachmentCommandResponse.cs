using Feezbow.Domain.Entities.Common;

namespace Feezbow.Application.Features.Attachments.DeleteAttachment;

public record DeleteAttachmentCommandResponse(Result<bool> Result);
