using Survey_Basket.Application.Abstraction;

namespace Survey_Basket.Application.Errors;

public static class PollErrors
{
    public static readonly Error PollNotFound = new("Poll.NotFound", "Poll not found.");
    public static readonly Error PollAlreadyExists = new("Poll.NotFound", "Poll already exists.");
    public static readonly Error PollCreationFailed = new("Poll.NotFound", "Poll creation failed.");
    public static readonly Error PollUpdateFailed = new("Poll.NotFound", "Poll update failed.");
    public static readonly Error PollDeletionFailed = new("Poll.NotFound", "Poll deletion failed.");
}
