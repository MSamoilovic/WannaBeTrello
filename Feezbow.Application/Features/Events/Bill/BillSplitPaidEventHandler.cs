using MediatR;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Events.Bill_Events;

namespace Feezbow.Application.Features.Events.Bill;

public class BillSplitPaidEventHandler(IBillNotificationService notifications)
    : INotificationHandler<BillSplitPaidEvent>
{
    public Task Handle(BillSplitPaidEvent notification, CancellationToken cancellationToken)
        => notifications.NotifyBillSplitPaid(
            notification.BillId,
            notification.ProjectId,
            notification.UserId,
            notification.Amount,
            notification.PaidBy,
            cancellationToken);
}
