using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Services;

namespace WannabeTrello.Application.Features.Boards.UpdateBoard;

public class UpdateBoardCommandHandler(BoardService boardService, ICurrentUserService currentUserService)
    : IRequestHandler<UpdateBoardCommand, UpdateBoardCommandResponse>
{
    
    public async Task<UpdateBoardCommandResponse> Handle(UpdateBoardCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("Korisnik nije autentifikovan.");
        }

        var updatedBoard = await boardService.UpdateBoardDetailsAsync(
            request.Id,
            request?.Name,
            request?.Description,
            currentUserService.UserId.Value
        );
        
        // Koristimo statičku metodu za ručno mapiranje umesto AutoMappera
        return UpdateBoardCommandResponse.FromEntity(updatedBoard);
    }
}