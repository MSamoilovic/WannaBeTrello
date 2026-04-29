using MediatR;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Events.Bill_Events;

namespace Feezbow.Application.Features.Events.Bill;

public class BillCreatedEventHandler(IBillNotificationService notifications)
    : INotificationHandler<BillCreatedEvent>
{
    public Task Handle(BillCreatedEvent notification, CancellationToken cancellationToken)
        => notifications.NotifyBillCreated(
            notification.BillId,
            notification.ProjectId,
            notification.Title,
            notification.Amount,
            notification.Currency,
            notification.CreatedBy,
            cancellationToken);
}
