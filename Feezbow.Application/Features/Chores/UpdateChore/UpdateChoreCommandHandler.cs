using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities.Common;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces;

namespace Feezbow.Application.Features.Chores.UpdateChore;

public class UpdateChoreCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<UpdateChoreCommand, UpdateChoreCommandResponse>
{
    public async Task<UpdateChoreCommandResponse> Handle(
        UpdateChoreCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var userId = currentUserService.UserId ?? 0;

        var chore = await unitOfWork.Chores.GetByIdAsync(request.ChoreId, cancellationToken)
            ?? throw new NotFoundException("Chore", request.ChoreId);

        if (!chore.Project.IsMember(userId))
            throw new AccessDeniedException("You are not a member of this project.");

        chore.Update(request.Title, request.Description, request.DueDate, request.Priority, userId);

        await unitOfWork.CompleteAsync(cancellationToken);

        await cacheService.RemoveAsync(CacheKeys.ProjectChores(chore.ProjectId), cancellationToken);

        return new UpdateChoreCommandResponse(Result<bool>.Success(true, "Chore updated successfully."));
    }
}
