using MediatR;
using WannabeTrello.Application.Common.Caching;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Entities.Common;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Comments.UpdateCommentContent;

public class UpdateCommentContentCommandHandler(
    ICommentService commentService, 
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<UpdateCommentContentCommand, UpdateCommentContentCommandResponse>
{
    public async Task<UpdateCommentContentCommandResponse> Handle(UpdateCommentContentCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }
        
        var userId = currentUserService.UserId.Value;

        // Get comment before update to get TaskId
        var comment = await commentService.GetCommentByIdAsync(request.CommentId, cancellationToken);

        await commentService.UpdateCommentAsync(request.CommentId, request.NewContent, userId, cancellationToken);

        await InvalidateCacheAsync(comment.TaskId, cancellationToken);
        
        return new UpdateCommentContentCommandResponse(Result<long>.Success(request.CommentId, "Comment updated successfully"));
    }

    private async Task InvalidateCacheAsync(long taskId, CancellationToken ct)
    {
        await cacheService.RemoveAsync(CacheKeys.TaskComments(taskId), ct);
    }
}