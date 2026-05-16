using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MLN111.Data;
using MLN111.Entities;

namespace MLN111.Infrastructure;

public static class AdminUserSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<SeedAdminOptions>>().Value;
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("AdminUserSeeder");

        if (string.IsNullOrWhiteSpace(options.AdminEmail) || string.IsNullOrWhiteSpace(options.AdminPassword))
        {
            logger.LogWarning("Bo qua seed admin: chua cau hinh Seed:AdminEmail / Seed:AdminPassword.");
            return;
        }

        var email = options.AdminEmail.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(u => u.Email == email))
            return;

        db.Users.Add(new AppUser
        {
            Id = Guid.NewGuid(),
            Email = email,
            DisplayName = options.AdminDisplayName.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(options.AdminPassword),
            Role = UserRole.Admin,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        });

        await db.SaveChangesAsync();
        logger.LogInformation("Da tao tai khoan admin seed: {Email}", email);
    }
}
