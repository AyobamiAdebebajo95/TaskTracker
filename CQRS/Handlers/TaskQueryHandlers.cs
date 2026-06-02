using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TaskTracker.CQRS.Queries;
using TaskTracker.Data;
using TaskTracker.Models.DTOs;

namespace TaskTracker.CQRS.Handlers;

public class GetTasksQueryHandler : IRequestHandler<GetTasksQuery, List<TaskResponseDto>>
{
    private readonly AppDbContext _context;

    public GetTasksQueryHandler(AppDbContext context) => _context = context;

    public async Task<List<TaskResponseDto>> Handle(GetTasksQuery request, CancellationToken cancellationToken)
    {
        return await _context.Tasks
            .Where(t => t.UserId == request.UserId)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TaskResponseDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                DueDate = t.DueDate,
                IsComplete = t.IsComplete,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }
}

public class GetTaskByIdQueryHandler : IRequestHandler<GetTaskByIdQuery, TaskResponseDto?>
{
    private readonly AppDbContext _context;

    public GetTaskByIdQueryHandler(AppDbContext context) => _context = context;

    public async Task<TaskResponseDto?> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.Tasks
            .Where(t => t.Id == request.TaskId && t.UserId == request.UserId)
            .Select(t => new TaskResponseDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                DueDate = t.DueDate,
                IsComplete = t.IsComplete,
                CreatedAt = t.CreatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}

public class GetActivityLogQueryHandler : IRequestHandler<GetActivityLogQuery, List<ActivityLogDto>>
{
    private readonly AppDbContext _context;

    public GetActivityLogQueryHandler(AppDbContext context) => _context = context;

    public async Task<List<ActivityLogDto>> Handle(GetActivityLogQuery request, CancellationToken cancellationToken)
    {
        var logs = await _context.ActivityLogs
            .Where(a => a.UserId == request.UserId)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync(cancellationToken);

        return logs.Select(a =>
        {
            var dto = new ActivityLogDto
            {
                Id = a.Id,
                Action = a.Action,
                Timestamp = a.Timestamp
            };

            // TaskUpdated and TaskToggled store a JSON payload — deserialize for rich output
            if (a.Action is "TaskUpdated" or "TaskToggled" && a.Details != null)
            {
                try
                {
                    var payload = JsonSerializer.Deserialize<ActivityDetailsPayload>(
                        a.Details,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    dto.Details = payload?.Summary;
                    dto.Changes = payload?.Changes ?? new();
                }
                catch
                {
                    // Fallback: treat as plain text (e.g. legacy entries)
                    dto.Details = a.Details;
                }
            }
            else
            {
                dto.Details = a.Details;
            }

            return dto;
        }).ToList();
    }

    // Matches the shape serialized by the command handlers
    private sealed class ActivityDetailsPayload
    {
        public string? Summary { get; set; }
        public List<FieldChangeDto> Changes { get; set; } = new();
    }
}
