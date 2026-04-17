using Feezbow.Domain.Exceptions;

namespace Feezbow.Domain.Entities;

public class ShoppingListItem : AuditableEntity
{
    public long ShoppingListId { get; private set; }
    public string Name { get; private set; } = null!;
    public decimal? Quantity { get; private set; }
    public string? Unit { get; private set; }
    public string? Notes { get; private set; }
    public bool IsPurchased { get; private set; }
    public DateTime? PurchasedAt { get; private set; }
    public long? PurchasedBy { get; private set; }

    private ShoppingListItem() { }

    internal static ShoppingListItem Create(
        long shoppingListId,
        string name,
        decimal? quantity,
        string? unit,
        string? notes,
        long createdBy)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessRuleValidationException("Item name cannot be empty.");

        if (quantity is < 0)
            throw new BusinessRuleValidationException("Quantity cannot be negative.");

        if (createdBy <= 0)
            throw new BusinessRuleValidationException("CreatedBy must be a positive number.");

        return new ShoppingListItem
        {
            ShoppingListId = shoppingListId,
            Name = name.Trim(),
            Quantity = quantity,
            Unit = string.IsNullOrWhiteSpace(unit) ? null : unit.Trim(),
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
            IsPurchased = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

    internal bool Update(
        string? name,
        decimal? quantity,
        string? unit,
        string? notes,
        long updatedBy,
        out IDictionary<string, object?> oldValues,
        out IDictionary<string, object?> newValues)
    {
        oldValues = new Dictionary<string, object?>();
        newValues = new Dictionary<string, object?>();
        var changed = false;

        if (!string.IsNullOrWhiteSpace(name))
        {
            var normalized = name.Trim();
            if (!string.Equals(Name, normalized, StringComparison.Ordinal))
            {
                oldValues[nameof(Name)] = Name;
                Name = normalized;
                newValues[nameof(Name)] = Name;
                changed = true;
            }
        }

        if (quantity.HasValue)
        {
            if (quantity.Value < 0)
                throw new BusinessRuleValidationException("Quantity cannot be negative.");

            if (Quantity != quantity.Value)
            {
                oldValues[nameof(Quantity)] = Quantity;
                Quantity = quantity.Value;
                newValues[nameof(Quantity)] = Quantity;
                changed = true;
            }
        }

        var normalizedUnit = string.IsNullOrWhiteSpace(unit) ? null : unit!.Trim();
        if (unit is not null && !string.Equals(Unit, normalizedUnit, StringComparison.Ordinal))
        {
            oldValues[nameof(Unit)] = Unit;
            Unit = normalizedUnit;
            newValues[nameof(Unit)] = Unit;
            changed = true;
        }

        var normalizedNotes = string.IsNullOrWhiteSpace(notes) ? null : notes!.Trim();
        if (notes is not null && !string.Equals(Notes, normalizedNotes, StringComparison.Ordinal))
        {
            oldValues[nameof(Notes)] = Notes;
            Notes = normalizedNotes;
            newValues[nameof(Notes)] = Notes;
            changed = true;
        }

        if (!changed) return false;

        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = updatedBy;
        return true;
    }

    internal void MarkPurchased(long purchasedBy)
    {
        if (purchasedBy <= 0)
            throw new BusinessRuleValidationException("PurchasedBy must be a positive number.");

        if (IsPurchased) return;

        IsPurchased = true;
        PurchasedAt = DateTime.UtcNow;
        PurchasedBy = purchasedBy;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = purchasedBy;
    }

    internal void MarkUnpurchased(long updatedBy)
    {
        if (updatedBy <= 0)
            throw new BusinessRuleValidationException("UpdatedBy must be a positive number.");

        if (!IsPurchased) return;

        IsPurchased = false;
        PurchasedAt = null;
        PurchasedBy = null;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = updatedBy;
    }
}
