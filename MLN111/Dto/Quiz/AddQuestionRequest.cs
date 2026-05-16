using System.ComponentModel.DataAnnotations;

namespace MLN111.Dto.Quiz;

public sealed class AddQuestionRequest
{
    [Required, MaxLength(2000)]
    public string Content { get; set; } = string.Empty;

    [MinLength(2)]
    public List<AddChoiceRequest> Choices { get; set; } = [];
}

public sealed class AddChoiceRequest
{
    [Required, MaxLength(500)]
    public string Text { get; set; } = string.Empty;

    public bool IsCorrect { get; set; }
}
