using MLN111.Infrastructure;

namespace MLN111.Services.Ai;

public sealed record ResolvedAiConfig(
    bool Enabled,
    string ProviderName,
    string ApiKey,
    string Model,
    string BaseUrl,
    AiProtocol Protocol);

public static class AiOptionsResolver
{
    public static ResolvedAiConfig Resolve(AiOptions options)
    {
        var preset = AiProviderCatalog.Resolve(options.Provider, options.BaseUrl);

        var apiKey = options.ApiKey;
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            apiKey = Environment.GetEnvironmentVariable(preset.ApiKeyEnvironmentVariable)
                ?? Environment.GetEnvironmentVariable("AI_API_KEY")
                ?? Environment.GetEnvironmentVariable("Ai__ApiKey")
                ?? string.Empty;
        }

        var model = string.IsNullOrWhiteSpace(options.Model) ? preset.DefaultModel : options.Model.Trim();
        if (string.IsNullOrWhiteSpace(model))
            throw new InvalidOperationException("Ai:Model trong hoac thieu default cho provider nay.");

        return new ResolvedAiConfig(
            options.Enabled,
            preset.DisplayName,
            apiKey,
            model,
            preset.BaseUrl,
            preset.Protocol);
    }
}
