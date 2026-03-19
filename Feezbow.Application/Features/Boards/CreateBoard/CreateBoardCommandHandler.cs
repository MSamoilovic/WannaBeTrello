using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities.Common;
using Feezbow.Domain.Interfaces.Services;
using Feezbow.Domain.Services;

namespace Feezbow.Application.Features.Boards.CreateBoard;

public class CreateBoardCommandHandler(
    ICurrentUserService currentUserService, 
    IBoardService boardService, 
    ICacheService cacheService)
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
        
        await InvalidateCacheAsync(request.ProjectId, cancellationToken);
        
        var result =  Result<long>.Success(board.Id, "Board created successfully");
        
        return new CreateBoardCommandResponse(result);
    }
    
    private async Task InvalidateCacheAsync(long projectId, CancellationToken ct)
    {
        await cacheService.RemoveAsync(CacheKeys.ProjectBoards(projectId), ct);
    }
}