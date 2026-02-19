namespace Survey_Basket.Application.Contracts.Votes;

public sealed record FileAnswerRequest(
    string FileName,
    string ContentType,
    string Base64Content
);
