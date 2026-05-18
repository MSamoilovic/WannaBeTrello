namespace Feezbow.Domain.Exceptions;

public class AIResponseParseException(string typeName)
 : DomainException($"Failed to parse AI's response as {typeName}.")
{
}