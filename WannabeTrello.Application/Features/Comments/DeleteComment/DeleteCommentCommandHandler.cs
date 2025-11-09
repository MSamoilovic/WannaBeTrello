using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Entities.Common;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Comments.DeleteComment;

public class DeleteCommentCommandHandler(ICommentService commentService, ICurrentUserService currentUserService)
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

        await commentService.DeleteCommentAsync(request.CommentId, userId, cancellationToken);

        return new DeleteCommentCommandResponse(Result<long>.Success(request.CommentId,
            "Comment deleted successfully"));
    }
}