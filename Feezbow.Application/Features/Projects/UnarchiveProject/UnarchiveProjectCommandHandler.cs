using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities.Common;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Projects.UnarchiveProject;

public class UnarchiveProjectCommandHandler(
    IProjectService service,
    ICurrentUserService currentUserService,
    ICacheService cacheService
) : IRequestHandler<UnarchiveProjectCommand, UnarchiveProjectCommandResponse>
{
    public async Task<UnarchiveProjectCommandResponse> Handle(UnarchiveProjectCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
            throw new UnauthorizedAccessException("User is not authenticated");

        var projectId = await service.UnarchiveProjectAsync(request.ProjectId, currentUserService.UserId.Value, cancellationToken);

        // Fetch project AFTER restoring — global filter (!IsArchived) now allows the query.
        var project = await service.GetProjectByIdAsync(projectId, currentUserService.UserId.Value, cancellationToken);

        await InvalidateCacheAsync(projectId, project.OwnerId, cancellationToken);

        var result = Result<long>.Success(projectId, $"Project {request.ProjectId} is now restored.");

        return new UnarchiveProjectCommandResponse(result);
    }

    private async Task InvalidateCacheAsync(long projectId, long ownerId, CancellationToken ct)
    {
        await cacheService.RemoveAsync(CacheKeys.Project(projectId), ct);
        await cacheService.RemoveAsync(CacheKeys.UserProjects(ownerId), ct);
    }
}
