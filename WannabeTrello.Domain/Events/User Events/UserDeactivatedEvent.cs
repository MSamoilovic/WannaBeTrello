using WannabeTrello.Domain.Events;

namespace WannabeTrello.Domain.Events.UserEvents;

public class UserDeactivatedEvent(long userId, long deactivatedByUserId) : DomainEvent
{
    public long UserId => userId;
    public long DeactivatedByUserId => deactivatedByUserId;
}

