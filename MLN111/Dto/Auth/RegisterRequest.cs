using System.ComponentModel.DataAnnotations;

namespace MLN111.Dto.Auth;

public sealed class RegisterRequest
{
    [Required, EmailAddress, MaxLength(320)]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(2), MaxLength(100)]
    public string DisplayName { get; set; } = string.Empty;

    [Required, MinLength(6), MaxLength(100)]
    public string Password { get; set; } = string.Empty;

    [Required, Compare(nameof(Password), ErrorMessage = "Mat khau nhap lai khong khop.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
