using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities.Common;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.ShoppingLists.CreateShoppingList;

public class CreateShoppingListCommandHandler(
    IShoppingListService shoppingListService,
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<CreateShoppingListCommand, CreateShoppingListCommandResponse>
{
    public async Task<CreateShoppingListCommandResponse> Handle(
        CreateShoppingListCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var userId = currentUserService.UserId ?? 0;

        var list = await shoppingListService.CreateListAsync(
            request.ProjectId,
            request.Name,
            userId,
            cancellationToken);

        await cacheService.RemoveAsync(CacheKeys.ProjectShoppingLists(request.ProjectId), cancellationToken);

        var result = Result<long>.Success(list.Id, "Shopping list created successfully.");
        return new CreateShoppingListCommandResponse(result);
    }
}
