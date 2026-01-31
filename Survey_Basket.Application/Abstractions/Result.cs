namespace Survey_Basket.Application.Abstractions;

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    public Error Error { get; } = default!;

    public Result(bool isSuccess, Error error)
    {

        if ((isSuccess && error != Error.None) || (!isSuccess && error == Error.None))
        {
            throw new InvalidOperationException();
        }
        IsSuccess = isSuccess;
        Error = error;

    }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);

    public static Result<T> Success<T>(T value) => new(true, Error.None, value);
    public static Result<T> Failure<T>(Error error) => new(false, error, default!);
}

public class Result<T>(bool isSuccess, Error error, T value) : Result(isSuccess, error)
{
    private readonly T _value = value;

    public T Value => IsSuccess ? _value : throw new InvalidOperationException("Cannot access value of a failed result.");
}
