using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.ShoppingLists.GetShoppingListById;

public class GetShoppingListByIdQueryHandler(
    IShoppingListService shoppingListService,
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<GetShoppingListByIdQuery, GetShoppingListByIdQueryResponse>
{
    public async Task<GetShoppingListByIdQueryResponse> Handle(
        GetShoppingListByIdQuery request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var userId = currentUserService.UserId ?? 0;

        var response = await cacheService.GetOrSetAsync(
            CacheKeys.ShoppingList(request.ShoppingListId),
            async () =>
            {
                var list = await shoppingListService.GetByIdAsync(request.ShoppingListId, userId, cancellationToken);
                return GetShoppingListByIdQueryResponse.FromEntity(list);
            },
            CacheExpiration.Medium,
            cancellationToken);

        return response!;
    }
}
