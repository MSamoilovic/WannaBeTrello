using System.Collections.Immutable;
using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Application.Features.Boards.GetBoardById;
using Feezbow.Domain.Entities;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces.Repositories;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Tasks.GetTasksByBoardId;

public class GetTasksByBoardIdQueryHandler(
    IBoardTaskService boardTaskService,
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<GetTasksByBoardIdQuery, ImmutableList<GetTaskByBoardIdQueryResponse>>
{
    public async Task<ImmutableList<GetTaskByBoardIdQueryResponse>> Handle(GetTasksByBoardIdQuery request, CancellationToken cancellationToken)
    {
        
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new AccessDeniedException("User is not authenticated");
        }
        
        var userId = currentUserService.UserId.Value;
        var cacheKey = CacheKeys.BoardTasks(request.BoardId);
        
        var tasks = await cacheService.GetOrSetAsync(
            cacheKey,
            () => boardTaskService.GetTasksByBoardIdAsync(request.BoardId, userId, cancellationToken),
            CacheExpiration.Short,
            cancellationToken
        );
        
        if (tasks == null)
        {
            return ImmutableList<GetTaskByBoardIdQueryResponse>.Empty;
        }
        
        return tasks.Select(GetTaskByBoardIdQueryResponse.FromEntity).ToImmutableList();
    }
}