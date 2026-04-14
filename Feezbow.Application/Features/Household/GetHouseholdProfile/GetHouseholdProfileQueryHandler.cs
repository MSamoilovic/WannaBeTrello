using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces;

namespace Feezbow.Application.Features.Household.GetHouseholdProfile;

public class GetHouseholdProfileQueryHandler(
    IUnitOfWork unitOfWork,
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
                var profile = await unitOfWork.Households.GetByProjectIdAsync(request.ProjectId, cancellationToken)
                    ?? throw new NotFoundException("HouseholdProfile", request.ProjectId);

                if (!profile.Project.IsMember(userId))
                    throw new AccessDeniedException("You are not a member of this project.");

                return GetHouseholdProfileQueryResponse.FromEntity(profile);
            },
            CacheExpiration.Medium,
            cancellationToken);

        return response!;
    }
}
