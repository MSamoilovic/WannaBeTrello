using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities.Common;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Household.AssignHouseholdRole;

public class AssignHouseholdRoleCommandHandler(
    IHouseholdService householdService,
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<AssignHouseholdRoleCommand, AssignHouseholdRoleCommandResponse>
{
    public async Task<AssignHouseholdRoleCommandResponse> Handle(
        AssignHouseholdRoleCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var userId = currentUserService.UserId ?? 0;

        await householdService.AssignRoleAsync(
            request.ProjectId,
            request.MemberId,
            request.Role,
            userId,
            cancellationToken);

        await cacheService.RemoveAsync(CacheKeys.HouseholdMembers(request.ProjectId), cancellationToken);

        var result = Result<bool>.Success(true, "Household role assigned successfully.");
        return new AssignHouseholdRoleCommandResponse(result);
    }
}
