using MediatR;
using WannabeTrello.Application.Common.Caching;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Entities.Common;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Projects.UpdateProjectMemberRole;

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