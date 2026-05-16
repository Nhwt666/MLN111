using Microsoft.AspNetCore.Mvc;
using MLN111.Services.Health;

namespace MLN111.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class HealthController : ControllerBase
{
    private readonly IDatabaseHealthService _databaseHealth;

    public HealthController(IDatabaseHealthService databaseHealth)
    {
        _databaseHealth = databaseHealth;
    }

    [HttpGet("database")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Database(CancellationToken cancellationToken)
    {
        var result = await _databaseHealth.GetStatusAsync(cancellationToken);
        return Ok(result);
    }
}
