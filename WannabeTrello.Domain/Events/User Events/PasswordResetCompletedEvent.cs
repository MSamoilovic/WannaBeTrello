namespace WannabeTrello.Domain.Events.UserEvents;

public class PasswordResetCompletedEvent(long userId, string email, string ipAddress, DateTime CompletedAt): DomainEvent
{
    public long UserId => userId;
    public string Email => email;
    public string IpAddress => ipAddress;
    public DateTime CompletedAt => CompletedAt;
}
