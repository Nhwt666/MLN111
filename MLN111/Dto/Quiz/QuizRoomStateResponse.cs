using MLN111.Entities;

namespace MLN111.Dto.Quiz;

public sealed record QuizRoomStateResponse(
    Guid RoomId,
    string Title,
    QuizRoomStatus Status,
    int SecondsPerQuestion,
    int? CurrentQuestionIndex,
    DateTimeOffset? QuestionStartedAt,
    int? SecondsRemaining,
    int ParticipantCount,
    int QuestionCount,
    QuestionStateDto? CurrentQuestion,
    Guid? MyParticipantId,
    int? MyTotalScore,
    bool HasAnsweredCurrent);

public sealed record QuestionStateDto(
    Guid QuestionId,
    string Content,
    int OrderIndex,
    IReadOnlyList<ChoiceStateDto> Choices);

public sealed record ChoiceStateDto(Guid Id, string Text);
