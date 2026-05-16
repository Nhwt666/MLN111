using MLN111.Entities;

namespace MLN111.Services.Auth;

public interface IJwtTokenService
{
    (string Token, DateTimeOffset ExpiresAt) CreateToken(AppUser user);
}
