using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Boards.GetColumnsByBoardIId;

public class GetColumnsByBoardIdQueryHandler(IBoardService boardService, ICurrentUserService currentUserService)
    : IRequestHandler<GetColumnsByBoardIdQuery, List<GetColumnsByBoardIdQueryResponse>>
{
    public async Task<List<GetColumnsByBoardIdQueryResponse>> Handle(GetColumnsByBoardIdQuery request,
        CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }

        var result = await boardService.GetColumnsByBoardIdAsync(
            request.BoardId,
            currentUserService.UserId!.Value,
            cancellationToken
        );

        return result.Select(GetColumnsByBoardIdQueryResponse.FromEntity).ToList();
    }
}