using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces;

namespace Feezbow.Application.Features.ShoppingLists.GetShoppingListById;

public class GetShoppingListByIdQueryHandler(
    IUnitOfWork unitOfWork,
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
                var list = await unitOfWork.ShoppingLists.GetByIdWithItemsAsync(request.ShoppingListId, cancellationToken)
                    ?? throw new NotFoundException(nameof(Domain.Entities.ShoppingList), request.ShoppingListId);

                if (!list.Project.IsMember(userId))
                    throw new AccessDeniedException("You are not a member of this project.");

                return GetShoppingListByIdQueryResponse.FromEntity(list);
            },
            CacheExpiration.Medium,
            cancellationToken);

        return response!;
    }
}
