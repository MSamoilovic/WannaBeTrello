using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Entities.Common;
using WannabeTrello.Domain.Interfaces.Services;
using WannabeTrello.Domain.Services;

namespace WannabeTrello.Application.Features.Boards.CreateBoard;

public class CreateBoardCommandHandler(ICurrentUserService currentUserService, IBoardService boardService)
    : IRequestHandler<CreateBoardCommand, CreateBoardCommandResponse>
{
    
    public async Task<CreateBoardCommandResponse> Handle(CreateBoardCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }
        
        var board = await boardService.CreateBoardAsync(
            request.Name,
            request.Description,
            request.ProjectId,
            currentUserService!.UserId ?? 0,
            cancellationToken
        );
        
        var result =  Result<long>.Success(board.Id, "Board created successfully");
        
        return new CreateBoardCommandResponse(result);
    }
}