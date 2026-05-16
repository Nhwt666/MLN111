using MLN111.Entities;

namespace MLN111.Dto.Auth;

public sealed record AuthResponse(
    string Token,
    DateTimeOffset ExpiresAt,
    Guid UserId,
    string Email,
    string DisplayName,
    UserRole Role);
