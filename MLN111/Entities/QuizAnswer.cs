namespace MLN111.Entities;

public class QuizAnswer
{
    public Guid Id { get; set; }
    public Guid ParticipantId { get; set; }
    public QuizParticipant Participant { get; set; } = null!;
    public Guid QuestionId { get; set; }
    public QuizQuestion Question { get; set; } = null!;
    public Guid ChoiceId { get; set; }
    public QuizChoice Choice { get; set; } = null!;
    public DateTimeOffset AnsweredAt { get; set; }
    public int? ResponseTimeMs { get; set; }
    public bool IsCorrect { get; set; }
    public int PointsEarned { get; set; }
}
