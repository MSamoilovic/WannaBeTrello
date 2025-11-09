using WannabeTrello.Domain.Entities.Common;

namespace WannabeTrello.Application.Features.Comments.RestoreComment;

public record RestoreCommentCommandResponse(Result<long> Result);