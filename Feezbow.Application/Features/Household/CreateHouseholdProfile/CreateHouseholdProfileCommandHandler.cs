using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities.Common;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Household.CreateHouseholdProfile;

public class CreateHouseholdProfileCommandHandler(
    IHouseholdService householdService,
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<CreateHouseholdProfileCommand, CreateHouseholdProfileCommandResponse>
{
    public async Task<CreateHouseholdProfileCommandResponse> Handle(
        CreateHouseholdProfileCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var userId = currentUserService.UserId ?? 0;

        var profile = await householdService.CreateProfileAsync(
            request.ProjectId,
            userId,
            request.Address,
            request.City,
            request.Timezone,
            request.ShoppingDay,
            cancellationToken);

        await cacheService.RemoveAsync(CacheKeys.HouseholdProfile(request.ProjectId), cancellationToken);

        var result = Result<long>.Success(profile.Id, "Household profile created successfully.");
        return new CreateHouseholdProfileCommandResponse(result);
    }
}
