using Microsoft.AspNetCore.Http;
using Survey_Basket.Application.Abstraction;

namespace Survey_Basket.Application.Errors;

public static class VoteErrors
{
    public static readonly Error VoteNotFound = new("Vote.NotFound", "Vote not found.", StatusCodes.Status404NotFound);
    public static readonly Error VoteAlreadyExists = new("Vote.AlreadyExists", "Vote already exists.", StatusCodes.Status409Conflict);
    public static readonly Error VoteCreationFailed = new("Vote.CreationFailed", "Vote creation failed.", StatusCodes.Status409Conflict);
    public static readonly Error VoteUpdateFailed = new("Vote.UpdateFailed", "Vote update failed.", StatusCodes.Status409Conflict);
    public static readonly Error VoteDeletionFailed = new("Vote.DeletionFailed", "Vote deletion failed.", StatusCodes.Status409Conflict);
    public static readonly Error InvalidQuestionsInVote = new("Vote.InvalidQuestions", "Vote contains invalid questions.", StatusCodes.Status400BadRequest);
}
