using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities.Common;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces;

namespace Feezbow.Application.Features.Chores.AssignChore;

public class AssignChoreCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<AssignChoreCommand, AssignChoreCommandResponse>
{
    public async Task<AssignChoreCommandResponse> Handle(
        AssignChoreCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var userId = currentUserService.UserId ?? 0;

        var chore = await unitOfWork.Chores.GetByIdAsync(request.ChoreId, cancellationToken)
            ?? throw new NotFoundException("Chore", request.ChoreId);

        if (!chore.Project.IsMember(userId))
            throw new AccessDeniedException("You are not a member of this project.");

        if (request.AssignedToUserId.HasValue && !chore.Project.IsMember(request.AssignedToUserId.Value))
            throw new BusinessRuleValidationException("Assigned user is not a member of this project.");

        chore.Assign(request.AssignedToUserId, userId);

        await unitOfWork.CompleteAsync(cancellationToken);

        await cacheService.RemoveAsync(CacheKeys.ProjectChores(chore.ProjectId), cancellationToken);

        return new AssignChoreCommandResponse(Result<bool>.Success(true, "Chore assigned successfully."));
    }
}
