using MediatR;
using WannabeTrello.Application.Common.Caching;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Entities.Common;
using WannabeTrello.Domain.Interfaces.Services;
using WannabeTrello.Domain.Services;

namespace WannabeTrello.Application.Features.Projects.AddProjectMember;

public class AddProjectMemberCommandHandler(
    IProjectService projectService,
    ICurrentUserService currentUserService,
    ICacheService cacheService
) : IRequestHandler<AddProjectMemberCommand, AddProjectMemberCommandResponse>
{
    public async Task<AddProjectMemberCommandResponse> Handle(AddProjectMemberCommand request,
        CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }

        var projectId = await projectService.AddProjectMember(request.ProjectId, request.NewMemberId, request.Role,
            currentUserService.UserId.Value, cancellationToken);

        await InvalidateCacheAsync(request.ProjectId, cancellationToken);

        var result = Result<long>.Success(projectId, $"{request.NewMemberId} is now added to the project.");

        return new AddProjectMemberCommandResponse(result);
    }

    private async Task InvalidateCacheAsync(long projectId, CancellationToken ct)
    {
        await cacheService.RemoveAsync(CacheKeys.ProjectMembers(projectId), ct);
    }
}