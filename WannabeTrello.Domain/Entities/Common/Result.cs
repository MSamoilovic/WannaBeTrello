namespace WannabeTrello.Domain.Entities.Common;

public class Result<T>
{
    internal Result(T value, string message, bool isSuccess)
    {
        Value = value;
        Message = message;
        IsSuccess = isSuccess;
    }

    public static Result<T> Success(T value, string message = "")
    {
        return new Result<T>(value, message, true);
    }

    public static Result<T> Fail(T value, string message = "")
    {
        return new Result<T>(value, message, false);
    }
    
    public T Value { get; private set; }
    public string Message { get; private set; }
    public bool IsSuccess { get; private set; }
    
    public bool IsError => !IsSuccess;
}