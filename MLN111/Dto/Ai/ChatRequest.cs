using System.ComponentModel.DataAnnotations;

namespace MLN111.Dto.Ai;

public sealed class ChatRequest
{
    [Required, MinLength(1), MaxLength(4000)]
    public string Message { get; set; } = string.Empty;
}
