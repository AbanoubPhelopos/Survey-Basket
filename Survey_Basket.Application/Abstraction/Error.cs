namespace Survey_Basket.Application.Abstraction;

public record Error(string Code, string Message, int? statusCode)
{
    public static Error None => new(string.Empty, string.Empty, null);
};
