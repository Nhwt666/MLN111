namespace MLN111.Entities;

public class QuizQuestion
{
    public Guid Id { get; set; }
    public Guid QuizRoomId { get; set; }
    public QuizRoom QuizRoom { get; set; } = null!;
    public string Content { get; set; } = string.Empty;
    public int OrderIndex { get; set; }

    public ICollection<QuizChoice> Choices { get; set; } = [];
}
