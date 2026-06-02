using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TaskTracker.Models;

public class TaskItem
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime? DueDate { get; set; }

    public bool IsComplete { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int UserId { get; set; }

    [JsonIgnore]
    public User? User { get; set; }
}
