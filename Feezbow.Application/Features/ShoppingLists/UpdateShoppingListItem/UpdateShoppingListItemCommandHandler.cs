using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities.Common;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.ShoppingLists.UpdateShoppingListItem;

public class UpdateShoppingListItemCommandHandler(
    IShoppingListService shoppingListService,
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<UpdateShoppingListItemCommand, Result<long>>
{
    public async Task<Result<long>> Handle(UpdateShoppingListItemCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var userId = currentUserService.UserId ?? 0;

        var projectId = await shoppingListService.UpdateItemAsync(
            request.ShoppingListId,
            request.ItemId,
            request.Name,
            request.Quantity,
            request.Unit,
            request.Notes,
            userId,
            cancellationToken);

        await cacheService.RemoveAsync(CacheKeys.ShoppingList(request.ShoppingListId), cancellationToken);
        await cacheService.RemoveAsync(CacheKeys.ProjectShoppingLists(projectId), cancellationToken);

        return Result<long>.Success(request.ItemId, "Item updated successfully.");
    }
}
