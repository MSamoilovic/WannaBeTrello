using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Projects.GetBoardsByProjectId;

public class GetBoardsByProjectIdQueryHandler(IBoardService boardService, ICurrentUserService currentUserService)
    : IRequestHandler<GetBoardsByProjectIdQuery, List<GetBoardsByProjectIdQueryResponse>>
{
    public async Task<List<GetBoardsByProjectIdQueryResponse>> Handle(GetBoardsByProjectIdQuery request,
        CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }

        var result = await boardService.GetBoardByProjectIdAsync(
            request.ProjectId, 
            currentUserService.UserId!.Value,
            cancellationToken
        );

        return result.Select(board => new GetBoardsByProjectIdQueryResponse(board.Id, board.Name)).ToList();
    }
}