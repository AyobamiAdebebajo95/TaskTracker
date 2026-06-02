using System.ComponentModel.DataAnnotations;
using TaskTracker.Validation;

namespace TaskTracker.Models.DTOs;

public class RegisterDto
{
    [Required]
    [EmailAddress]
    [NotPlaceholder]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    [NotPlaceholder]
    public string Password { get; set; } = string.Empty;
}

public class LoginDto
{
    [Required]
    [EmailAddress]
    [NotPlaceholder]
    public string Email { get; set; } = string.Empty;

    [Required]
    [NotPlaceholder]
    public string Password { get; set; } = string.Empty;
}

public class AuthResponseDto
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}
