using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Polly;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Infrastructure.SignalR.Contracts;
using Feezbow.Infrastructure.SignalR.Hubs;
using Feezbow.Infrastructure.SignalR.Resilience;

namespace Feezbow.Infrastructure.Services.Notifications;

public class ShoppingListNotificationService(
    IHubContext<ProjectHub, IProjectHubClient> projectHub,
    ResiliencePipeline pipeline,
    ILogger<ShoppingListNotificationService> logger)
    : ResilientNotificationBase(pipeline, logger), IShoppingListNotificationService
{
    private static string Group(long projectId) => $"Project:{projectId}";

    public Task NotifyListCreated(long shoppingListId, long projectId, string name, long createdBy, CancellationToken cancellationToken = default)
        => SendAsync(_ => new ValueTask(projectHub.Clients.Group(Group(projectId))
            .ShoppingListCreated(new ShoppingListCreatedNotification
            {
                ShoppingListId = shoppingListId, ProjectId = projectId, Name = name, CreatedBy = createdBy
            })), cancellationToken);

    public Task NotifyListRenamed(long shoppingListId, long projectId, string oldName, string newName, long modifiedBy, CancellationToken cancellationToken = default)
        => SendAsync(_ => new ValueTask(projectHub.Clients.Group(Group(projectId))
            .ShoppingListRenamed(new ShoppingListRenamedNotification
            {
                ShoppingListId = shoppingListId, ProjectId = projectId, OldName = oldName, NewName = newName, ModifiedBy = modifiedBy
            })), cancellationToken);

    public Task NotifyListArchived(long shoppingListId, long projectId, long archivedBy, CancellationToken cancellationToken = default)
        => SendAsync(_ => new ValueTask(projectHub.Clients.Group(Group(projectId))
            .ShoppingListArchived(new ShoppingListArchivedNotification
            {
                ShoppingListId = shoppingListId, ProjectId = projectId, ArchivedBy = archivedBy
            })), cancellationToken);

    public Task NotifyItemAdded(long shoppingListId, long projectId, long itemId, string name, long addedBy, CancellationToken cancellationToken = default)
        => SendAsync(_ => new ValueTask(projectHub.Clients.Group(Group(projectId))
            .ShoppingListItemAdded(new ShoppingListItemAddedNotification
            {
                ShoppingListId = shoppingListId, ProjectId = projectId, ItemId = itemId, Name = name, AddedBy = addedBy
            })), cancellationToken);

    public Task NotifyItemUpdated(long shoppingListId, long projectId, long itemId, long modifiedBy, IDictionary<string, object?> changes, CancellationToken cancellationToken = default)
        => SendAsync(_ => new ValueTask(projectHub.Clients.Group(Group(projectId))
            .ShoppingListItemUpdated(new ShoppingListItemUpdatedNotification
            {
                ShoppingListId = shoppingListId, ProjectId = projectId, ItemId = itemId, ModifiedBy = modifiedBy, Changes = changes
            })), cancellationToken);

    public Task NotifyItemRemoved(long shoppingListId, long projectId, long itemId, long removedBy, CancellationToken cancellationToken = default)
        => SendAsync(_ => new ValueTask(projectHub.Clients.Group(Group(projectId))
            .ShoppingListItemRemoved(new ShoppingListItemRemovedNotification
            {
                ShoppingListId = shoppingListId, ProjectId = projectId, ItemId = itemId, RemovedBy = removedBy
            })), cancellationToken);

    public Task NotifyItemPurchased(long shoppingListId, long projectId, long itemId, long purchasedBy, CancellationToken cancellationToken = default)
        => SendAsync(_ => new ValueTask(projectHub.Clients.Group(Group(projectId))
            .ShoppingListItemPurchased(new ShoppingListItemPurchasedNotification
            {
                ShoppingListId = shoppingListId, ProjectId = projectId, ItemId = itemId, PurchasedBy = purchasedBy
            })), cancellationToken);

    public Task NotifyItemUnpurchased(long shoppingListId, long projectId, long itemId, long modifiedBy, CancellationToken cancellationToken = default)
        => SendAsync(_ => new ValueTask(projectHub.Clients.Group(Group(projectId))
            .ShoppingListItemUnpurchased(new ShoppingListItemUnpurchasedNotification
            {
                ShoppingListId = shoppingListId, ProjectId = projectId, ItemId = itemId, ModifiedBy = modifiedBy
            })), cancellationToken);
}
