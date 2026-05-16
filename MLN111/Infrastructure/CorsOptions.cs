namespace MLN111.Infrastructure;

public sealed class CorsOptions
{
    public const string SectionName = "Cors";

    public bool AllowAnyOrigin { get; set; }
    public string[] AllowedOrigins { get; set; } = [];
}
