using Microsoft.AspNetCore.Http;
using Survey_Basket.Application.Abstractions;

namespace Survey_Basket.Application.Errors;

public static class QuestionErrors
{
    public static readonly Error QuestionNotFound = new("Question.NotFound", "Question not found.", StatusCodes.Status404NotFound);
    public static readonly Error QuestionAlreadyExists = new("Question.AlreadyExists", "Question already exists.", StatusCodes.Status409Conflict);
    public static readonly Error QuestionCreationFailed = new("Question.CreationFailed", "Question creation failed.", StatusCodes.Status500InternalServerError);
    public static readonly Error QuestionUpdateFailed = new("Question.UpdateFailed", "Question update failed.", StatusCodes.Status500InternalServerError);
    public static readonly Error QuestionDeletionFailed = new("Question.DeletionFailed", "Question deletion failed.", StatusCodes.Status500InternalServerError);
}
