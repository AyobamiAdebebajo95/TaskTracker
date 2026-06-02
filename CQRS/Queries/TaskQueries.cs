using MediatR;
using TaskTracker.Models.DTOs;

namespace TaskTracker.CQRS.Queries;

public record GetTasksQuery(int UserId) : IRequest<List<TaskResponseDto>>;

public record GetTaskByIdQuery(int UserId, int TaskId) : IRequest<TaskResponseDto?>;

public record GetActivityLogQuery(int UserId) : IRequest<List<ActivityLogDto>>;
