using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities.Common;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.ShoppingLists.AddShoppingListItem;

public class AddShoppingListItemCommandHandler(
    IShoppingListService shoppingListService,
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<AddShoppingListItemCommand, Result<long>>
{
    public async Task<Result<long>> Handle(AddShoppingListItemCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var userId = currentUserService.UserId ?? 0;

        var (projectId, itemId) = await shoppingListService.AddItemAsync(
            request.ShoppingListId,
            request.Name,
            request.Quantity,
            request.Unit,
            request.Notes,
            userId,
            cancellationToken);

        await cacheService.RemoveAsync(CacheKeys.ShoppingList(request.ShoppingListId), cancellationToken);
        await cacheService.RemoveAsync(CacheKeys.ProjectShoppingLists(projectId), cancellationToken);

        return Result<long>.Success(itemId, "Item added successfully.");
    }
}
