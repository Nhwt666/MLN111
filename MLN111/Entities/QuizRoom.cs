namespace MLN111.Entities;

public class QuizRoom
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string JoinCode { get; set; } = string.Empty;
    public QuizRoomStatus Status { get; set; } = QuizRoomStatus.Draft;

    public int SecondsPerQuestion { get; set; } = QuizSessionDefaults.SecondsPerQuestion;
    public int? CurrentQuestionIndex { get; set; }
    public DateTimeOffset? QuestionStartedAt { get; set; }
    public DateTimeOffset? SessionStartedAt { get; set; }
    public DateTimeOffset? SessionFinishedAt { get; set; }

    public Guid CreatedById { get; set; }
    public AppUser CreatedBy { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    public ICollection<QuizQuestion> Questions { get; set; } = [];
    public ICollection<QuizParticipant> Participants { get; set; } = [];
}
