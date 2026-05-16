using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using MLN111.Services.Common;

namespace MLN111.Services.Ai.Backends;

public sealed class XaiResponsesBackend : IAiModelBackend
{
    private readonly HttpClient _http;
    private readonly string _apiKey;
    private readonly string _model;

    public XaiResponsesBackend(HttpClient http, string apiKey, string model)
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
        var body = new XaiResponsesRequest
        {
            Model = _model,
            Store = false,
            Input =
            [
                new XaiInputMessage { Role = "system", Content = systemPrompt },
                new XaiInputMessage { Role = "user", Content = userMessage }
            ]
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.x.ai/v1/responses");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        request.Content = JsonContent.Create(body);

        HttpResponseMessage response;
        try
        {
            response = await _http.SendAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            return ServiceResult<string>.Fail($"Grok: khong ket noi duoc ({ex.Message}).");
        }

        var raw = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
            return ServiceResult<string>.Fail(ParseError(raw, response.StatusCode));

        var text = ExtractOutputText(raw);
        if (string.IsNullOrWhiteSpace(text))
            return ServiceResult<string>.Fail("Grok: khong doc duoc noi dung tra loi.");

        return ServiceResult<string>.Ok(text.Trim());
    }

    private static string? ExtractOutputText(string raw)
    {
        using var doc = JsonDocument.Parse(raw);
        var root = doc.RootElement;

        if (root.TryGetProperty("output", out var output) && output.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in output.EnumerateArray())
            {
                var fromContent = ReadTextFromContentArray(item);
                if (!string.IsNullOrWhiteSpace(fromContent))
                    return fromContent;
            }
        }

        if (root.TryGetProperty("output_text", out var outputText) && outputText.ValueKind == JsonValueKind.String)
            return outputText.GetString();

        return null;
    }

    private static string? ReadTextFromContentArray(JsonElement item)
    {
        if (!item.TryGetProperty("content", out var content))
            return null;

        if (content.ValueKind == JsonValueKind.String)
            return content.GetString();

        if (content.ValueKind != JsonValueKind.Array)
            return null;

        foreach (var part in content.EnumerateArray())
        {
            if (part.TryGetProperty("type", out var type)
                && type.GetString() == "output_text"
                && part.TryGetProperty("text", out var text))
                return text.GetString();

            if (part.TryGetProperty("text", out var plainText))
                return plainText.GetString();
        }

        return null;
    }

    private static string ParseError(string raw, System.Net.HttpStatusCode status)
    {
        try
        {
            using var doc = JsonDocument.Parse(raw);
            if (doc.RootElement.TryGetProperty("error", out var error))
            {
                if (error.TryGetProperty("message", out var msg))
                    return $"Grok ({(int)status}): {msg.GetString()}";
                if (error.ValueKind == JsonValueKind.String)
                    return $"Grok ({(int)status}): {error.GetString()}";
            }
        }
        catch
        {
            // ignored
        }

        if (!string.IsNullOrWhiteSpace(raw) && raw.Length < 500)
            return $"Grok ({(int)status}): {raw}";

        return $"Grok API loi ({(int)status}). Kiem tra API key, model va credit tai console.x.ai.";
    }

    private sealed class XaiResponsesRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("input")]
        public List<XaiInputMessage> Input { get; set; } = [];

        [JsonPropertyName("store")]
        public bool Store { get; set; }
    }

    private sealed class XaiInputMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
    }
}
