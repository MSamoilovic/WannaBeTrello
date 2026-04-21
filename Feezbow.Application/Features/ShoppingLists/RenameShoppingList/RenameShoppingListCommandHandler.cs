using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities.Common;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.ShoppingLists.RenameShoppingList;

public class RenameShoppingListCommandHandler(
    IShoppingListService shoppingListService,
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<RenameShoppingListCommand, Result<long>>
{
    public async Task<Result<long>> Handle(RenameShoppingListCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var userId = currentUserService.UserId ?? 0;

        var projectId = await shoppingListService.RenameListAsync(
            request.ShoppingListId,
            request.Name,
            userId,
            cancellationToken);

        await cacheService.RemoveAsync(CacheKeys.ProjectShoppingLists(projectId), cancellationToken);
        await cacheService.RemoveAsync(CacheKeys.ShoppingList(request.ShoppingListId), cancellationToken);

        return Result<long>.Success(request.ShoppingListId, "Shopping list renamed successfully.");
    }
}
