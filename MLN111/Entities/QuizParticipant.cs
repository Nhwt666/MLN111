namespace MLN111.Entities;

public class QuizParticipant
{
    public Guid Id { get; set; }
    public Guid QuizRoomId { get; set; }
    public QuizRoom QuizRoom { get; set; } = null!;
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public string DisplayNameSnapshot { get; set; } = string.Empty;
    public DateTimeOffset JoinedAt { get; set; }
    public int TotalScore { get; set; }

    public ICollection<QuizAnswer> Answers { get; set; } = [];
}
