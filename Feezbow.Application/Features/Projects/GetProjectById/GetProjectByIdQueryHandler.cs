using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces.Services;
using Feezbow.Domain.Services;

namespace Feezbow.Application.Features.Projects.GetProjectById;

public class GetProjectByIdQueryHandler(
    IProjectService projectService, 
    ICurrentUserService currentUserService, 
    ICacheService cacheService)
    : IRequestHandler<GetProjectByIdQuery, GetProjectByIdQueryResponse>
{
    public async Task<GetProjectByIdQueryResponse> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new AccessDeniedException("User is not authenticated");
        }
        
        var userId = currentUserService.UserId.Value;
        var cacheKey = CacheKeys.Project(request.ProjectId);
        
        var project = await cacheService.GetOrSetAsync(
            cacheKey,
            () => projectService.GetProjectByIdAsync(request.ProjectId, userId, cancellationToken),
            CacheExpiration.Medium,
            cancellationToken
        );
        
        return project is null ? throw new NotFoundException(nameof(Project), request.ProjectId) : GetProjectByIdQueryResponse.FromEntity(project);
    }
}