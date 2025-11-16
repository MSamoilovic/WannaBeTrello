namespace WannabeTrello.Domain.Events.Auth_Events;

public class PasswordResetFailedEvent(string email,
    string reason,
    string ipAddress,
    DateTime attemptedAt): DomainEvent
{
    public string Email => email;
    public string Reason => reason;
    public string IpAddress => ipAddress;
    public DateTime AttemptedAt => attemptedAt;
}
