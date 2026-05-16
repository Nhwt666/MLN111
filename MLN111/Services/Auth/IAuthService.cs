using MLN111.Dto.Auth;
using MLN111.Services.Common;

namespace MLN111.Services.Auth;

public interface IAuthService
{
    Task<ServiceResult<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult<UserProfileResponse>> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default);
}
