namespace WannabeTrello.Domain.Events.User_Events;

public class EmailConfirmationSuccededEvent(long userId,
string email,
string ipAddress,
DateTime confirmedAt): DomainEvent
{
    public long UserId => userId;
    public string Email => email;
    public string IpAddress => ipAddress;
    public DateTime ConfirmedAt => confirmedAt;

}
