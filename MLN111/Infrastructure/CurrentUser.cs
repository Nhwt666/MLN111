using System.Security.Claims;

namespace MLN111.Infrastructure;

public static class CurrentUser
{
    public static Guid GetUserId(ClaimsPrincipal user)
    {
        var raw = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(raw) || !Guid.TryParse(raw, out var id))
            throw new UnauthorizedAccessException();
        return id;
    }
}
