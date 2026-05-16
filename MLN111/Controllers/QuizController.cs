using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MLN111.Dto.Quiz;
using MLN111.Entities;
using MLN111.Infrastructure;
using MLN111.Services.Quiz;

namespace MLN111.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class QuizController : ControllerBase
{
    private readonly IQuizRoomService _quiz;

    public QuizController(IQuizRoomService quiz)
    {
        _quiz = quiz;
    }

    [HttpPost("rooms")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> CreateRoom(CreateQuizRoomRequest request, CancellationToken cancellationToken)
    {
        var result = await _quiz.CreateRoomAsync(CurrentUser.GetUserId(User), request, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost("rooms/{roomId:guid}/questions")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> AddQuestion(Guid roomId, AddQuestionRequest request, CancellationToken cancellationToken)
    {
        var result = await _quiz.AddQuestionAsync(CurrentUser.GetUserId(User), roomId, request, cancellationToken);
        return ToActionResult(result);
    }

    [HttpGet("rooms/{roomId:guid}/questions")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> ListQuestions(Guid roomId, CancellationToken cancellationToken)
    {
        var result = await _quiz.ListQuestionsAsync(CurrentUser.GetUserId(User), roomId, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost("rooms/{roomId:guid}/lobby")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> OpenLobby(Guid roomId, CancellationToken cancellationToken)
    {
        var result = await _quiz.OpenLobbyAsync(CurrentUser.GetUserId(User), roomId, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost("rooms/{roomId:guid}/start")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> StartSession(Guid roomId, CancellationToken cancellationToken)
    {
        var result = await _quiz.StartSessionAsync(CurrentUser.GetUserId(User), roomId, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost("rooms/{roomId:guid}/next")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> NextQuestion(Guid roomId, CancellationToken cancellationToken)
    {
        var result = await _quiz.NextQuestionAsync(CurrentUser.GetUserId(User), roomId, cancellationToken);
        return ToActionResult(result);
    }

    [HttpGet("rooms/code/{joinCode}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByCode(string joinCode, CancellationToken cancellationToken)
    {
        var result = await _quiz.GetByJoinCodeAsync(joinCode, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost("rooms/join")]
    [Authorize]
    public async Task<IActionResult> Join(JoinRoomRequest request, CancellationToken cancellationToken)
    {
        var result = await _quiz.JoinAsync(CurrentUser.GetUserId(User), request, cancellationToken);
        return ToActionResult(result);
    }

    [HttpGet("rooms/{roomId:guid}/state")]
    [AllowAnonymous]
    public async Task<IActionResult> GetState(Guid roomId, CancellationToken cancellationToken)
    {
        Guid? userId = User.Identity?.IsAuthenticated == true
            ? CurrentUser.GetUserId(User)
            : null;
        var result = await _quiz.GetStateAsync(roomId, userId, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost("rooms/{roomId:guid}/answers")]
    [Authorize]
    public async Task<IActionResult> SubmitAnswer(Guid roomId, SubmitAnswerRequest request, CancellationToken cancellationToken)
    {
        var result = await _quiz.SubmitAnswerAsync(CurrentUser.GetUserId(User), roomId, request, cancellationToken);
        return ToActionResult(result);
    }

    [HttpGet("rooms/{roomId:guid}/leaderboard")]
    [AllowAnonymous]
    public async Task<IActionResult> Leaderboard(Guid roomId, CancellationToken cancellationToken)
    {
        var result = await _quiz.GetLeaderboardAsync(roomId, cancellationToken);
        return ToActionResult(result);
    }

    private IActionResult ToActionResult<T>(Services.Common.ServiceResult<T> result)
    {
        if (result.Success)
            return Ok(result.Data);
        return BadRequest(new { message = result.Error });
    }
}
