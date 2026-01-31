using Microsoft.AspNetCore.Http;
using Survey_Basket.Application.Abstractions;

namespace Survey_Basket.Application.Errors;

public static class PollErrors
{
    public static readonly Error PollNotFound = new("Poll.NotFound", "Poll not found.", StatusCodes.Status404NotFound);
    public static readonly Error PollAlreadyExists = new("Poll.AlreadyExists", "Poll already exists.", StatusCodes.Status409Conflict);
    public static readonly Error PollCreationFailed = new("Poll.CreationFailed", "Poll creation failed.", StatusCodes.Status500InternalServerError);
    public static readonly Error PollUpdateFailed = new("Poll.UpdateFailed", "Poll update failed.", StatusCodes.Status500InternalServerError);
    public static readonly Error PollDeletionFailed = new("Poll.DeletionFailed", "Poll deletion failed.", StatusCodes.Status500InternalServerError);
}
