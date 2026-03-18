using MediatR;

namespace WannabeTrello.Domain.Events;

public abstract class DomainEvent: INotification
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}