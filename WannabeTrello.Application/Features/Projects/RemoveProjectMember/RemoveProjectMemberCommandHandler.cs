using MediatR;
using WannabeTrello.Application.Common.Caching;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Entities.Common;
using WannabeTrello.Domain.Services;

namespace WannabeTrello.Application.Features.Projects.RemoveProjectMember;

public class RemoveProjectMemberCommandHandler(
    ProjectService projectService, 
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<RemoveProjectMemberCommand, RemoveProjectMemberCommandResponse>
{
    public async Task<RemoveProjectMemberCommandResponse> Handle(RemoveProjectMemberCommand request,
        CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }
        
        var removingUserId = currentUserService.UserId.Value;

        await projectService.RemoveProjectMember(
            request.ProjectId, 
            request.UserToRemoveId,
            removingUserId,
            cancellationToken);

        await InvalidateCacheAsync(request.ProjectId, cancellationToken);

        var result = Result<long>.Success(request.ProjectId, "User Removed from the project");
        
        return new RemoveProjectMemberCommandResponse(result);
    }

    private async Task InvalidateCacheAsync(long projectId, CancellationToken ct)
    {
        await cacheService.RemoveAsync(CacheKeys.ProjectMembers(projectId), ct);
    }
}