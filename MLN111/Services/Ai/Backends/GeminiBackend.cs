using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using MLN111.Services.Common;

namespace MLN111.Services.Ai.Backends;

public sealed class GeminiBackend : IAiModelBackend
{
    private readonly HttpClient _http;
    private readonly string _apiKey;
    private readonly string _model;

    public GeminiBackend(HttpClient http, string apiKey, string model)
    {
        _http = http;
        _apiKey = apiKey;
        _model = model;
    }

    public async Task<ServiceResult<string>> CompleteAsync(
        string systemPrompt,
        string userMessage,
        CancellationToken cancellationToken = default)
    {
        var url =
            $"https://generativelanguage.googleapis.com/v1beta/models/{Uri.EscapeDataString(_model)}:generateContent?key={Uri.EscapeDataString(_apiKey)}";

        var body = new GeminiRequest
        {
            SystemInstruction = new GeminiContent { Parts = [new GeminiPart { Text = systemPrompt }] },
            Contents =
            [
                new GeminiContent
                {
                    Role = "user",
                    Parts = [new GeminiPart { Text = userMessage }]
                }
            ]
        };

        HttpResponseMessage response;
        try
        {
            response = await _http.PostAsJsonAsync(url, body, cancellationToken);
        }
        catch (Exception ex)
        {
            return ServiceResult<string>.Fail($"Gemini: khong ket noi duoc ({ex.Message}).");
        }

        var raw = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            try
            {
                var err = JsonSerializer.Deserialize<GeminiErrorResponse>(raw);
                if (!string.IsNullOrWhiteSpace(err?.Error?.Message))
                    return ServiceResult<string>.Fail($"Gemini ({(int)response.StatusCode}): {err.Error.Message}");
            }
            catch
            {
                // ignored
            }

            return ServiceResult<string>.Fail($"Gemini API loi ({(int)response.StatusCode}).");
        }

        var parsed = JsonSerializer.Deserialize<GeminiResponse>(raw);
        var text = parsed?.Candidates?
            .FirstOrDefault()?
            .Content?
            .Parts?
            .FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.Text))?
            .Text;

        if (string.IsNullOrWhiteSpace(text))
            return ServiceResult<string>.Fail("Gemini: khong tra ve noi dung.");

        return ServiceResult<string>.Ok(text.Trim());
    }

    private sealed class GeminiRequest
    {
        [JsonPropertyName("systemInstruction")]
        public GeminiContent? SystemInstruction { get; set; }

        [JsonPropertyName("contents")]
        public List<GeminiContent> Contents { get; set; } = [];
    }

    private sealed class GeminiContent
    {
        [JsonPropertyName("role")]
        public string? Role { get; set; }

        [JsonPropertyName("parts")]
        public List<GeminiPart> Parts { get; set; } = [];
    }

    private sealed class GeminiPart
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }

    private sealed class GeminiResponse
    {
        [JsonPropertyName("candidates")]
        public List<GeminiCandidate>? Candidates { get; set; }
    }

    private sealed class GeminiCandidate
    {
        [JsonPropertyName("content")]
        public GeminiContent? Content { get; set; }
    }

    private sealed class GeminiErrorResponse
    {
        [JsonPropertyName("error")]
        public GeminiError? Error { get; set; }
    }

    private sealed class GeminiError
    {
        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }
}
