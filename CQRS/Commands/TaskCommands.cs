using MediatR;
using TaskTracker.Models.DTOs;

namespace TaskTracker.CQRS.Commands;

public record CreateTaskCommand(int UserId, CreateTaskDto TaskDto) : IRequest<TaskResponseDto>;

public record UpdateTaskCommand(int UserId, int TaskId, UpdateTaskDto UpdateDto) : IRequest<TaskResponseDto?>;

public record DeleteTaskCommand(int UserId, int TaskId) : IRequest<bool>;

/// <summary>Flips IsComplete between true and false without requiring a full update body.</summary>
public record ToggleTaskCompleteCommand(int UserId, int TaskId) : IRequest<TaskResponseDto?>;

