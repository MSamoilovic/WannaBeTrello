using MediatR;
using Feezbow.Domain.Entities.Common;

namespace Feezbow.Application.Features.ShoppingLists.RemoveShoppingListItem;

public record RemoveShoppingListItemCommand(long ShoppingListId, long ItemId) : IRequest<Result<long>>;
