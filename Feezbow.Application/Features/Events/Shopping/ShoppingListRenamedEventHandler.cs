using MediatR;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Events.Shopping_Events;

namespace Feezbow.Application.Features.Events.Shopping;

public class ShoppingListRenamedEventHandler(IShoppingListNotificationService notifications)
    : INotificationHandler<ShoppingListRenamedEvent>
{
    public Task Handle(ShoppingListRenamedEvent notification, CancellationToken cancellationToken)
        => notifications.NotifyListRenamed(
            notification.ShoppingListId,
            notification.ProjectId,
            notification.OldName,
            notification.NewName,
            notification.UpdatedBy,
            cancellationToken);
}
