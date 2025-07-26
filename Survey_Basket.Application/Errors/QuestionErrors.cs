using Survey_Basket.Application.Abstraction;

namespace Survey_Basket.Application.Errors;

public static class QuestionErrors
{
    public static readonly Error QuestionNotFound = new("Question.NotFound", "Question not found.");
    public static readonly Error QuestionAlreadyExists = new("Question.AlreadyExists", "Question already exists.");
    public static readonly Error QuestionCreationFailed = new("Question.CreationFailed", "Question creation failed.");
    public static readonly Error QuestionUpdateFailed = new("Question.UpdateFailed", "Question update failed.");
    public static readonly Error QuestionDeletionFailed = new("Question.DeletionFailed", "Question deletion failed.");
}
