namespace MLN111.Dto.Quiz;

public sealed record SubmitAnswerResponse(
    bool IsCorrect,
    int PointsEarned,
    int TotalScore);
