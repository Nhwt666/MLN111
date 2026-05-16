using System.ComponentModel.DataAnnotations;

namespace MLN111.Dto.Quiz;

public sealed class SubmitAnswerRequest
{
    [Required]
    public Guid ChoiceId { get; set; }
}
