using System.ComponentModel.DataAnnotations;

namespace TaskTracker.Models;

public class ActivityLog
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    /// <summary>e.g. "TaskCreated", "TaskDeleted", "Login", "Logout"</summary>
    [Required]
    public string Action { get; set; } = string.Empty;

    /// <summary>Human-readable description of what changed.</summary>
    public string? Details { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
