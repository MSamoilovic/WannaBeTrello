using MediatR;
using WannabeTrello.Application.Common.Caching;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Interfaces.Repositories;

namespace WannabeTrello.Application.Features.Projects.GetProjectMembersById;

public class GetProjectMembersByIdQueryHandler(
    IProjectRepository projectRepository,
    ICacheService cacheService
) : IRequestHandler<GetProjectMembersByIdQuery, List<GetProjectMembersByIdQueryResponse>>
{
    public async Task<List<GetProjectMembersByIdQueryResponse>> Handle(GetProjectMembersByIdQuery request,
        CancellationToken cancellationToken)
    {
        var cacheKey = CacheKeys.ProjectMembers(request.ProjectId);

        var projectMembers = await cacheService.GetOrSetAsync(
            cacheKey,
            () => projectRepository.GetProjectMembersByProjectIdAsync(request.ProjectId, cancellationToken),
            CacheExpiration.Medium,
            cancellationToken
        );
        
        return projectMembers?.Select(pm => new GetProjectMembersByIdQueryResponse(
                pm.User.Id,
                pm.User.FirstName,
                pm.User.LastName,
                pm.Role
            )).ToList() ?? [];
    }
}