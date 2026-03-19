using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities.Common;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Comments.RestoreComment;

public class RestoreCommentCommandHandler(
    ICommentService commentService, 
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<RestoreCommentCommand, RestoreCommentCommandResponse>
{
    public async Task<RestoreCommentCommandResponse> Handle(RestoreCommentCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }

        var userId = currentUserService.UserId.Value;

        // Get comment before restore to get TaskId
        var comment = await commentService.GetCommentByIdAsync(request.CommentId, cancellationToken);

        await commentService.RestoreCommentAsync(request.CommentId, userId, cancellationToken);

        await InvalidateCacheAsync(comment.TaskId, cancellationToken);

        return new RestoreCommentCommandResponse(Result<long>.Success(request.CommentId,
            "Comment restored successfully"));
    }

    private async Task InvalidateCacheAsync(long taskId, CancellationToken ct)
    {
        await cacheService.RemoveAsync(CacheKeys.TaskComments(taskId), ct);
    }
}