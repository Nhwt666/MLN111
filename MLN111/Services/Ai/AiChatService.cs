using Microsoft.Extensions.Options;
using MLN111.Dto.Ai;
using MLN111.Infrastructure;
using MLN111.Services.Ai.Backends;
using MLN111.Services.Common;

namespace MLN111.Services.Ai;

public sealed class AiChatService : IAiChatService
{
    private readonly HttpClient _http;
    private readonly ResolvedAiConfig _config;
    private readonly IAiModelBackend _backend;

    public AiChatService(IHttpClientFactory httpClientFactory, IOptions<AiOptions> options)
    {
        _http = httpClientFactory.CreateClient("Ai");
        _config = AiOptionsResolver.Resolve(options.Value);
        _backend = CreateBackend(_config, _http);
    }

    public AiStatusResponse GetStatus() =>
        new(_config.Enabled, !string.IsNullOrWhiteSpace(_config.ApiKey), _config.Model, _config.ProviderName);

    public async Task<ServiceResult<ChatResponse>> AskAsync(string message, CancellationToken cancellationToken = default)
    {
        if (!_config.Enabled)
            return ServiceResult<ChatResponse>.Fail("Tinh nang AI dang tat (Ai:Enabled = false).");

        if (string.IsNullOrWhiteSpace(_config.ApiKey))
            return ServiceResult<ChatResponse>.Fail(
                $"Chua cau hinh Ai:ApiKey (hoac bien moi truong cho provider {_config.ProviderName}).");

        var result = await _backend.CompleteAsync(
            MlnChapterPrompts.Chapter3System,
            message.Trim(),
            cancellationToken);

        if (!result.Success)
            return ServiceResult<ChatResponse>.Fail(result.Error!);

        return ServiceResult<ChatResponse>.Ok(new ChatResponse(result.Data!, _config.Model));
    }

    private static IAiModelBackend CreateBackend(ResolvedAiConfig config, HttpClient http) =>
        config.Protocol switch
        {
            AiProtocol.Gemini => new GeminiBackend(http, config.ApiKey, config.Model),
            AiProtocol.XaiResponses => new XaiResponsesBackend(http, config.ApiKey, config.Model),
            _ => new OpenAiCompatibleBackend(http, config.ProviderName, config.BaseUrl, config.ApiKey, config.Model)
        };
}
