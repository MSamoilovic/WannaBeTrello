namespace Feezbow.Domain.Exceptions;

public class AIServiceException : DomainException
{
    public AIServiceException(string message) : base(message) { }
    public AIServiceException(string message, Exception inner) : base(message, inner) { }
}