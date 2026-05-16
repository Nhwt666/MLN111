using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using MLN111.Services.Common;

namespace MLN111.Services.Ai.Backends;

public sealed class OpenAiCompatibleBackend : IAiModelBackend
{
    private readonly HttpClient _http;
    private readonly string _providerName;
    private readonly string _baseUrl;
    private readonly string _apiKey;
    private readonly string _model;

    public OpenAiCompatibleBackend(
        HttpClient http,
        string providerName,
        string baseUrl,
        string apiKey,
        string model)
    {
        _http = http;
        _providerName = providerName;
        _baseUrl = baseUrl.EndsWith('/') ? baseUrl : baseUrl + '/';
        _apiKey = apiKey;
        _model = model;
    }

    public async Task<ServiceResult<string>> CompleteAsync(
        string systemPrompt,
        string userMessage,
        CancellationToken cancellationToken = default)
    {
        var body = new ChatRequestDto
        {
            Model = _model,
            Messages =
            [
                new ChatMessageDto { Role = "system", Content = systemPrompt },
                new ChatMessageDto { Role = "user", Content = userMessage }
            ]
        };

        var url = new Uri(new Uri(_baseUrl), "v1/chat/completions");
        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        request.Content = JsonContent.Create(body);

        HttpResponseMessage response;
        try
        {
            response = await _http.SendAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            return ServiceResult<string>.Fail($"{_providerName}: khong ket noi duoc ({ex.Message}).");
        }

        var raw = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
            return ServiceResult<string>.Fail(ParseError(raw, response.StatusCode));

        var parsed = JsonSerializer.Deserialize<ChatResponseDto>(raw);
        var text = parsed?.Choices?.FirstOrDefault()?.Message?.Content;
        if (string.IsNullOrWhiteSpace(text))
            return ServiceResult<string>.Fail($"{_providerName}: khong tra ve noi dung.");

        return ServiceResult<string>.Ok(text.Trim());
    }

    private string ParseError(string raw, System.Net.HttpStatusCode status)
    {
        try
        {
            var err = JsonSerializer.Deserialize<ErrorResponseDto>(raw);
            if (!string.IsNullOrWhiteSpace(err?.Error?.Message))
                return $"{_providerName} ({(int)status}): {err.Error.Message}";
        }
        catch
        {
            // ignored
        }

        if (!string.IsNullOrWhiteSpace(raw) && raw.Length < 400)
            return $"{_providerName} ({(int)status}): {raw}";

        return $"{_providerName} API loi ({(int)status}).";
    }

    private sealed class ChatRequestDto
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("messages")]
        public List<ChatMessageDto> Messages { get; set; } = [];
    }

    private sealed class ChatMessageDto
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
    }

    private sealed class ChatResponseDto
    {
        [JsonPropertyName("choices")]
        public List<ChoiceDto>? Choices { get; set; }
    }

    private sealed class ChoiceDto
    {
        [JsonPropertyName("message")]
        public ChatMessageDto? Message { get; set; }
    }

    private sealed class ErrorResponseDto
    {
        [JsonPropertyName("error")]
        public ErrorBodyDto? Error { get; set; }
    }

    private sealed class ErrorBodyDto
    {
        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }
}
