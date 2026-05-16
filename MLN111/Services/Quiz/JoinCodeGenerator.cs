namespace MLN111.Services.Quiz;

public static class JoinCodeGenerator
{
    private const string Chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    public static string Create(int length = 6)
    {
        Span<char> buffer = stackalloc char[length];
        for (var i = 0; i < length; i++)
            buffer[i] = Chars[Random.Shared.Next(Chars.Length)];
        return new string(buffer);
    }
}
