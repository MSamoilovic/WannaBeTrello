using MediatR;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Events.Shopping_Events;

namespace Feezbow.Application.Features.Events.Shopping;

public class ShoppingListItemPurchasedEventHandler(IShoppingListNotificationService notifications)
    : INotificationHandler<ShoppingListItemPurchasedEvent>
{
    public Task Handle(ShoppingListItemPurchasedEvent notification, CancellationToken cancellationToken)
        => notifications.NotifyItemPurchased(
            notification.ShoppingListId,
            notification.ProjectId,
            notification.ItemId,
            notification.PurchasedBy,
            cancellationToken);
}
