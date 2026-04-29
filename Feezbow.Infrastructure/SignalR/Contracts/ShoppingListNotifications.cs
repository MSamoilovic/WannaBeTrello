namespace Feezbow.Infrastructure.SignalR.Contracts;

public record ShoppingListCreatedNotification
{
    public required long ShoppingListId { get; init; }
    public required long ProjectId { get; init; }
    public required string Name { get; init; }
    public required long CreatedBy { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
}

public record ShoppingListRenamedNotification
{
    public required long ShoppingListId { get; init; }
    public required long ProjectId { get; init; }
    public required string OldName { get; init; }
    public required string NewName { get; init; }
    public required long ModifiedBy { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
}

public record ShoppingListArchivedNotification
{
    public required long ShoppingListId { get; init; }
    public required long ProjectId { get; init; }
    public required long ArchivedBy { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
}

public record ShoppingListItemAddedNotification
{
    public required long ShoppingListId { get; init; }
    public required long ProjectId { get; init; }
    public required long ItemId { get; init; }
    public required string Name { get; init; }
    public required long AddedBy { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
}

public record ShoppingListItemUpdatedNotification
{
    public required long ShoppingListId { get; init; }
    public required long ProjectId { get; init; }
    public required long ItemId { get; init; }
    public required long ModifiedBy { get; init; }
    public required IDictionary<string, object?> Changes { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
}

public record ShoppingListItemRemovedNotification
{
    public required long ShoppingListId { get; init; }
    public required long ProjectId { get; init; }
    public required long ItemId { get; init; }
    public required long RemovedBy { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
}

public record ShoppingListItemPurchasedNotification
{
    public required long ShoppingListId { get; init; }
    public required long ProjectId { get; init; }
    public required long ItemId { get; init; }
    public required long PurchasedBy { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
}

public record ShoppingListItemUnpurchasedNotification
{
    public required long ShoppingListId { get; init; }
    public required long ProjectId { get; init; }
    public required long ItemId { get; init; }
    public required long ModifiedBy { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
}
