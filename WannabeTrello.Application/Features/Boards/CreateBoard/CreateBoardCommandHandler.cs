using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Interfaces.Services;
using WannabeTrello.Domain.Services;

namespace WannabeTrello.Application.Features.Boards.CreateBoard;

public class CreateBoardCommandHandler(ICurrentUserService currentUserService, IBoardService boardService)
    : IRequestHandler<CreateBoardCommand, long>
{
  

    public async Task<long> Handle(CreateBoardCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }
        
        var board = await boardService.CreateBoardAsync(
            request.ProjectId,
            request.Name,
            request.Description,
            currentUserService!.UserId ?? 0,
            cancellationToken
        );
        
        return board.Id;
    }
}