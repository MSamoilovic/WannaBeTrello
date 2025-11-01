using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Entities.Common;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Boards.ArchiveBoard;

public class ArchiveBoardCommandHandler(IBoardService boardService, ICurrentUserService currentUserService)
    : IRequestHandler<ArchiveBoardCommand, ArchiveBoardCommandResponse>
{
    public async Task<ArchiveBoardCommandResponse> Handle(ArchiveBoardCommand request,
        CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }

        var boardId =
            await boardService.ArchiveBoardAsync(request.BoardId, currentUserService.UserId.Value, cancellationToken);
        
        var result = Result<long>.Success(boardId, $"Board {request.BoardId} is now archived.");

        return new ArchiveBoardCommandResponse(result);
    }
}