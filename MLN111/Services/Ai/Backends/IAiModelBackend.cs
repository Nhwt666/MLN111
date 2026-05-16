using MLN111.Services.Common;

namespace MLN111.Services.Ai.Backends;

public interface IAiModelBackend
{
    Task<ServiceResult<string>> CompleteAsync(string systemPrompt, string userMessage, CancellationToken cancellationToken = default);
}
