using MLN111.Dto.Health;

namespace MLN111.Services.Health;

public interface IDatabaseHealthService
{
    Task<DatabaseHealthResponse> GetStatusAsync(CancellationToken cancellationToken = default);
}
