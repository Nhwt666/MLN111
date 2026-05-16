namespace MLN111.Dto.Health;

public sealed record DatabaseHealthResponse(bool Connected, long ElapsedMilliseconds, string? Error);
