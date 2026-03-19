using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities.Common;
using Feezbow.Domain.Interfaces.Services;
using Feezbow.Domain.Services;

namespace Feezbow.Application.Features.Projects.CreateProject;

public class CreateProjectCommandHandler(
    ICurrentUserService currentUserService, 
    IProjectService projectService,
    ICacheService cacheService)
    : IRequestHandler<CreateProjectCommand, CreateProjectCommandResponse>
{
    public async Task<CreateProjectCommandResponse> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }
        
        var userId = currentUserService.UserId ?? 0;
        var project = await projectService.CreateProjectAsync(request.Name, request.Description, userId, cancellationToken);
        
        await InvalidateCacheAsync(userId, cancellationToken);
        
        var result = Result<long>.Success(project.Id, "Project Created Successfully");
        
        return new CreateProjectCommandResponse(result);
    }

    private async Task InvalidateCacheAsync(long userId, CancellationToken ct)
    {
        await cacheService.RemoveAsync(CacheKeys.UserProjects(userId), ct);
    }
}