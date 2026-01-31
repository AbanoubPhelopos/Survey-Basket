using Mapster;
using Survey_Basket.Application.Abstractions;
using Survey_Basket.Application.Contracts.Votes;
using Survey_Basket.Application.Errors;
using Survey_Basket.Domain.Abstractions;
using Survey_Basket.Domain.Entities;
using Survey_Basket.Domain.Models;


namespace Survey_Basket.Application.Services.VoteServices;

public class VoteService(IUnitOfWork unitOfWork) : IVoteService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result> AddAsync(Guid pollId, Guid userId, VoteRequest request, CancellationToken cancellationToken = default)
    {
        var hasVote = await _unitOfWork.Repository<Vote>().AnyAsync(x => x.PollId == pollId && x.UserId == userId, cancellationToken);

        if (hasVote)
            return Result.Failure(VoteErrors.VoteAlreadyExists);

        var isPollExist = await _unitOfWork.Repository<Poll>().AnyAsync(p => p.Id == pollId && p.IsPublished && p.StartedAt <= DateOnly.FromDateTime(DateTime.UtcNow)
                            && p.EndedAt >= DateOnly.FromDateTime(DateTime.UtcNow), cancellationToken);
        if (!isPollExist)
            return Result.Failure(PollErrors.PollNotFound);

        var availableQuestions = await _unitOfWork.Repository<Question>()
                .GetAllAsync(x => x.PollId == pollId && x.IsActive, cancellationToken);

        var availableQuestionIds = availableQuestions.Select(x => x.Id).ToList();

        if (!request.Answers.Select(x => x.QuestionId).SequenceEqual(availableQuestionIds))
            return Result.Failure(VoteErrors.InvalidQuestionsInVote);

        var vote = new Vote
        {
            PollId = pollId,
            UserId = userId,
            Answers = request.Answers.Adapt<IEnumerable<VoteAnswers>>().ToList()
        };
        await _unitOfWork.Repository<Vote>().AddAsync(vote, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();

    }
}
