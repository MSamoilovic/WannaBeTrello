using Feezbow.Domain.Entities.Common;

namespace Feezbow.Application.Features.ShoppingLists.CreateShoppingList;

public record CreateShoppingListCommandResponse(Result<long> Result);
