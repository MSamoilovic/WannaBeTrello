using MediatR;
using WannabeTrello.Application.Common.Caching;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Entities.Common;
using WannabeTrello.Domain.Interfaces.Services;
using WannabeTrello.Domain.Services;

namespace WannabeTrello.Application.Features.Projects.CreateProject;

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