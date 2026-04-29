using MediatR;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Events.Shopping_Events;

namespace Feezbow.Application.Features.Events.Shopping;

public class ShoppingListItemAddedEventHandler(IShoppingListNotificationService notifications)
    : INotificationHandler<ShoppingListItemAddedEvent>
{
    public Task Handle(ShoppingListItemAddedEvent notification, CancellationToken cancellationToken)
        => notifications.NotifyItemAdded(
            notification.ShoppingListId,
            notification.ProjectId,
            notification.ItemId,
            notification.Name,
            notification.AddedBy,
            cancellationToken);
}
