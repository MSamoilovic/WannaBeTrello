using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities.Common;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.ShoppingLists.ToggleItemPurchased;

public class ToggleItemPurchasedCommandHandler(
    IShoppingListService shoppingListService,
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<ToggleItemPurchasedCommand, Result<long>>
{
    public async Task<Result<long>> Handle(ToggleItemPurchasedCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var userId = currentUserService.UserId ?? 0;

        var projectId = await shoppingListService.ToggleItemPurchasedAsync(
            request.ShoppingListId,
            request.ItemId,
            request.IsPurchased,
            userId,
            cancellationToken);

        await cacheService.RemoveAsync(CacheKeys.ShoppingList(request.ShoppingListId), cancellationToken);
        await cacheService.RemoveAsync(CacheKeys.ProjectShoppingLists(projectId), cancellationToken);

        return Result<long>.Success(request.ItemId,
            request.IsPurchased ? "Item marked as purchased." : "Item marked as not purchased.");
    }
}
