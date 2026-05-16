namespace MLN111.Dto.Quiz;

public sealed record QuestionDetailDto(
    Guid Id,
    string Content,
    int OrderIndex,
    IReadOnlyList<ChoiceDetailDto> Choices);

public sealed record ChoiceDetailDto(Guid Id, string Text, bool IsCorrect);
