using System.ComponentModel.DataAnnotations;

namespace MLN111.Dto.Quiz;

public sealed class CreateQuizRoomRequest
{
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    [Range(5, 120)]
    public int SecondsPerQuestion { get; set; } = 20;
}
