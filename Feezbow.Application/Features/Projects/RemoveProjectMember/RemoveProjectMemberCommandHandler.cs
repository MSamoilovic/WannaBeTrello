using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities.Common;
using Feezbow.Domain.Services;

namespace Feezbow.Application.Features.Projects.RemoveProjectMember;

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