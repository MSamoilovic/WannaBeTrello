using Feezbow.Domain.Events.Shopping_Events;
using Feezbow.Domain.Exceptions;

namespace Feezbow.Domain.Entities;

public class ShoppingList : AuditableEntity
{
    public long ProjectId { get; private set; }
    public Project Project { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public bool IsArchived { get; private set; }

    private readonly List<ShoppingListItem> _items = [];
    public IReadOnlyCollection<ShoppingListItem> Items => _items.AsReadOnly();

    private ShoppingList() { }

    public static ShoppingList Create(long projectId, string name, long createdBy)
    {
        if (projectId <= 0)
            throw new BusinessRuleValidationException("ProjectId must be a positive number.");

        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessRuleValidationException("Shopping list name cannot be empty.");

        if (createdBy <= 0)
            throw new BusinessRuleValidationException("CreatedBy must be a positive number.");

        var list = new ShoppingList
        {
            ProjectId = projectId,
            Name = name.Trim(),
            IsArchived = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        list.AddDomainEvent(new ShoppingListCreatedEvent(list.Id, projectId, list.Name, createdBy));
        return list;
    }

    public void Rename(string name, long updatedBy)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessRuleValidationException("Shopping list name cannot be empty.");

        if (updatedBy <= 0)
            throw new BusinessRuleValidationException("UpdatedBy must be a positive number.");

        var normalized = name.Trim();
        if (string.Equals(Name, normalized, StringComparison.Ordinal)) return;

        var oldName = Name;
        Name = normalized;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = updatedBy;

        AddDomainEvent(new ShoppingListRenamedEvent(Id, ProjectId, oldName, Name, updatedBy));
    }

    public void Archive(long archivedBy)
    {
        if (archivedBy <= 0)
            throw new BusinessRuleValidationException("ArchivedBy must be a positive number.");

        if (IsArchived) return;

        IsArchived = true;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = archivedBy;

        AddDomainEvent(new ShoppingListArchivedEvent(Id, ProjectId, archivedBy));
    }

    public void Restore(long restoredBy)
    {
        if (restoredBy <= 0)
            throw new BusinessRuleValidationException("RestoredBy must be a positive number.");

        if (!IsArchived) return;

        IsArchived = false;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = restoredBy;
    }

    public ShoppingListItem AddItem(string name, decimal? quantity, string? unit, string? notes, long addedBy)
    {
        if (IsArchived)
            throw new InvalidOperationDomainException("Cannot add items to an archived shopping list.");

        var item = ShoppingListItem.Create(Id, name, quantity, unit, notes, addedBy);
        _items.Add(item);

        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = addedBy;

        AddDomainEvent(new ShoppingListItemAddedEvent(Id, ProjectId, item.Id, item.Name, addedBy));
        return item;
    }

    public void UpdateItem(long itemId, string? name, decimal? quantity, string? unit, string? notes, long updatedBy)
    {
        var item = FindItem(itemId);

        if (item.Update(name, quantity, unit, notes, updatedBy, out var oldValues, out var newValues))
        {
            LastModifiedAt = DateTime.UtcNow;
            LastModifiedBy = updatedBy;
            AddDomainEvent(new ShoppingListItemUpdatedEvent(Id, ProjectId, item.Id, updatedBy, oldValues, newValues));
        }
    }

    public void RemoveItem(long itemId, long removedBy)
    {
        if (removedBy <= 0)
            throw new BusinessRuleValidationException("RemovedBy must be a positive number.");

        var item = FindItem(itemId);
        _items.Remove(item);

        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = removedBy;

        AddDomainEvent(new ShoppingListItemRemovedEvent(Id, ProjectId, itemId, removedBy));
    }

    public void MarkItemPurchased(long itemId, long purchasedBy)
    {
        var item = FindItem(itemId);
        if (item.IsPurchased) return;

        item.MarkPurchased(purchasedBy);
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = purchasedBy;

        AddDomainEvent(new ShoppingListItemPurchasedEvent(Id, ProjectId, itemId, purchasedBy));
    }

    public void MarkItemUnpurchased(long itemId, long updatedBy)
    {
        var item = FindItem(itemId);
        if (!item.IsPurchased) return;

        item.MarkUnpurchased(updatedBy);
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = updatedBy;

        AddDomainEvent(new ShoppingListItemUnpurchasedEvent(Id, ProjectId, itemId, updatedBy));
    }

    private ShoppingListItem FindItem(long itemId)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new NotFoundException(nameof(ShoppingListItem), itemId);
        return item;
    }
}
