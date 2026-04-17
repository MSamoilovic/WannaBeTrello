using Feezbow.Domain.Entities;

namespace Feezbow.Domain.Interfaces.Repositories;

public interface IShoppingListRepository
{
    Task<ShoppingList?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<ShoppingList?> GetByIdWithItemsAsync(long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ShoppingList>> GetByProjectAsync(long projectId, bool includeArchived, CancellationToken cancellationToken = default);
    Task AddAsync(ShoppingList list, CancellationToken cancellationToken = default);
    void Remove(ShoppingList list);
}
