namespace WannabeTrello.Domain.Events.User_Events
{
    internal class EmailConfirmationFailedEvent(string email,
    string reason,
    string ipAddress,
    DateTime attemptedAt) : DomainEvent
    {
        public string Email => email;
        public string Reason => reason;
        public string IpAddress => ipAddress;
        public DateTime AttemptedAt => attemptedAt;
    }
}
