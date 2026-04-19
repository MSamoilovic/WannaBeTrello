using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities.Common;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces;

namespace Feezbow.Application.Features.Chores.CompleteChore;

public class CompleteChoreCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<CompleteChoreCommand, CompleteChoreCommandResponse>
{
    public async Task<CompleteChoreCommandResponse> Handle(
        CompleteChoreCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var userId = currentUserService.UserId ?? 0;

        var chore = await unitOfWork.Chores.GetByIdAsync(request.ChoreId, cancellationToken)
            ?? throw new NotFoundException("Chore", request.ChoreId);

        if (!chore.Project.IsMember(userId))
            throw new AccessDeniedException("You are not a member of this project.");

        var nextChore = chore.Complete(userId);

        long? nextChoreId = null;
        if (nextChore is not null)
        {
            await unitOfWork.Chores.AddAsync(nextChore, cancellationToken);
        }

        await unitOfWork.CompleteAsync(cancellationToken);

        if (nextChore is not null)
            nextChoreId = nextChore.Id;

        await cacheService.RemoveAsync(CacheKeys.ProjectChores(chore.ProjectId), cancellationToken);

        var message = nextChoreId.HasValue
            ? $"Chore completed. Next occurrence created (ID: {nextChoreId.Value})."
            : "Chore completed.";

        return new CompleteChoreCommandResponse(Result<long?>.Success(nextChoreId, message));
    }
}
