using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Exceptions;
using WannabeTrello.Domain.Interfaces.Repositories;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Boards.GetBoardById;

public class GetBoardByIdQueryHandler(IBoardService boardService, ICurrentUserService currentUserService)
    : IRequestHandler<GetBoardByIdQuery, GetBoardByIdQueryResponse>
{
    public async Task<GetBoardByIdQueryResponse> Handle(GetBoardByIdQuery request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new AccessDeniedException("User is not authenticated");
        }
        
        var userId = currentUserService.UserId.Value;
        var board = await boardService.GetBoardByIdAsync(request.BoardId, userId,  cancellationToken);

        return GetBoardByIdQueryResponse.FromEntity(board);
    }
}