using Microsoft.EntityFrameworkCore;
using MLN111.Data;
using MLN111.Dto.Auth;
using MLN111.Entities;
using MLN111.Services.Common;

namespace MLN111.Services.Auth;

public sealed class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IJwtTokenService _jwt;

    public AuthService(AppDbContext db, IJwtTokenService jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    public async Task<ServiceResult<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Password != request.ConfirmPassword)
            return ServiceResult<AuthResponse>.Fail("Mat khau nhap lai khong khop.");

        var email = request.Email.Trim().ToLowerInvariant();
        if (await _db.Users.AnyAsync(u => u.Email == email, cancellationToken))
            return ServiceResult<AuthResponse>.Fail("Email da duoc su dung.");

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            Email = email,
            DisplayName = request.DisplayName.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = UserRole.User,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync(cancellationToken);
        return ServiceResult<AuthResponse>.Ok(BuildAuthResponse(user));
    }

    public async Task<ServiceResult<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        if (user is null || !user.IsActive)
            return ServiceResult<AuthResponse>.Fail("Email hoac mat khau khong dung.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return ServiceResult<AuthResponse>.Fail("Email hoac mat khau khong dung.");

        return ServiceResult<AuthResponse>.Ok(BuildAuthResponse(user));
    }

    public async Task<ServiceResult<UserProfileResponse>> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user is null)
            return ServiceResult<UserProfileResponse>.Fail("Khong tim thay nguoi dung.");

        return ServiceResult<UserProfileResponse>.Ok(
            new UserProfileResponse(user.Id, user.Email, user.DisplayName, user.Role));
    }

    private AuthResponse BuildAuthResponse(AppUser user)
    {
        var (token, expires) = _jwt.CreateToken(user);
        return new AuthResponse(token, expires, user.Id, user.Email, user.DisplayName, user.Role);
    }
}
