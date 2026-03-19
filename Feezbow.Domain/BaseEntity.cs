namespace Feezbow.Domain;

public abstract class BaseEntity<T> where T : struct
{
    public T Id { get; internal init; }
}