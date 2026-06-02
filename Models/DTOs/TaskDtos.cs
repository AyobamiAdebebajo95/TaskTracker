using System.ComponentModel.DataAnnotations;
using TaskTracker.Validation;

namespace TaskTracker.Models.DTOs;

public class CreateTaskDto
{
    [Required]
    [NotPlaceholder]
    public string Title { get; set; } = string.Empty;

    [NotPlaceholder]
    public string? Description { get; set; }

    public DateTime? DueDate { get; set; }
}

public class UpdateTaskDto
{
    [Required]
    [NotPlaceholder]
    public string Title { get; set; } = string.Empty;

    [NotPlaceholder]
    public string? Description { get; set; }

    public DateTime? DueDate { get; set; }

    public bool IsComplete { get; set; }
}

public class TaskResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public bool IsComplete { get; set; }
    public DateTime CreatedAt { get; set; }
}
