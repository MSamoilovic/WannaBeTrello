using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities.Common;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces;

namespace Feezbow.Application.Features.Household.AssignHouseholdRole;

public class AssignHouseholdRoleCommandHandler(
    IUnitOfWork unitOfWork,
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

        var profile = await unitOfWork.Households.GetByProjectIdWithMembersAsync(request.ProjectId, cancellationToken)
            ?? throw new NotFoundException("HouseholdProfile", request.ProjectId);

        if (!profile.Project.IsMember(userId))
            throw new AccessDeniedException("You are not a member of this project.");

        var member = profile.Project.ProjectMembers.FirstOrDefault(pm => pm.UserId == request.MemberId)
            ?? throw new NotFoundException("ProjectMember", request.MemberId);

        profile.AssignMemberRole(member, request.Role, userId);

        await unitOfWork.CompleteAsync(cancellationToken);

        await cacheService.RemoveAsync(CacheKeys.HouseholdMembers(request.ProjectId), cancellationToken);

        var result = Result<bool>.Success(true, "Household role assigned successfully.");
        return new AssignHouseholdRoleCommandResponse(result);
    }
}
