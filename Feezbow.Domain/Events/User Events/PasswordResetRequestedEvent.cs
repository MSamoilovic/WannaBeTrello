namespace WannabeTrello.Domain.Events.UserEvents;

public class PasswordResetRequestedEvent(long userId, string email, string ipAddress, DateTime requestedAt): DomainEvent
{
    public long UserId => userId;
    public string Email => email;
    public string IpAddress => ipAddress;
    public DateTime RequestedAt => requestedAt;
}
