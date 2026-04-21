using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Household.GetHouseholdMembers;

public class GetHouseholdMembersQueryHandler(
    IHouseholdService householdService,
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
                var members = await householdService.GetMembersAsync(request.ProjectId, userId, cancellationToken);
                return GetHouseholdMembersQueryResponse.FromMembers(members);
            },
            CacheExpiration.Medium,
            cancellationToken);

        return response!;
    }
}
