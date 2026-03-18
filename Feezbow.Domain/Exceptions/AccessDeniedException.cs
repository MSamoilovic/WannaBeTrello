namespace WannabeTrello.Domain.Exceptions;

public class AccessDeniedException(string message = "Pristup traženom resursu je zabranjen.") : DomainException(message);