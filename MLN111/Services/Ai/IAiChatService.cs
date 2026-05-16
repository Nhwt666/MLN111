using MLN111.Dto.Ai;
using MLN111.Services.Common;

namespace MLN111.Services.Ai;

public interface IAiChatService
{
    Task<ServiceResult<ChatResponse>> AskAsync(string message, CancellationToken cancellationToken = default);
    AiStatusResponse GetStatus();
}
