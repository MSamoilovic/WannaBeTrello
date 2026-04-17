using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities.Common;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces;

namespace Feezbow.Application.Features.ShoppingLists.AddShoppingListItem;

public class AddShoppingListItemCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<AddShoppingListItemCommand, Result<long>>
{
    public async Task<Result<long>> Handle(AddShoppingListItemCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var userId = currentUserService.UserId ?? 0;

        var list = await unitOfWork.ShoppingLists.GetByIdWithItemsAsync(request.ShoppingListId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.ShoppingList), request.ShoppingListId);

        if (!list.Project.IsMember(userId))
            throw new AccessDeniedException("You are not a member of this project.");

        var item = list.AddItem(request.Name, request.Quantity, request.Unit, request.Notes, userId);
        await unitOfWork.CompleteAsync(cancellationToken);

        await cacheService.RemoveAsync(CacheKeys.ShoppingList(list.Id), cancellationToken);
        await cacheService.RemoveAsync(CacheKeys.ProjectShoppingLists(list.ProjectId), cancellationToken);

        return Result<long>.Success(item.Id, "Item added successfully.");
    }
}
