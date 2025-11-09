using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Entities.Common;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Comments.UpdateCommentContent;

public class UpdateCommentContentCommandHandler(ICommentService commentService, ICurrentUserService currentUserService): IRequestHandler<UpdateCommentContentCommand, UpdateCommentContentCommandResponse>
{
    public async Task<UpdateCommentContentCommandResponse> Handle(UpdateCommentContentCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }
        
        var userId = currentUserService.UserId.Value;

        await commentService.UpdateCommentAsync(request.CommentId, request.NewContent, userId, cancellationToken);
        
        return new UpdateCommentContentCommandResponse(Result<long>.Success(request.CommentId, "Comment updated successfully"));
    }
}