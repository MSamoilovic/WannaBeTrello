using Feezbow.Domain.Entities;
using Feezbow.Domain.Events.Shopping_Events;
using Feezbow.Domain.Exceptions;

namespace Feezbow.Domain.Tests.Entities;

public class ShoppingListTests
{
    private const long ProjectId = 11L;
    private const long UserId = 7L;

    [Fact]
    [Trait("Category", "Domain")]
    public void Create_ValidArguments_ReturnsInitializedList()
    {
        var list = ShoppingList.Create(ProjectId, "  Weekly groceries  ", UserId);

        Assert.Equal(ProjectId, list.ProjectId);
        Assert.Equal("Weekly groceries", list.Name);
        Assert.False(list.IsArchived);
        Assert.Equal(UserId, list.CreatedBy);
        Assert.Empty(list.Items);
        Assert.Single(list.DomainEvents);
        Assert.IsType<ShoppingListCreatedEvent>(list.DomainEvents.First());
    }

    [Theory]
    [Trait("Category", "Domain")]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_InvalidName_Throws(string name)
    {
        Assert.Throws<BusinessRuleValidationException>(() => ShoppingList.Create(ProjectId, name, UserId));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Create_NonPositiveProjectId_Throws()
    {
        Assert.Throws<BusinessRuleValidationException>(() => ShoppingList.Create(0, "Groceries", UserId));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Create_NonPositiveCreatedBy_Throws()
    {
        Assert.Throws<BusinessRuleValidationException>(() => ShoppingList.Create(ProjectId, "Groceries", 0));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Rename_ChangesName_AndRaisesEvent()
    {
        var list = ShoppingList.Create(ProjectId, "Old", UserId);
        list.ClearDomainEvents();

        list.Rename("New", UserId);

        Assert.Equal("New", list.Name);
        Assert.Single(list.DomainEvents);
        var evt = Assert.IsType<ShoppingListRenamedEvent>(list.DomainEvents.First());
        Assert.Equal("Old", evt.OldName);
        Assert.Equal("New", evt.NewName);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Rename_SameName_DoesNotRaiseEvent()
    {
        var list = ShoppingList.Create(ProjectId, "Groceries", UserId);
        list.ClearDomainEvents();

        list.Rename("Groceries", UserId);

        Assert.Empty(list.DomainEvents);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Archive_SetsFlag_AndRaisesEvent()
    {
        var list = ShoppingList.Create(ProjectId, "Groceries", UserId);
        list.ClearDomainEvents();

        list.Archive(UserId);

        Assert.True(list.IsArchived);
        Assert.Single(list.DomainEvents);
        Assert.IsType<ShoppingListArchivedEvent>(list.DomainEvents.First());
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Archive_AlreadyArchived_NoOp()
    {
        var list = ShoppingList.Create(ProjectId, "Groceries", UserId);
        list.Archive(UserId);
        list.ClearDomainEvents();

        list.Archive(UserId);

        Assert.Empty(list.DomainEvents);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Restore_AfterArchive_ClearsFlag()
    {
        var list = ShoppingList.Create(ProjectId, "Groceries", UserId);
        list.Archive(UserId);

        list.Restore(UserId);

        Assert.False(list.IsArchived);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void AddItem_AppendsItem_AndRaisesEvent()
    {
        var list = ShoppingList.Create(ProjectId, "Groceries", UserId);
        list.ClearDomainEvents();

        var item = list.AddItem("Milk", 2m, "L", "whole", UserId);

        Assert.Single(list.Items);
        Assert.Equal("Milk", item.Name);
        Assert.Equal(2m, item.Quantity);
        Assert.Equal("L", item.Unit);
        Assert.Equal("whole", item.Notes);
        Assert.False(item.IsPurchased);
        Assert.Single(list.DomainEvents);
        Assert.IsType<ShoppingListItemAddedEvent>(list.DomainEvents.First());
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void AddItem_ArchivedList_Throws()
    {
        var list = ShoppingList.Create(ProjectId, "Groceries", UserId);
        list.Archive(UserId);

        Assert.Throws<InvalidOperationDomainException>(() =>
            list.AddItem("Milk", 1m, null, null, UserId));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void AddItem_NegativeQuantity_Throws()
    {
        var list = ShoppingList.Create(ProjectId, "Groceries", UserId);

        Assert.Throws<BusinessRuleValidationException>(() =>
            list.AddItem("Milk", -1m, null, null, UserId));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void UpdateItem_ChangesFields_AndRaisesEvent()
    {
        var list = ShoppingList.Create(ProjectId, "Groceries", UserId);
        var item = list.AddItem("Milk", 1m, "L", null, UserId);
        list.ClearDomainEvents();

        list.UpdateItem(item.Id, "Soy milk", 2m, null, "lactose-free", UserId);

        Assert.Equal("Soy milk", item.Name);
        Assert.Equal(2m, item.Quantity);
        Assert.Equal("lactose-free", item.Notes);
        Assert.Single(list.DomainEvents);
        Assert.IsType<ShoppingListItemUpdatedEvent>(list.DomainEvents.First());
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void UpdateItem_NoChanges_DoesNotRaiseEvent()
    {
        var list = ShoppingList.Create(ProjectId, "Groceries", UserId);
        var item = list.AddItem("Milk", 1m, "L", null, UserId);
        list.ClearDomainEvents();

        list.UpdateItem(item.Id, null, null, null, null, UserId);

        Assert.Empty(list.DomainEvents);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void UpdateItem_UnknownId_Throws()
    {
        var list = ShoppingList.Create(ProjectId, "Groceries", UserId);

        Assert.Throws<NotFoundException>(() =>
            list.UpdateItem(999L, "X", null, null, null, UserId));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void RemoveItem_RemovesAndRaisesEvent()
    {
        var list = ShoppingList.Create(ProjectId, "Groceries", UserId);
        var item = list.AddItem("Milk", 1m, null, null, UserId);
        list.ClearDomainEvents();

        list.RemoveItem(item.Id, UserId);

        Assert.Empty(list.Items);
        Assert.Single(list.DomainEvents);
        Assert.IsType<ShoppingListItemRemovedEvent>(list.DomainEvents.First());
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void MarkItemPurchased_SetsFlags_AndRaisesEvent()
    {
        var list = ShoppingList.Create(ProjectId, "Groceries", UserId);
        var item = list.AddItem("Milk", 1m, null, null, UserId);
        list.ClearDomainEvents();

        list.MarkItemPurchased(item.Id, UserId);

        Assert.True(item.IsPurchased);
        Assert.NotNull(item.PurchasedAt);
        Assert.Equal(UserId, item.PurchasedBy);
        Assert.Single(list.DomainEvents);
        Assert.IsType<ShoppingListItemPurchasedEvent>(list.DomainEvents.First());
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void MarkItemPurchased_AlreadyPurchased_NoOp()
    {
        var list = ShoppingList.Create(ProjectId, "Groceries", UserId);
        var item = list.AddItem("Milk", 1m, null, null, UserId);
        list.MarkItemPurchased(item.Id, UserId);
        list.ClearDomainEvents();

        list.MarkItemPurchased(item.Id, UserId);

        Assert.Empty(list.DomainEvents);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void MarkItemUnpurchased_ClearsFlags_AndRaisesEvent()
    {
        var list = ShoppingList.Create(ProjectId, "Groceries", UserId);
        var item = list.AddItem("Milk", 1m, null, null, UserId);
        list.MarkItemPurchased(item.Id, UserId);
        list.ClearDomainEvents();

        list.MarkItemUnpurchased(item.Id, UserId);

        Assert.False(item.IsPurchased);
        Assert.Null(item.PurchasedAt);
        Assert.Null(item.PurchasedBy);
        Assert.Single(list.DomainEvents);
        Assert.IsType<ShoppingListItemUnpurchasedEvent>(list.DomainEvents.First());
    }
}
