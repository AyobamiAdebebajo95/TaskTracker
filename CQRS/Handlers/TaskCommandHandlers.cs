using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TaskTracker.CQRS.Commands;
using TaskTracker.Data;
using TaskTracker.Models;
using TaskTracker.Models.DTOs;
using TaskTracker.Services;

namespace TaskTracker.CQRS.Handlers;

public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, TaskResponseDto>
{
    private readonly AppDbContext _context;
    private readonly IActivityLoggerService _logger;

    public CreateTaskCommandHandler(AppDbContext context, IActivityLoggerService logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<TaskResponseDto> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        var task = new TaskItem
        {
            Title = request.TaskDto.Title,
            Description = request.TaskDto.Description,
            DueDate = request.TaskDto.DueDate,
            IsComplete = false,
            UserId = request.UserId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync(cancellationToken);

        await _logger.LogAsync(request.UserId, "TaskCreated",
            $"Created task '{task.Title}' (ID: {task.Id})", cancellationToken);

        return MapToDto(task);
    }

    private static TaskResponseDto MapToDto(TaskItem task) => new()
    {
        Id = task.Id,
        Title = task.Title,
        Description = task.Description,
        DueDate = task.DueDate,
        IsComplete = task.IsComplete,
        CreatedAt = task.CreatedAt
    };
}

public class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand, TaskResponseDto?>
{
    private readonly AppDbContext _context;
    private readonly IActivityLoggerService _logger;

    public UpdateTaskCommandHandler(AppDbContext context, IActivityLoggerService logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<TaskResponseDto?> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _context.Tasks
            .FirstOrDefaultAsync(t => t.Id == request.TaskId && t.UserId == request.UserId, cancellationToken);

        if (task == null) return null;

        // Capture field-level diff before applying changes
        var changes = BuildChanges(task, request.UpdateDto);

        task.Title = request.UpdateDto.Title;
        task.Description = request.UpdateDto.Description;
        task.DueDate = request.UpdateDto.DueDate;
        task.IsComplete = request.UpdateDto.IsComplete;

        await _context.SaveChangesAsync(cancellationToken);

        var summary = changes.Count > 0
            ? $"Updated task '{task.Title}' (ID: {task.Id}): " +
              string.Join(", ", changes.Select(c => $"{c.Field} '{c.From}' → '{c.To}'"))
            : $"Updated task '{task.Title}' (ID: {task.Id}) — no fields changed";

        var detailsPayload = JsonSerializer.Serialize(new { summary, changes });
        await _logger.LogAsync(request.UserId, "TaskUpdated", detailsPayload, cancellationToken);

        return new TaskResponseDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            DueDate = task.DueDate,
            IsComplete = task.IsComplete,
            CreatedAt = task.CreatedAt
        };
    }

    private static List<FieldChangeDto> BuildChanges(TaskItem before, UpdateTaskDto after)
    {
        var changes = new List<FieldChangeDto>();

        if (before.Title != after.Title)
            changes.Add(new FieldChangeDto { Field = "Title", From = before.Title, To = after.Title });

        if (before.Description != after.Description)
            changes.Add(new FieldChangeDto
            {
                Field = "Description",
                From = before.Description ?? "(none)",
                To = after.Description ?? "(none)"
            });

        if (before.DueDate != after.DueDate)
            changes.Add(new FieldChangeDto
            {
                Field = "DueDate",
                From = before.DueDate?.ToString("yyyy-MM-dd") ?? "(none)",
                To = after.DueDate?.ToString("yyyy-MM-dd") ?? "(none)"
            });

        if (before.IsComplete != after.IsComplete)
            changes.Add(new FieldChangeDto
            {
                Field = "Status",
                From = before.IsComplete ? "Complete" : "Incomplete",
                To = after.IsComplete ? "Complete" : "Incomplete"
            });

        return changes;
    }
}

public class DeleteTaskCommandHandler : IRequestHandler<DeleteTaskCommand, bool>
{
    private readonly AppDbContext _context;
    private readonly IActivityLoggerService _logger;

    public DeleteTaskCommandHandler(AppDbContext context, IActivityLoggerService logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _context.Tasks
            .FirstOrDefaultAsync(t => t.Id == request.TaskId && t.UserId == request.UserId, cancellationToken);

        if (task == null) return false;

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync(cancellationToken);

        await _logger.LogAsync(request.UserId, "TaskDeleted",
            $"Deleted task '{task.Title}' (ID: {task.Id})", cancellationToken);

        return true;
    }
}

public class ToggleTaskCompleteCommandHandler : IRequestHandler<ToggleTaskCompleteCommand, TaskResponseDto?>
{
    private readonly AppDbContext _context;
    private readonly IActivityLoggerService _logger;

    public ToggleTaskCompleteCommandHandler(AppDbContext context, IActivityLoggerService logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<TaskResponseDto?> Handle(ToggleTaskCompleteCommand request, CancellationToken cancellationToken)
    {
        var task = await _context.Tasks
            .FirstOrDefaultAsync(t => t.Id == request.TaskId && t.UserId == request.UserId, cancellationToken);

        if (task == null) return null;

        var previousStatus = task.IsComplete ? "Complete" : "Incomplete";
        task.IsComplete = !task.IsComplete;
        var newStatus = task.IsComplete ? "Complete" : "Incomplete";

        await _context.SaveChangesAsync(cancellationToken);

        var changes = new List<FieldChangeDto>
        {
            new() { Field = "Status", From = previousStatus, To = newStatus }
        };

        var detailsPayload = JsonSerializer.Serialize(new
        {
            summary = $"Task '{task.Title}' (ID: {task.Id}) status: '{previousStatus}' → '{newStatus}'",
            changes
        });

        await _logger.LogAsync(request.UserId, "TaskToggled", detailsPayload, cancellationToken);

        return new TaskResponseDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            DueDate = task.DueDate,
            IsComplete = task.IsComplete,
            CreatedAt = task.CreatedAt
        };
    }
}
