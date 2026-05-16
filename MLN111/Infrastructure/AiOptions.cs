namespace MLN111.Infrastructure;

public sealed class AiOptions
{
    public const string SectionName = "Ai";

    public string Provider { get; set; } = "Grok";
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
}
