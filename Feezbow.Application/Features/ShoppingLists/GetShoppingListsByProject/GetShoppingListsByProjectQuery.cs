using MediatR;

namespace Feezbow.Application.Features.ShoppingLists.GetShoppingListsByProject;

public record GetShoppingListsByProjectQuery(long ProjectId, bool IncludeArchived = false)
    : IRequest<IReadOnlyList<GetShoppingListsByProjectQueryResponse>>;
