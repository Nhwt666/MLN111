namespace MLN111.Infrastructure;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "MLN111";
    public string Audience { get; set; } = "MLN111";
    public string Key { get; set; } = string.Empty;
    public int ExpiresMinutes { get; set; } = 120;
}
