using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces;

namespace Feezbow.Application.Features.Household.GetHouseholdMembers;

public class GetHouseholdMembersQueryHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<GetHouseholdMembersQuery, GetHouseholdMembersQueryResponse>
{
    public async Task<GetHouseholdMembersQueryResponse> Handle(
        GetHouseholdMembersQuery request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var userId = currentUserService.UserId ?? 0;

        var response = await cacheService.GetOrSetAsync(
            CacheKeys.HouseholdMembers(request.ProjectId),
            async () =>
            {
                var profile = await unitOfWork.Households.GetByProjectIdWithMembersAsync(request.ProjectId, cancellationToken)
                    ?? throw new NotFoundException("HouseholdProfile", request.ProjectId);

                if (!profile.Project.IsMember(userId))
                    throw new AccessDeniedException("You are not a member of this project.");

                return GetHouseholdMembersQueryResponse.FromMembers(profile.Project.ProjectMembers);
            },
            CacheExpiration.Medium,
            cancellationToken);

        return response!;
    }
}
