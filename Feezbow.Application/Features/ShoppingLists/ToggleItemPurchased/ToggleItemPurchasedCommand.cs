using MediatR;
using Feezbow.Domain.Entities.Common;

namespace Feezbow.Application.Features.ShoppingLists.ToggleItemPurchased;

public record ToggleItemPurchasedCommand(long ShoppingListId, long ItemId, bool IsPurchased) : IRequest<Result<long>>;
