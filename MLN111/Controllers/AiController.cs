using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MLN111.Dto.Ai;
using MLN111.Services.Ai;

namespace MLN111.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AiController : ControllerBase
{
    private readonly IAiChatService _ai;

    public AiController(IAiChatService ai)
    {
        _ai = ai;
    }

    [HttpGet("status")]
    [AllowAnonymous]
    public IActionResult Status() => Ok(_ai.GetStatus());

    [HttpPost("chat")]
    [Authorize]
    public async Task<IActionResult> Chat(ChatRequest request, CancellationToken cancellationToken)
    {
        var result = await _ai.AskAsync(request.Message, cancellationToken);
        if (!result.Success)
            return BadRequest(new { message = result.Error });
        return Ok(result.Data);
    }
}
