using TaskTracker.Data;
using TaskTracker.Models;

namespace TaskTracker.Services;

public interface IActivityLoggerService
{
    Task LogAsync(int userId, string action, string? details = null, CancellationToken ct = default);
}

public class ActivityLoggerService : IActivityLoggerService
{
    private readonly AppDbContext _context;

    public ActivityLoggerService(AppDbContext context)
    {
        _context = context;
    }

    public async Task LogAsync(int userId, string action, string? details = null, CancellationToken ct = default)
    {
        _context.ActivityLogs.Add(new ActivityLog
        {
            UserId = userId,
            Action = action,
            Details = details,
            Timestamp = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(ct);
    }
}
