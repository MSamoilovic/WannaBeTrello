using WannabeTrello.Domain.Events;

namespace WannabeTrello.Domain.Events.UserEvents;

public class UserProfileUpdatedEvent(
    long userId,
    IReadOnlyDictionary<string, object?> oldValues,
    IReadOnlyDictionary<string, object?> newValues,
    long modifyingUserId) : DomainEvent
{
    public long UserId => userId;
    public IReadOnlyDictionary<string, object?> OldValues => oldValues;
    public IReadOnlyDictionary<string, object?> NewValues => newValues;
    public long ModifyingUserId => modifyingUserId;
}

