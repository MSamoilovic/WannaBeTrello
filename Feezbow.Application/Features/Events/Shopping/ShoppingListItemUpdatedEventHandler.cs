using MediatR;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Events.Shopping_Events;

namespace Feezbow.Application.Features.Events.Shopping;

public class ShoppingListItemUpdatedEventHandler(IShoppingListNotificationService notifications)
    : INotificationHandler<ShoppingListItemUpdatedEvent>
{
    public Task Handle(ShoppingListItemUpdatedEvent notification, CancellationToken cancellationToken)
        => notifications.NotifyItemUpdated(
            notification.ShoppingListId,
            notification.ProjectId,
            notification.ItemId,
            notification.UpdatedBy,
            notification.NewValues,
            cancellationToken);
}
