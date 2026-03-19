using Feezbow.Domain.Events;

namespace Feezbow.Domain.Events.UserEvents;

public class UserDeactivatedEvent(long userId, long deactivatedByUserId) : DomainEvent
{
    public long UserId => userId;
    public long DeactivatedByUserId => deactivatedByUserId;
}

