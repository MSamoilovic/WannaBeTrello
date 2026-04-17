using MediatR;
using Feezbow.Domain.Entities.Common;

namespace Feezbow.Application.Features.ShoppingLists.DeleteShoppingList;

public record DeleteShoppingListCommand(long ShoppingListId) : IRequest<Result<long>>;
