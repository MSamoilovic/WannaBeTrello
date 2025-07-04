namespace WannabeTrello.Domain.Exceptions;

public class NotFoundException(string name, object key) : DomainException($"Entitet \"{name}\" ({key}) nije pronađen.");