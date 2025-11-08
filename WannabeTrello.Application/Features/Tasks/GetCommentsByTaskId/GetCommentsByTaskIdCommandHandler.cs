using System.Collections.Immutable;
using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Tasks.GetCommentsByTaskId;

public class GetCommentsByTaskIdCommandHandler(ICommentService commentService, ICurrentUserService currentUserService)
    : IRequestHandler<GetCommentsByTaskIdCommand, IReadOnlyList<GetCommentsByTaskIdCommandResponse>>
{
    public async Task<IReadOnlyList<GetCommentsByTaskIdCommandResponse>> Handle(GetCommentsByTaskIdCommand request,
        CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }
        
        var result =  await commentService.GetCommentsByTaskId(request.TaskId, cancellationToken);

        return result.Select(GetCommentsByTaskIdCommandResponse.FromEntity).ToImmutableList();
    }
}