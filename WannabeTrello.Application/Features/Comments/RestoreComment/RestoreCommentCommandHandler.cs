using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Entities.Common;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Comments.RestoreComment;

public class RestoreCommentCommandHandler(ICommentService commentService, ICurrentUserService currentUserService): IRequestHandler<RestoreCommentCommand, RestoreCommentCommandResponse>
{
    public async Task<RestoreCommentCommandResponse> Handle(RestoreCommentCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }

        var userId = currentUserService.UserId.Value;

        await commentService.RestoreCommentAsync(request.CommentId, userId, cancellationToken);

        return new RestoreCommentCommandResponse(Result<long>.Success(request.CommentId,
            "Comment restored successfully"));
    }
}