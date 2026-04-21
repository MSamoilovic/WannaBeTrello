using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.ShoppingLists.GetShoppingListsByProject;

public class GetShoppingListsByProjectQueryHandler(
    IShoppingListService shoppingListService,
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<GetShoppingListsByProjectQuery, IReadOnlyList<GetShoppingListsByProjectQueryResponse>>
{
    public async Task<IReadOnlyList<GetShoppingListsByProjectQueryResponse>> Handle(
        GetShoppingListsByProjectQuery request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var userId = currentUserService.UserId ?? 0;

        if (request.IncludeArchived)
        {
            var lists = await shoppingListService.GetByProjectAsync(request.ProjectId, userId, true, cancellationToken);
            return lists.Select(GetShoppingListsByProjectQueryResponse.FromEntity).ToList();
        }

        var cached = await cacheService.GetOrSetAsync(
            CacheKeys.ProjectShoppingLists(request.ProjectId),
            async () =>
            {
                var lists = await shoppingListService.GetByProjectAsync(request.ProjectId, userId, false, cancellationToken);
                return lists.Select(GetShoppingListsByProjectQueryResponse.FromEntity).ToList();
            },
            CacheExpiration.Medium,
            cancellationToken);

        return cached ?? new List<GetShoppingListsByProjectQueryResponse>();
    }
}
