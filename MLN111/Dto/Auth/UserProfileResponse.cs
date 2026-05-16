using MLN111.Entities;

namespace MLN111.Dto.Auth;

public sealed record UserProfileResponse(
    Guid UserId,
    string Email,
    string DisplayName,
    UserRole Role);
