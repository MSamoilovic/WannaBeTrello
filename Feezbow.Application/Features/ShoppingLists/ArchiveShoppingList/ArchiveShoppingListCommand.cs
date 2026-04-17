using MediatR;
using Feezbow.Domain.Entities.Common;

namespace Feezbow.Application.Features.ShoppingLists.ArchiveShoppingList;

public record ArchiveShoppingListCommand(long ShoppingListId) : IRequest<Result<long>>;
