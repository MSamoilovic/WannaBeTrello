using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities.Common;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces;

namespace Feezbow.Application.Features.ShoppingLists.RenameShoppingList;

public class RenameShoppingListCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<RenameShoppingListCommand, Result<long>>
{
    public async Task<Result<long>> Handle(RenameShoppingListCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var userId = currentUserService.UserId ?? 0;

        var list = await unitOfWork.ShoppingLists.GetByIdAsync(request.ShoppingListId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.ShoppingList), request.ShoppingListId);

        if (!list.Project.IsMember(userId))
            throw new AccessDeniedException("You are not a member of this project.");

        list.Rename(request.Name, userId);
        await unitOfWork.CompleteAsync(cancellationToken);

        await cacheService.RemoveAsync(CacheKeys.ProjectShoppingLists(list.ProjectId), cancellationToken);
        await cacheService.RemoveAsync(CacheKeys.ShoppingList(list.Id), cancellationToken);

        return Result<long>.Success(list.Id, "Shopping list renamed successfully.");
    }
}
