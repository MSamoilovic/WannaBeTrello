using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities.Common;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces;

namespace Feezbow.Application.Features.Household.UpdateHouseholdProfile;

public class UpdateHouseholdProfileCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<UpdateHouseholdProfileCommand, UpdateHouseholdProfileCommandResponse>
{
    public async Task<UpdateHouseholdProfileCommandResponse> Handle(
        UpdateHouseholdProfileCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var userId = currentUserService.UserId ?? 0;

        var profile = await unitOfWork.Households.GetByProjectIdAsync(request.ProjectId, cancellationToken)
            ?? throw new NotFoundException("HouseholdProfile", request.ProjectId);

        if (!profile.Project.IsMember(userId))
            throw new AccessDeniedException("You are not a member of this project.");

        profile.Update(request.Address, request.City, request.Timezone, request.ShoppingDay, userId);

        await unitOfWork.CompleteAsync(cancellationToken);
        await cacheService.RemoveAsync(CacheKeys.HouseholdProfile(request.ProjectId), cancellationToken);

        var result = Result<long>.Success(profile.Id, "Household profile updated successfully.");
        return new UpdateHouseholdProfileCommandResponse(result);
    }
}
