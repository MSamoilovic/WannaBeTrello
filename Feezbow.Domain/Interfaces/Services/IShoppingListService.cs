using Feezbow.Domain.Entities;

namespace Feezbow.Domain.Interfaces.Services;

public interface IShoppingListService
{
    Task<ShoppingList> CreateListAsync(
        long projectId,
        string name,
        long userId,
        CancellationToken cancellationToken = default);

    Task<ShoppingList> GetByIdAsync(
        long shoppingListId,
        long userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ShoppingList>> GetByProjectAsync(
        long projectId,
        long userId,
        bool includeArchived,
        CancellationToken cancellationToken = default);

    Task<long> RenameListAsync(
        long shoppingListId,
        string name,
        long userId,
        CancellationToken cancellationToken = default);

    Task<long> ArchiveListAsync(
        long shoppingListId,
        long userId,
        CancellationToken cancellationToken = default);

    Task<long> DeleteListAsync(
        long shoppingListId,
        long userId,
        CancellationToken cancellationToken = default);

    Task<(long ProjectId, long ItemId)> AddItemAsync(
        long shoppingListId,
        string name,
        decimal? quantity,
        string? unit,
        string? notes,
        long userId,
        CancellationToken cancellationToken = default);

    Task<long> UpdateItemAsync(
        long shoppingListId,
        long itemId,
        string? name,
        decimal? quantity,
        string? unit,
        string? notes,
        long userId,
        CancellationToken cancellationToken = default);

    Task<long> RemoveItemAsync(
        long shoppingListId,
        long itemId,
        long userId,
        CancellationToken cancellationToken = default);

    Task<long> ToggleItemPurchasedAsync(
        long shoppingListId,
        long itemId,
        bool isPurchased,
        long userId,
        CancellationToken cancellationToken = default);
}
