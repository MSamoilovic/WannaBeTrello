using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities.Common;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Projects.UpdateProjectMemberRole;

public class UpdateProjectMemberRoleCommandHandler(
    IProjectService projectService,
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<UpdateProjectMemberRoleCommand, UpdateProjectMemberRoleCommandResponse>
{
    public async Task<UpdateProjectMemberRoleCommandResponse> Handle(UpdateProjectMemberRoleCommand request,
        CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }

        await projectService.UpdateProjectMember(
            request.ProjectId,
            request.MemberId,
            request.Role,
            currentUserService.UserId.Value, cancellationToken);

        await InvalidateCacheAsync(request.ProjectId, cancellationToken);

        var result = Result<long>.Success(request.ProjectId, $"Project member {request.MemberId} role updated");
        return new UpdateProjectMemberRoleCommandResponse(result);
    }

    private async Task InvalidateCacheAsync(long projectId, CancellationToken ct)
    {
        await cacheService.RemoveAsync(CacheKeys.ProjectMembers(projectId), ct);
    }
}