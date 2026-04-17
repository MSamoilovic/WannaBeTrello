using MediatR;

namespace Feezbow.Application.Features.ShoppingLists.CreateShoppingList;

public record CreateShoppingListCommand(long ProjectId, string Name) : IRequest<CreateShoppingListCommandResponse>;
