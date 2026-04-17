using MediatR;
using Feezbow.Domain.Entities.Common;

namespace Feezbow.Application.Features.ShoppingLists.UpdateShoppingListItem;

public record UpdateShoppingListItemCommand(
    long ShoppingListId,
    long ItemId,
    string? Name,
    decimal? Quantity,
    string? Unit,
    string? Notes) : IRequest<Result<long>>;
