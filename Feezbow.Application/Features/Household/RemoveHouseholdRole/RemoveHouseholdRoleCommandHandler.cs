using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities.Common;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Household.RemoveHouseholdRole;

public class RemoveHouseholdRoleCommandHandler(
    IHouseholdService householdService,
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<RemoveHouseholdRoleCommand, RemoveHouseholdRoleCommandResponse>
{
    public async Task<RemoveHouseholdRoleCommandResponse> Handle(
        RemoveHouseholdRoleCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var userId = currentUserService.UserId ?? 0;

        await householdService.RemoveRoleAsync(
            request.ProjectId,
            request.MemberId,
            userId,
            cancellationToken);

        await cacheService.RemoveAsync(CacheKeys.HouseholdMembers(request.ProjectId), cancellationToken);

        var result = Result<bool>.Success(true, "Household role removed successfully.");
        return new RemoveHouseholdRoleCommandResponse(result);
    }
}
