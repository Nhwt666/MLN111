namespace MLN111.Services.Ai;

public enum AiProtocol
{
    OpenAiCompatible,
    XaiResponses,
    Gemini
}

public sealed record AiProviderPreset(
    string DisplayName,
    string BaseUrl,
    string ApiKeyEnvironmentVariable,
    string DefaultModel,
    AiProtocol Protocol);

public static class AiProviderCatalog
{
    private static readonly Dictionary<string, AiProviderPreset> Presets =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["Grok"] = new("Grok", "https://api.x.ai/", "XAI_API_KEY", "grok-4.20-reasoning", AiProtocol.XaiResponses),
            ["Xai"] = new("Grok", "https://api.x.ai/", "XAI_API_KEY", "grok-4.20-reasoning", AiProtocol.XaiResponses),
            ["OpenAI"] = new("OpenAI", "https://api.openai.com/", "OPENAI_API_KEY", "gpt-5-nano", AiProtocol.OpenAiCompatible),
            ["OpenRouter"] = new("OpenRouter", "https://openrouter.ai/api/", "OPENROUTER_API_KEY", "openai/gpt-4o-mini", AiProtocol.OpenAiCompatible),
            ["Gemini"] = new("Gemini", "https://generativelanguage.googleapis.com/", "GEMINI_API_KEY", "gemini-1.5-flash", AiProtocol.Gemini),
            ["Google"] = new("Gemini", "https://generativelanguage.googleapis.com/", "GEMINI_API_KEY", "gemini-1.5-flash", AiProtocol.Gemini)
        };

    public static AiProviderPreset Resolve(string provider, string? customBaseUrl)
    {
        if (string.Equals(provider, "Custom", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(customBaseUrl))
                throw new InvalidOperationException("Ai:BaseUrl bat buoc khi Provider = Custom.");

            var url = customBaseUrl.Trim();
            if (!url.EndsWith('/'))
                url += '/';

            return new AiProviderPreset("Custom", url, "AI_API_KEY", string.Empty, AiProtocol.OpenAiCompatible);
        }

        if (Presets.TryGetValue(provider.Trim(), out var preset))
            return preset;

        throw new InvalidOperationException(
            $"Provider '{provider}' khong ho tro. Dung: Grok, OpenAI, OpenRouter, Gemini, Custom.");
    }
}
