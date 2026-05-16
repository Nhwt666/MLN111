using Microsoft.EntityFrameworkCore;
using MLN111.Data;
using MLN111.Dto.Quiz;
using MLN111.Entities;
using MLN111.Services.Common;

namespace MLN111.Services.Quiz;

public sealed class QuizRoomService : IQuizRoomService
{
    private readonly AppDbContext _db;

    public QuizRoomService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ServiceResult<QuizRoomResponse>> CreateRoomAsync(
        Guid adminId,
        CreateQuizRoomRequest request,
        CancellationToken cancellationToken = default)
    {
        var room = new QuizRoom
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            JoinCode = await GenerateUniqueJoinCodeAsync(cancellationToken),
            SecondsPerQuestion = request.SecondsPerQuestion,
            Status = QuizRoomStatus.Draft,
            CreatedById = adminId,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _db.QuizRooms.Add(room);
        await _db.SaveChangesAsync(cancellationToken);
        return ServiceResult<QuizRoomResponse>.Ok(MapRoom(room, 0));
    }

    public async Task<ServiceResult<QuestionDetailDto>> AddQuestionAsync(
        Guid adminId,
        Guid roomId,
        AddQuestionRequest request,
        CancellationToken cancellationToken = default)
    {
        var room = await LoadOwnedRoomAsync(adminId, roomId, cancellationToken);
        if (room is null)
            return ServiceResult<QuestionDetailDto>.Fail("Khong tim thay phong hoac ban khong phai chu phong.");

        if (room.Status != QuizRoomStatus.Draft)
            return ServiceResult<QuestionDetailDto>.Fail("Chi them cau hoi khi phong o trang thai Draft.");

        var count = await _db.QuizQuestions.CountAsync(q => q.QuizRoomId == roomId, cancellationToken);
        if (count >= QuizSessionDefaults.QuestionsPerSession)
            return ServiceResult<QuestionDetailDto>.Fail($"Toi da {QuizSessionDefaults.QuestionsPerSession} cau moi phong.");

        if (request.Choices.Count < 2)
            return ServiceResult<QuestionDetailDto>.Fail("Moi cau can it nhat 2 dap an.");

        if (request.Choices.Count(c => c.IsCorrect) != 1)
            return ServiceResult<QuestionDetailDto>.Fail("Moi cau phai co dung 1 dap an dung.");

        var question = new QuizQuestion
        {
            Id = Guid.NewGuid(),
            QuizRoomId = roomId,
            Content = request.Content.Trim(),
            OrderIndex = count
        };

        foreach (var c in request.Choices)
        {
            question.Choices.Add(new QuizChoice
            {
                Id = Guid.NewGuid(),
                QuestionId = question.Id,
                Text = c.Text.Trim(),
                IsCorrect = c.IsCorrect
            });
        }

        _db.QuizQuestions.Add(question);
        room.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return ServiceResult<QuestionDetailDto>.Ok(MapQuestion(question));
    }

    public async Task<ServiceResult<IReadOnlyList<QuestionDetailDto>>> ListQuestionsAsync(
        Guid adminId,
        Guid roomId,
        CancellationToken cancellationToken = default)
    {
        var room = await LoadOwnedRoomAsync(adminId, roomId, cancellationToken);
        if (room is null)
            return ServiceResult<IReadOnlyList<QuestionDetailDto>>.Fail("Khong tim thay phong hoac ban khong phai chu phong.");

        var questions = await _db.QuizQuestions
            .AsNoTracking()
            .Include(q => q.Choices)
            .Where(q => q.QuizRoomId == roomId)
            .OrderBy(q => q.OrderIndex)
            .ToListAsync(cancellationToken);

        var list = questions.Select(MapQuestion).ToList();
        return ServiceResult<IReadOnlyList<QuestionDetailDto>>.Ok(list);
    }

    public async Task<ServiceResult<QuizRoomResponse>> OpenLobbyAsync(
        Guid adminId,
        Guid roomId,
        CancellationToken cancellationToken = default)
    {
        var room = await LoadOwnedRoomAsync(adminId, roomId, cancellationToken);
        if (room is null)
            return ServiceResult<QuizRoomResponse>.Fail("Khong tim thay phong hoac ban khong phai chu phong.");

        if (room.Status != QuizRoomStatus.Draft)
            return ServiceResult<QuizRoomResponse>.Fail("Chi mo lobby tu trang thai Draft.");

        var count = await _db.QuizQuestions.CountAsync(q => q.QuizRoomId == roomId, cancellationToken);
        if (count == 0)
            return ServiceResult<QuizRoomResponse>.Fail("Can it nhat 1 cau hoi truoc khi mo phong.");

        room.Status = QuizRoomStatus.Lobby;
        room.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return ServiceResult<QuizRoomResponse>.Ok(MapRoom(room, count));
    }

    public async Task<ServiceResult<QuizRoomStateResponse>> StartSessionAsync(
        Guid adminId,
        Guid roomId,
        CancellationToken cancellationToken = default)
    {
        var room = await LoadOwnedRoomAsync(adminId, roomId, cancellationToken);
        if (room is null)
            return ServiceResult<QuizRoomStateResponse>.Fail("Khong tim thay phong hoac ban khong phai chu phong.");

        if (room.Status != QuizRoomStatus.Lobby)
            return ServiceResult<QuizRoomStateResponse>.Fail("Chi bat dau session khi phong dang Lobby.");

        var questions = await LoadOrderedQuestionsAsync(roomId, cancellationToken);
        if (questions.Count == 0)
            return ServiceResult<QuizRoomStateResponse>.Fail("Phong chua co cau hoi.");

        var now = DateTimeOffset.UtcNow;
        room.Status = QuizRoomStatus.InProgress;
        room.CurrentQuestionIndex = 0;
        room.QuestionStartedAt = now;
        room.SessionStartedAt = now;
        room.SessionFinishedAt = null;
        room.UpdatedAt = now;
        await _db.SaveChangesAsync(cancellationToken);

        return await BuildStateAsync(roomId, adminId, cancellationToken);
    }

    public async Task<ServiceResult<QuizRoomStateResponse>> NextQuestionAsync(
        Guid adminId,
        Guid roomId,
        CancellationToken cancellationToken = default)
    {
        var room = await LoadOwnedRoomAsync(adminId, roomId, cancellationToken);
        if (room is null)
            return ServiceResult<QuizRoomStateResponse>.Fail("Khong tim thay phong hoac ban khong phai chu phong.");

        if (room.Status != QuizRoomStatus.InProgress)
            return ServiceResult<QuizRoomStateResponse>.Fail("Session chua bat dau.");

        var questions = await LoadOrderedQuestionsAsync(roomId, cancellationToken);
        var index = room.CurrentQuestionIndex ?? 0;

        if (index >= questions.Count - 1)
        {
            room.Status = QuizRoomStatus.Finished;
            room.SessionFinishedAt = DateTimeOffset.UtcNow;
            room.QuestionStartedAt = null;
            room.UpdatedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
            return await BuildStateAsync(roomId, adminId, cancellationToken);
        }

        room.CurrentQuestionIndex = index + 1;
        room.QuestionStartedAt = DateTimeOffset.UtcNow;
        room.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return await BuildStateAsync(roomId, adminId, cancellationToken);
    }

    public async Task<ServiceResult<JoinRoomResponse>> JoinAsync(
        Guid userId,
        JoinRoomRequest request,
        CancellationToken cancellationToken = default)
    {
        var code = request.JoinCode.Trim().ToUpperInvariant();
        var room = await _db.QuizRooms.FirstOrDefaultAsync(r => r.JoinCode == code, cancellationToken);
        if (room is null)
            return ServiceResult<JoinRoomResponse>.Fail("Ma phong khong hop le.");

        if (room.Status is QuizRoomStatus.Draft or QuizRoomStatus.Finished)
            return ServiceResult<JoinRoomResponse>.Fail("Phong chua mo hoac da ket thuc.");

        var user = await _db.Users.AsNoTracking().FirstAsync(u => u.Id == userId, cancellationToken);
        var existing = await _db.QuizParticipants
            .FirstOrDefaultAsync(p => p.QuizRoomId == room.Id && p.UserId == userId, cancellationToken);

        if (existing is not null)
            return ServiceResult<JoinRoomResponse>.Ok(
                new JoinRoomResponse(room.Id, existing.Id, room.Title, room.JoinCode));

        var participant = new QuizParticipant
        {
            Id = Guid.NewGuid(),
            QuizRoomId = room.Id,
            UserId = userId,
            DisplayNameSnapshot = user.DisplayName,
            JoinedAt = DateTimeOffset.UtcNow
        };

        _db.QuizParticipants.Add(participant);
        await _db.SaveChangesAsync(cancellationToken);
        return ServiceResult<JoinRoomResponse>.Ok(
            new JoinRoomResponse(room.Id, participant.Id, room.Title, room.JoinCode));
    }

    public async Task<ServiceResult<QuizRoomStateResponse>> GetStateAsync(
        Guid roomId,
        Guid? userId,
        CancellationToken cancellationToken = default)
    {
        var exists = await _db.QuizRooms.AnyAsync(r => r.Id == roomId, cancellationToken);
        if (!exists)
            return ServiceResult<QuizRoomStateResponse>.Fail("Khong tim thay phong.");

        return await BuildStateAsync(roomId, userId, cancellationToken);
    }

    public async Task<ServiceResult<SubmitAnswerResponse>> SubmitAnswerAsync(
        Guid userId,
        Guid roomId,
        SubmitAnswerRequest request,
        CancellationToken cancellationToken = default)
    {
        var room = await _db.QuizRooms.FirstOrDefaultAsync(r => r.Id == roomId, cancellationToken);
        if (room is null)
            return ServiceResult<SubmitAnswerResponse>.Fail("Khong tim thay phong.");

        if (room.Status != QuizRoomStatus.InProgress)
            return ServiceResult<SubmitAnswerResponse>.Fail("Session chua dang chay.");

        if (room.CurrentQuestionIndex is null || room.QuestionStartedAt is null)
            return ServiceResult<SubmitAnswerResponse>.Fail("Chua co cau hoi dang mo.");

        var deadline = room.QuestionStartedAt.Value.AddSeconds(room.SecondsPerQuestion);
        var now = DateTimeOffset.UtcNow;
        if (now > deadline)
            return ServiceResult<SubmitAnswerResponse>.Fail("Het thoi gian tra loi cau nay.");

        var participant = await _db.QuizParticipants
            .FirstOrDefaultAsync(p => p.QuizRoomId == roomId && p.UserId == userId, cancellationToken);
        if (participant is null)
            return ServiceResult<SubmitAnswerResponse>.Fail("Ban chua join phong nay.");

        var questions = await LoadOrderedQuestionsAsync(roomId, cancellationToken);
        var current = questions[room.CurrentQuestionIndex.Value];

        var already = await _db.QuizAnswers.AnyAsync(
            a => a.ParticipantId == participant.Id && a.QuestionId == current.Id,
            cancellationToken);
        if (already)
            return ServiceResult<SubmitAnswerResponse>.Fail("Ban da tra loi cau nay.");

        var choice = await _db.QuizChoices
            .FirstOrDefaultAsync(c => c.Id == request.ChoiceId && c.QuestionId == current.Id, cancellationToken);
        if (choice is null)
            return ServiceResult<SubmitAnswerResponse>.Fail("Dap an khong hop le.");

        var responseMs = (int)Math.Max(0, (now - room.QuestionStartedAt.Value).TotalMilliseconds);
        var points = QuizScoring.Calculate(choice.IsCorrect, responseMs, room.SecondsPerQuestion);

        var answer = new QuizAnswer
        {
            Id = Guid.NewGuid(),
            ParticipantId = participant.Id,
            QuestionId = current.Id,
            ChoiceId = choice.Id,
            AnsweredAt = now,
            ResponseTimeMs = responseMs,
            IsCorrect = choice.IsCorrect,
            PointsEarned = points
        };

        participant.TotalScore += points;
        _db.QuizAnswers.Add(answer);
        await _db.SaveChangesAsync(cancellationToken);

        return ServiceResult<SubmitAnswerResponse>.Ok(
            new SubmitAnswerResponse(choice.IsCorrect, points, participant.TotalScore));
    }

    public async Task<ServiceResult<LeaderboardResponse>> GetLeaderboardAsync(
        Guid roomId,
        CancellationToken cancellationToken = default)
    {
        var room = await _db.QuizRooms.AsNoTracking().FirstOrDefaultAsync(r => r.Id == roomId, cancellationToken);
        if (room is null)
            return ServiceResult<LeaderboardResponse>.Fail("Khong tim thay phong.");

        var ranked = await _db.QuizParticipants
            .AsNoTracking()
            .Where(p => p.QuizRoomId == roomId)
            .OrderByDescending(p => p.TotalScore)
            .ThenBy(p => p.JoinedAt)
            .ToListAsync(cancellationToken);

        var entries = ranked
            .Select((p, i) => new LeaderboardEntryDto(i + 1, p.DisplayNameSnapshot, p.TotalScore))
            .ToList();

        return ServiceResult<LeaderboardResponse>.Ok(
            new LeaderboardResponse(roomId, room.Title, entries));
    }

    public async Task<ServiceResult<QuizRoomResponse>> GetByJoinCodeAsync(
        string joinCode,
        CancellationToken cancellationToken = default)
    {
        var code = joinCode.Trim().ToUpperInvariant();
        var room = await _db.QuizRooms.AsNoTracking().FirstOrDefaultAsync(r => r.JoinCode == code, cancellationToken);
        if (room is null)
            return ServiceResult<QuizRoomResponse>.Fail("Ma phong khong hop le.");

        var count = await _db.QuizQuestions.CountAsync(q => q.QuizRoomId == room.Id, cancellationToken);
        return ServiceResult<QuizRoomResponse>.Ok(MapRoom(room, count));
    }

    private async Task<ServiceResult<QuizRoomStateResponse>> BuildStateAsync(
        Guid roomId,
        Guid? userId,
        CancellationToken cancellationToken)
    {
        var room = await _db.QuizRooms.AsNoTracking().FirstAsync(r => r.Id == roomId, cancellationToken);
        var questions = await LoadOrderedQuestionsAsync(roomId, cancellationToken);
        var participantCount = await _db.QuizParticipants.CountAsync(p => p.QuizRoomId == roomId, cancellationToken);

        QuestionStateDto? currentQuestion = null;
        Guid? myParticipantId = null;
        int? myTotalScore = null;
        var hasAnswered = false;

        if (room.Status == QuizRoomStatus.InProgress
            && room.CurrentQuestionIndex is not null
            && room.CurrentQuestionIndex.Value < questions.Count)
        {
            var q = questions[room.CurrentQuestionIndex.Value];
            currentQuestion = new QuestionStateDto(
                q.Id,
                q.Content,
                q.OrderIndex,
                q.Choices.Select(c => new ChoiceStateDto(c.Id, c.Text)).ToList());
        }

        if (userId is not null)
        {
            var me = await _db.QuizParticipants.AsNoTracking()
                .FirstOrDefaultAsync(p => p.QuizRoomId == roomId && p.UserId == userId, cancellationToken);
            if (me is not null)
            {
                myParticipantId = me.Id;
                myTotalScore = me.TotalScore;
                if (currentQuestion is not null)
                {
                    hasAnswered = await _db.QuizAnswers.AnyAsync(
                        a => a.ParticipantId == me.Id && a.QuestionId == currentQuestion.QuestionId,
                        cancellationToken);
                }
            }
        }

        int? secondsRemaining = null;
        if (room.Status == QuizRoomStatus.InProgress && room.QuestionStartedAt is not null)
        {
            var end = room.QuestionStartedAt.Value.AddSeconds(room.SecondsPerQuestion);
            secondsRemaining = Math.Max(0, (int)(end - DateTimeOffset.UtcNow).TotalSeconds);
        }

        return ServiceResult<QuizRoomStateResponse>.Ok(new QuizRoomStateResponse(
            room.Id,
            room.Title,
            room.Status,
            room.SecondsPerQuestion,
            room.CurrentQuestionIndex,
            room.QuestionStartedAt,
            secondsRemaining,
            participantCount,
            questions.Count,
            currentQuestion,
            myParticipantId,
            myTotalScore,
            hasAnswered));
    }

    private async Task<List<QuizQuestion>> LoadOrderedQuestionsAsync(Guid roomId, CancellationToken cancellationToken)
    {
        return await _db.QuizQuestions
            .Include(q => q.Choices)
            .Where(q => q.QuizRoomId == roomId)
            .OrderBy(q => q.OrderIndex)
            .ToListAsync(cancellationToken);
    }

    private async Task<QuizRoom?> LoadOwnedRoomAsync(Guid adminId, Guid roomId, CancellationToken cancellationToken)
    {
        return await _db.QuizRooms.FirstOrDefaultAsync(
            r => r.Id == roomId && r.CreatedById == adminId,
            cancellationToken);
    }

    private async Task<string> GenerateUniqueJoinCodeAsync(CancellationToken cancellationToken)
    {
        for (var i = 0; i < 20; i++)
        {
            var code = JoinCodeGenerator.Create();
            if (!await _db.QuizRooms.AnyAsync(r => r.JoinCode == code, cancellationToken))
                return code;
        }

        throw new InvalidOperationException("Khong tao duoc ma phong.");
    }

    private static QuizRoomResponse MapRoom(QuizRoom room, int questionCount) =>
        new(room.Id, room.Title, room.Description, room.JoinCode, room.Status, room.SecondsPerQuestion, questionCount, room.CreatedAt);

    private static QuestionDetailDto MapQuestion(QuizQuestion q) =>
        new(q.Id, q.Content, q.OrderIndex, q.Choices.Select(c => new ChoiceDetailDto(c.Id, c.Text, c.IsCorrect)).ToList());
}
