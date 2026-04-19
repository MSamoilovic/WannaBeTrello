using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities.Common;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces;

namespace Feezbow.Application.Features.Chores.DeleteChore;

public class DeleteChoreCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<DeleteChoreCommand, DeleteChoreCommandResponse>
{
    public async Task<DeleteChoreCommandResponse> Handle(
        DeleteChoreCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var userId = currentUserService.UserId ?? 0;

        var chore = await unitOfWork.Chores.GetByIdAsync(request.ChoreId, cancellationToken)
            ?? throw new NotFoundException("Chore", request.ChoreId);

        if (!chore.Project.IsMember(userId))
            throw new AccessDeniedException("You are not a member of this project.");

        var projectId = chore.ProjectId;

        unitOfWork.Chores.Remove(chore);
        await unitOfWork.CompleteAsync(cancellationToken);

        await cacheService.RemoveAsync(CacheKeys.ProjectChores(projectId), cancellationToken);

        return new DeleteChoreCommandResponse(Result<bool>.Success(true, "Chore deleted successfully."));
    }
}
