namespace MLN111.Infrastructure;

public sealed class SeedAdminOptions
{
    public const string SectionName = "Seed";

    public string AdminEmail { get; set; } = "admin@mln111.local";
    public string AdminPassword { get; set; } = "Admin@123456";
    public string AdminDisplayName { get; set; } = "Admin MLN111";
}
