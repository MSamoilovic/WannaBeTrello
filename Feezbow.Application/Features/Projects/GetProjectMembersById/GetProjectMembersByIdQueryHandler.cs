using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Interfaces.Repositories;

namespace Feezbow.Application.Features.Projects.GetProjectMembersById;

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