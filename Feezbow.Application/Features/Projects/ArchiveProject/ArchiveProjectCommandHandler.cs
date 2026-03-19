using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities.Common;
using Feezbow.Domain.Interfaces.Services;
using Feezbow.Domain.Services;

namespace Feezbow.Application.Features.Projects.ArchiveProject;

public class ArchiveProjectCommandHandler(
    IProjectService service,
    ICurrentUserService currentUserService,
    ICacheService cacheService
): IRequestHandler<ArchiveProjectCommand, ArchiveProjectCommandResponse>
{
    public async Task<ArchiveProjectCommandResponse> Handle(ArchiveProjectCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }
        
        // Get project before archiving to get OwnerId
        var project = await service.GetProjectByIdAsync(request.ProjectId, currentUserService.UserId.Value, cancellationToken);
        
        var projectId = await service.ArchiveProjectAsync(request.ProjectId, currentUserService.UserId.Value, cancellationToken);
        
        await InvalidateCacheAsync(request.ProjectId, project.OwnerId, cancellationToken);
        
        var result = Result<long>.Success(projectId, $"Project {request.ProjectId} is now archived.");
        
        return new ArchiveProjectCommandResponse(result);
    }

    private async Task InvalidateCacheAsync(long projectId, long ownerId, CancellationToken ct)
    {
        await cacheService.RemoveAsync(CacheKeys.Project(projectId), ct);
        await cacheService.RemoveAsync(CacheKeys.UserProjects(ownerId), ct);
    }
}