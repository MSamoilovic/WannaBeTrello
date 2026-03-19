using Feezbow.Domain.Entities.Common;

namespace Feezbow.Application.Features.Comments.RestoreComment;

public record RestoreCommentCommandResponse(Result<long> Result);