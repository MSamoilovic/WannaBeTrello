using Feezbow.Application.Features.Attachments.Common;
using Feezbow.Domain.Entities.Common;

namespace Feezbow.Application.Features.Attachments.UploadAttachment;

public record UploadAttachmentCommandResponse(Result<AttachmentDto> Result);
