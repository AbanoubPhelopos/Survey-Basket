using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Survey_Basket.Application.Abstraction;

public static class ResultExtensions
{
    public static ObjectResult ToProblemDetails(this Result result, int statusCode)
    {
        if (result.IsSuccess)
        {
            throw new InvalidOperationException("Cannot convert a successful result to ProblemDetails.");
        }

        var problem = Results.Problem(statusCode: statusCode);
        var problemDetails = problem.GetType().GetProperty("ProblemDetails")?.GetValue(problem) as ProblemDetails;

        problemDetails!.Extensions = new Dictionary<string, object?>
        {
            { "error", new[] { result.Error } }
        };

        return new ObjectResult(problemDetails);

    }
}
