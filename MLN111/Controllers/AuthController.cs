using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MLN111.Dto.Auth;
using MLN111.Infrastructure;
using MLN111.Services.Auth;

namespace MLN111.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth)
    {
        _auth = auth;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await _auth.RegisterAsync(request, cancellationToken);
        return result.Success ? Ok(result.Data) : BadRequest(new { message = result.Error });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _auth.LoginAsync(request, cancellationToken);
        return result.Success ? Ok(result.Data) : Unauthorized(new { message = result.Error });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var userId = CurrentUser.GetUserId(User);
        var result = await _auth.GetProfileAsync(userId, cancellationToken);
        return result.Success ? Ok(result.Data) : NotFound(new { message = result.Error });
    }
}
