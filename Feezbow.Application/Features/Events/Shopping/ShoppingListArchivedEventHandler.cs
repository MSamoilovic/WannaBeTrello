using MediatR;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Events.Shopping_Events;

namespace Feezbow.Application.Features.Events.Shopping;

public class ShoppingListArchivedEventHandler(IShoppingListNotificationService notifications)
    : INotificationHandler<ShoppingListArchivedEvent>
{
    public Task Handle(ShoppingListArchivedEvent notification, CancellationToken cancellationToken)
        => notifications.NotifyListArchived(
            notification.ShoppingListId,
            notification.ProjectId,
            notification.ArchivedBy,
            cancellationToken);
}
