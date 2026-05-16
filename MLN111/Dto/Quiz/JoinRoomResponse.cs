namespace MLN111.Dto.Quiz;

public sealed record JoinRoomResponse(
    Guid RoomId,
    Guid ParticipantId,
    string Title,
    string JoinCode);
