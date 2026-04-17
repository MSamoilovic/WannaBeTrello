using MediatR;
using Feezbow.Domain.Entities.Common;

namespace Feezbow.Application.Features.ShoppingLists.AddShoppingListItem;

public record AddShoppingListItemCommand(
    long ShoppingListId,
    string Name,
    decimal? Quantity,
    string? Unit,
    string? Notes) : IRequest<Result<long>>;
