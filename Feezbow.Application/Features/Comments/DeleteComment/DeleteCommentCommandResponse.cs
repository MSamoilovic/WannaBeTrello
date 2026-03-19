using Feezbow.Domain.Entities.Common;

namespace Feezbow.Application.Features.Comments.DeleteComment;

public record DeleteCommentCommandResponse(Result<long> Result);