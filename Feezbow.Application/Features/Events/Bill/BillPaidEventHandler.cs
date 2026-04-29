using MediatR;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Events.Bill_Events;

namespace Feezbow.Application.Features.Events.Bill;

public class BillPaidEventHandler(IBillNotificationService notifications)
    : INotificationHandler<BillPaidEvent>
{
    public Task Handle(BillPaidEvent notification, CancellationToken cancellationToken)
        => notifications.NotifyBillPaid(
            notification.BillId,
            notification.ProjectId,
            notification.Amount,
            notification.PaidBy,
            cancellationToken);
}
