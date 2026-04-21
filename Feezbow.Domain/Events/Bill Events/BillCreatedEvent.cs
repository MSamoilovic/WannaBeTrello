namespace Feezbow.Domain.Events.Bill_Events;

public class BillCreatedEvent(long billId, long projectId, string title, decimal amount, string currency, long createdBy) : DomainEvent
{
    public long BillId => billId;
    public long ProjectId => projectId;
    public string Title => title;
    public decimal Amount => amount;
    public string Currency => currency;
    public long CreatedBy => createdBy;
}
