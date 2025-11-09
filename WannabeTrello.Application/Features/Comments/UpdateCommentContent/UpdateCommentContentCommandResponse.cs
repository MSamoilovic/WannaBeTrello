using WannabeTrello.Domain.Entities.Common;

namespace WannabeTrello.Application.Features.Comments.UpdateCommentContent;

public record UpdateCommentContentCommandResponse(Result<long> Result);