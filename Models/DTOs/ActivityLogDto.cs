namespace TaskTracker.Models.DTOs;

public class ActivityLogDto
{
    public int Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Details { get; set; }

    /// <summary>Field-level changes recorded for update/toggle actions.</summary>
    public List<FieldChangeDto> Changes { get; set; } = new();

    public DateTime Timestamp { get; set; }
}

public class FieldChangeDto
{
    public string Field { get; set; } = string.Empty;
    public string? From { get; set; }
    public string? To { get; set; }
}
