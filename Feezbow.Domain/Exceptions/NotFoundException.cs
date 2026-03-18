namespace WannabeTrello.Domain.Exceptions;

public class NotFoundException(string name, object key) : DomainException($"Entity \'{name}\' ({key}) was not found.");