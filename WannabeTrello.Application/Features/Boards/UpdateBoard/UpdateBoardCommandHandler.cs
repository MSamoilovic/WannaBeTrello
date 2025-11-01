using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Interfaces.Services;
using WannabeTrello.Domain.Services;

namespace WannabeTrello.Application.Features.Boards.UpdateBoard;

public class UpdateBoardCommandHandler(IBoardService boardService, ICurrentUserService currentUserService)
    : IRequestHandler<UpdateBoardCommand, UpdateBoardCommandResponse>
{
    public async Task<UpdateBoardCommandResponse> Handle(UpdateBoardCommand request,
        CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var updatedBoard = await boardService.UpdateBoardAsync(
            request.Id, 
            request?.Name, 
            request?.Description,
            currentUserService.UserId.Value
            , cancellationToken);

        return UpdateBoardCommandResponse.FromEntity(updatedBoard);
    }
}