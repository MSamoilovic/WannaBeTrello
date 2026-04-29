using MediatR;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Events.Shopping_Events;

namespace Feezbow.Application.Features.Events.Shopping;

public class ShoppingListItemUnpurchasedEventHandler(IShoppingListNotificationService notifications)
    : INotificationHandler<ShoppingListItemUnpurchasedEvent>
{
    public Task Handle(ShoppingListItemUnpurchasedEvent notification, CancellationToken cancellationToken)
        => notifications.NotifyItemUnpurchased(
            notification.ShoppingListId,
            notification.ProjectId,
            notification.ItemId,
            notification.UpdatedBy,
            cancellationToken);
}
