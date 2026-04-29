using MediatR;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Events.Shopping_Events;

namespace Feezbow.Application.Features.Events.Shopping;

public class ShoppingListCreatedEventHandler(IShoppingListNotificationService notifications)
    : INotificationHandler<ShoppingListCreatedEvent>
{
    public Task Handle(ShoppingListCreatedEvent notification, CancellationToken cancellationToken)
        => notifications.NotifyListCreated(
            notification.ShoppingListId,
            notification.ProjectId,
            notification.Name,
            notification.CreatedBy,
            cancellationToken);
}
