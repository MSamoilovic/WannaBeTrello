namespace WannabeTrello.Domain.Events.User_Events;

public class EmailConfirmationRequestedEvent(long id, string? email, string ipAddress, DateTime date): DomainEvent
{
    public long Id => id;
    public string? Email => email;
    public string IpAddress => ipAddress;
    public DateTime Date => date;
}
