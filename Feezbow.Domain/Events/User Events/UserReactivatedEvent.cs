using Feezbow.Domain.Events;

namespace Feezbow.Domain.Events.UserEvents;

public class UserReactivatedEvent(long userId, long reactivatedByUserId) : DomainEvent
{
    public long UserId => userId;
    public long ReactivatedByUserId => reactivatedByUserId;
}

