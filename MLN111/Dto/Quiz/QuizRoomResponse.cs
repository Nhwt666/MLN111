using MLN111.Entities;

namespace MLN111.Dto.Quiz;

public sealed record QuizRoomResponse(
    Guid Id,
    string Title,
    string? Description,
    string JoinCode,
    QuizRoomStatus Status,
    int SecondsPerQuestion,
    int QuestionCount,
    DateTimeOffset CreatedAt);
