using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces.Repositories;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Tasks.GetTaskById;

public class GetTaskByIdQueryHandler(
    IBoardTaskService taskService, 
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<GetTaskByIdQuery, GetTaskByIdQueryResponse>
{
    public async Task<GetTaskByIdQueryResponse> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
    {
        if (currentUserService.UserId is null)
            throw new UnauthorizedAccessException("User is not authenticated");
        
        var cacheKey = CacheKeys.Task(request.taskId);
        
        var task = await cacheService.GetOrSetAsync(
            cacheKey,
            () => taskService.GetTaskByIdAsync(request.taskId, currentUserService.UserId.Value, cancellationToken),
            CacheExpiration.Short,
            cancellationToken
        );
        
        return GetTaskByIdQueryResponse.FromEntity(task!);
    }
}