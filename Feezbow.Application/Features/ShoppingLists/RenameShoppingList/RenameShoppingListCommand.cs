using MediatR;
using Feezbow.Domain.Entities.Common;

namespace Feezbow.Application.Features.ShoppingLists.RenameShoppingList;

public record RenameShoppingListCommand(long ShoppingListId, string Name) : IRequest<Result<long>>;
