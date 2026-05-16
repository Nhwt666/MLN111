using System.ComponentModel.DataAnnotations;

namespace MLN111.Dto.Quiz;

public sealed class JoinRoomRequest
{
    [Required, MinLength(4), MaxLength(12)]
    public string JoinCode { get; set; } = string.Empty;
}
