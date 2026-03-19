using Feezbow.Domain.Entities.Common;

namespace Feezbow.Application.Features.Comments.UpdateCommentContent;

public record UpdateCommentContentCommandResponse(Result<long> Result);