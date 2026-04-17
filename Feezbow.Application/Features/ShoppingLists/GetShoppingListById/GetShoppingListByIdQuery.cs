using MediatR;

namespace Feezbow.Application.Features.ShoppingLists.GetShoppingListById;

public record GetShoppingListByIdQuery(long ShoppingListId) : IRequest<GetShoppingListByIdQueryResponse>;
