using System.ComponentModel.DataAnnotations;

namespace TaskTracker.Models;

public class RevokedToken
{
    [Key]
    public int Id { get; set; }

    /// <summary>JWT ID claim (jti) — uniquely identifies the token.</summary>
    [Required]
    public string Jti { get; set; } = string.Empty;

    public int UserId { get; set; }

    public DateTime RevokedAt { get; set; } = DateTime.UtcNow;

    /// <summary>When the original token expires — used to clean up old rows.</summary>
    public DateTime ExpiresAt { get; set; }
}
