namespace Survey_Basket.Api.Models;

public sealed record ServiceError(string Description, int Code);

public sealed class ServiceResult<T>
{
    public bool Succeeded { get; init; }
    public T? Data { get; init; }
    public ServiceError? Error { get; init; }

    public static ServiceResult<T> Success(T data) => new()
    {
        Succeeded = true,
        Data = data
    };

    public static ServiceResult<T> Failed(ServiceError error) => new()
    {
        Succeeded = false,
        Error = error
    };
}
