using Feezbow.Domain.Entities;

namespace Feezbow.Application.Features.ShoppingLists.GetShoppingListsByProject;

public class GetShoppingListsByProjectQueryResponse
{
    public long Id { get; set; }
    public long ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsArchived { get; set; }
    public int ItemCount { get; set; }
    public int PurchasedItemCount { get; set; }
    public DateTime CreatedAt { get; set; }

    public static GetShoppingListsByProjectQueryResponse FromEntity(ShoppingList list)
    {
        return new GetShoppingListsByProjectQueryResponse
        {
            Id = list.Id,
            ProjectId = list.ProjectId,
            Name = list.Name,
            IsArchived = list.IsArchived,
            ItemCount = list.Items.Count,
            PurchasedItemCount = list.Items.Count(i => i.IsPurchased),
            CreatedAt = list.CreatedAt
        };
    }
}
