using Survey_Basket.Application.Abstraction;

namespace Survey_Basket.Application.Errors;

public static class VoteErrors
{
    public static readonly Error VoteNotFound = new("Vote.NotFound", "Vote not found.");
    public static readonly Error VoteAlreadyExists = new("Vote.AlreadyExists", "Vote already exists.");
    public static readonly Error VoteCreationFailed = new("Vote.CreationFailed", "Vote creation failed.");
    public static readonly Error VoteUpdateFailed = new("Vote.UpdateFailed", "Vote update failed.");
    public static readonly Error VoteDeletionFailed = new("Vote.DeletionFailed", "Vote deletion failed.");
}
