using MediatR;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Events.Shopping_Events;

namespace Feezbow.Application.Features.Events.Shopping;

public class ShoppingListItemRemovedEventHandler(IShoppingListNotificationService notifications)
    : INotificationHandler<ShoppingListItemRemovedEvent>
{
    public Task Handle(ShoppingListItemRemovedEvent notification, CancellationToken cancellationToken)
        => notifications.NotifyItemRemoved(
            notification.ShoppingListId,
            notification.ProjectId,
            notification.ItemId,
            notification.RemovedBy,
            cancellationToken);
}
