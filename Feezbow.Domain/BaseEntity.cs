namespace WannabeTrello.Domain;

public abstract class BaseEntity<T> where T : struct
{
    public T Id { get; internal init; }
}