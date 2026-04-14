using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities;
using Feezbow.Domain.Entities.Common;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces;
using Feezbow.Domain.Interfaces.Repositories;

namespace Feezbow.Application.Features.Household.CreateHouseholdProfile;

public class CreateHouseholdProfileCommandHandler(
    IUnitOfWork unitOfWork,
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

        var project = await unitOfWork.Projects.GetProjectWithMembersAsync(request.ProjectId, cancellationToken)
            ?? throw new NotFoundException("Project", request.ProjectId);

        if (!project.IsMember(userId))
            throw new AccessDeniedException("You are not a member of this project.");

        if (await unitOfWork.Households.ExistsForProjectAsync(request.ProjectId, cancellationToken))
            throw new InvalidOperationDomainException("This project already has a household profile.");

        var profile = HouseholdProfile.Create(
            request.ProjectId,
            userId,
            request.Address,
            request.City,
            request.Timezone,
            request.ShoppingDay);

        await unitOfWork.Households.AddAsync(profile, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        await cacheService.RemoveAsync(CacheKeys.HouseholdProfile(request.ProjectId), cancellationToken);

        var result = Result<long>.Success(profile.Id, "Household profile created successfully.");
        return new CreateHouseholdProfileCommandResponse(result);
    }
}
