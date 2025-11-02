using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Entities.Common;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Boards.RestoreBoard;

public class RestoreBoardCommandHandler(IBoardService boardService, ICurrentUserService currentUserService)
    : IRequestHandler<RestoreBoardCommand, RestoreBoardCommandResponse>
{
    public async Task<RestoreBoardCommandResponse> Handle(RestoreBoardCommand request,
        CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }
        
        var boardId = await boardService.RestoreBoardAsync(request.BoardId, currentUserService.UserId.Value, cancellationToken);
        var result = Result<long>.Success(boardId, $"Board {request.BoardId} is now restored.");
        
        return new RestoreBoardCommandResponse(result);
    }
}