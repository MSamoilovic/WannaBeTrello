using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities.Common;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces;

namespace Feezbow.Application.Features.ShoppingLists.DeleteShoppingList;

public class DeleteShoppingListCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<DeleteShoppingListCommand, Result<long>>
{
    public async Task<Result<long>> Handle(DeleteShoppingListCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var userId = currentUserService.UserId ?? 0;

        var list = await unitOfWork.ShoppingLists.GetByIdAsync(request.ShoppingListId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.ShoppingList), request.ShoppingListId);

        if (!list.Project.IsMember(userId))
            throw new AccessDeniedException("You are not a member of this project.");

        var projectId = list.ProjectId;
        unitOfWork.ShoppingLists.Remove(list);
        await unitOfWork.CompleteAsync(cancellationToken);

        await cacheService.RemoveAsync(CacheKeys.ProjectShoppingLists(projectId), cancellationToken);
        await cacheService.RemoveAsync(CacheKeys.ShoppingList(request.ShoppingListId), cancellationToken);

        return Result<long>.Success(request.ShoppingListId, "Shopping list deleted successfully.");
    }
}
