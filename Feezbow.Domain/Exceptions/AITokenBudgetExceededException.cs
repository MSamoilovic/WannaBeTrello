namespace Feezbow.Domain.Exceptions;

public class AITokenBudgetExceededException(int estimated, int limit)
: DomainException($"Estimated input tokens ({estimated}) exceed limit ({limit}).")
{
}