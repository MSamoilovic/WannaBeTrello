using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities.Common;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.ShoppingLists.RemoveShoppingListItem;

public class RemoveShoppingListItemCommandHandler(
    IShoppingListService shoppingListService,
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<RemoveShoppingListItemCommand, Result<long>>
{
    public async Task<Result<long>> Handle(RemoveShoppingListItemCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var userId = currentUserService.UserId ?? 0;

        var projectId = await shoppingListService.RemoveItemAsync(
            request.ShoppingListId,
            request.ItemId,
            userId,
            cancellationToken);

        await cacheService.RemoveAsync(CacheKeys.ShoppingList(request.ShoppingListId), cancellationToken);
        await cacheService.RemoveAsync(CacheKeys.ProjectShoppingLists(projectId), cancellationToken);

        return Result<long>.Success(request.ItemId, "Item removed successfully.");
    }
}
