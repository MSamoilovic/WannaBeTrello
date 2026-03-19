using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities.Common;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Comments.DeleteComment;

public class DeleteCommentCommandHandler(
    ICommentService commentService, 
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<DeleteCommentCommand, DeleteCommentCommandResponse>
{
    public async Task<DeleteCommentCommandResponse> Handle(DeleteCommentCommand request,
        CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }

        var userId = currentUserService.UserId.Value;

        // Get comment before deletion to get TaskId
        var comment = await commentService.GetCommentByIdAsync(request.CommentId, cancellationToken);

        await commentService.DeleteCommentAsync(request.CommentId, userId, cancellationToken);

        await InvalidateCacheAsync(comment.TaskId, cancellationToken);

        return new DeleteCommentCommandResponse(Result<long>.Success(request.CommentId,
            "Comment deleted successfully"));
    }

    private async Task InvalidateCacheAsync(long taskId, CancellationToken ct)
    {
        await cacheService.RemoveAsync(CacheKeys.TaskComments(taskId), ct);
    }
}