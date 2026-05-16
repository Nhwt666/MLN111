using MLN111.Dto.Quiz;
using MLN111.Services.Common;

namespace MLN111.Services.Quiz;

public interface IQuizRoomService
{
    Task<ServiceResult<QuizRoomResponse>> CreateRoomAsync(Guid adminId, CreateQuizRoomRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult<QuestionDetailDto>> AddQuestionAsync(Guid adminId, Guid roomId, AddQuestionRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult<IReadOnlyList<QuestionDetailDto>>> ListQuestionsAsync(Guid adminId, Guid roomId, CancellationToken cancellationToken = default);
    Task<ServiceResult<QuizRoomResponse>> OpenLobbyAsync(Guid adminId, Guid roomId, CancellationToken cancellationToken = default);
    Task<ServiceResult<QuizRoomStateResponse>> StartSessionAsync(Guid adminId, Guid roomId, CancellationToken cancellationToken = default);
    Task<ServiceResult<QuizRoomStateResponse>> NextQuestionAsync(Guid adminId, Guid roomId, CancellationToken cancellationToken = default);
    Task<ServiceResult<JoinRoomResponse>> JoinAsync(Guid userId, JoinRoomRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult<QuizRoomStateResponse>> GetStateAsync(Guid roomId, Guid? userId, CancellationToken cancellationToken = default);
    Task<ServiceResult<SubmitAnswerResponse>> SubmitAnswerAsync(Guid userId, Guid roomId, SubmitAnswerRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult<LeaderboardResponse>> GetLeaderboardAsync(Guid roomId, CancellationToken cancellationToken = default);
    Task<ServiceResult<QuizRoomResponse>> GetByJoinCodeAsync(string joinCode, CancellationToken cancellationToken = default);
}
