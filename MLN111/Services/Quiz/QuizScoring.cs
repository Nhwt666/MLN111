namespace MLN111.Services.Quiz;

public static class QuizScoring
{
    public static int Calculate(bool isCorrect, int responseTimeMs, int secondsPerQuestion)
    {
        if (!isCorrect)
            return 0;

        var maxMs = secondsPerQuestion * 1000;
        if (maxMs <= 0)
            return 1000;

        var ratio = 1.0 - (double)responseTimeMs / maxMs;
        if (ratio < 0)
            ratio = 0;
        if (ratio > 1)
            ratio = 1;

        return (int)(500 + 500 * ratio);
    }
}
