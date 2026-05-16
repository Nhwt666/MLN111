namespace MLN111.Dto.Quiz;

public sealed record LeaderboardEntryDto(
    int Rank,
    string DisplayName,
    int TotalScore);

public sealed record LeaderboardResponse(
    Guid RoomId,
    string Title,
    IReadOnlyList<LeaderboardEntryDto> Entries);
