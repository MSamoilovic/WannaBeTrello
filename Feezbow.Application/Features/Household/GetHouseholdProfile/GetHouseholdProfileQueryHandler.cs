using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Household.GetHouseholdProfile;

public class GetHouseholdProfileQueryHandler(
    IHouseholdService householdService,
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<GetHouseholdProfileQuery, GetHouseholdProfileQueryResponse>
{
    public async Task<GetHouseholdProfileQueryResponse> Handle(
        GetHouseholdProfileQuery request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var userId = currentUserService.UserId ?? 0;

        var response = await cacheService.GetOrSetAsync(
            CacheKeys.HouseholdProfile(request.ProjectId),
            async () =>
            {
                var profile = await householdService.GetProfileAsync(request.ProjectId, userId, cancellationToken);
                return GetHouseholdProfileQueryResponse.FromEntity(profile);
            },
            CacheExpiration.Medium,
            cancellationToken);

        return response!;
    }
}
