using WannabeTrello.Domain.Entities.Common;

namespace WannabeTrello.Application.Features.Comments.DeleteComment;

public record DeleteCommentCommandResponse(Result<long> Result);