namespace Feezbow.Domain.Events.Bill_Events;

public class BillPaidEvent(long billId, long projectId, decimal amount, long paidBy) : DomainEvent
{
    public long BillId => billId;
    public long ProjectId => projectId;
    public decimal Amount => amount;
    public long PaidBy => paidBy;
}
