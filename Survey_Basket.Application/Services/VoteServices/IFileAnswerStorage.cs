namespace Survey_Basket.Application.Services.VoteServices;

public interface IFileAnswerStorage
{
    Task<string> SaveAsync(Guid pollId, Guid questionId, Guid userId, string fileName, string contentType, byte[] content, CancellationToken cancellationToken = default);
}
