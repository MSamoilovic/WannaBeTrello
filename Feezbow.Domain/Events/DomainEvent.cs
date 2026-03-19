using MediatR;

namespace Feezbow.Domain.Events;

public abstract class DomainEvent: INotification
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}