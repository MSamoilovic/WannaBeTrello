using Feezbow.Domain.Entities;

namespace Feezbow.Application.Features.ShoppingLists.GetShoppingListById;

public class GetShoppingListByIdQueryResponse
{
    public long Id { get; set; }
    public long ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsArchived { get; set; }
    public DateTime CreatedAt { get; set; }
    public IReadOnlyList<ShoppingListItemResponse> Items { get; set; } = [];

    public static GetShoppingListByIdQueryResponse FromEntity(ShoppingList list)
    {
        return new GetShoppingListByIdQueryResponse
        {
            Id = list.Id,
            ProjectId = list.ProjectId,
            Name = list.Name,
            IsArchived = list.IsArchived,
            CreatedAt = list.CreatedAt,
            Items = list.Items
                .OrderBy(i => i.IsPurchased)
                .ThenBy(i => i.CreatedAt)
                .Select(ShoppingListItemResponse.FromEntity)
                .ToList()
        };
    }
}

public class ShoppingListItemResponse
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal? Quantity { get; set; }
    public string? Unit { get; set; }
    public string? Notes { get; set; }
    public bool IsPurchased { get; set; }
    public DateTime? PurchasedAt { get; set; }
    public long? PurchasedBy { get; set; }
    public DateTime CreatedAt { get; set; }

    public static ShoppingListItemResponse FromEntity(ShoppingListItem item)
    {
        return new ShoppingListItemResponse
        {
            Id = item.Id,
            Name = item.Name,
            Quantity = item.Quantity,
            Unit = item.Unit,
            Notes = item.Notes,
            IsPurchased = item.IsPurchased,
            PurchasedAt = item.PurchasedAt,
            PurchasedBy = item.PurchasedBy,
            CreatedAt = item.CreatedAt
        };
    }
}
