using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskTracker.CQRS.Commands;
using TaskTracker.CQRS.Queries;
using TaskTracker.Models.DTOs;

namespace TaskTracker.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly IMediator _mediator;

    public TasksController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Get all tasks for the logged-in user.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<TaskResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTasks()
    {
        var tasks = await _mediator.Send(new GetTasksQuery(GetUserId()));
        return Ok(tasks);
    }

    /// <summary>Get a single task by ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TaskResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTask(int id)
    {
        var task = await _mediator.Send(new GetTaskByIdQuery(GetUserId(), id));
        return task is null ? NotFound() : Ok(task);
    }

    /// <summary>Create a new task.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(TaskResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTask(CreateTaskDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var task = await _mediator.Send(new CreateTaskCommand(GetUserId(), dto));
        return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
    }

    /// <summary>Update an existing task (title, description, due date, status).</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(TaskResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateTask(int id, UpdateTaskDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var task = await _mediator.Send(new UpdateTaskCommand(GetUserId(), id, dto));
        return task is null ? NotFound() : Ok(task);
    }

    /// <summary>Toggle a task between complete and incomplete without a full update.</summary>
    [HttpPatch("{id:int}/toggle")]
    [ProducesResponseType(typeof(TaskResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleTask(int id)
    {
        var task = await _mediator.Send(new ToggleTaskCompleteCommand(GetUserId(), id));
        return task is null ? NotFound() : Ok(task);
    }

    /// <summary>Delete a task permanently.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTask(int id)
    {
        var deleted = await _mediator.Send(new DeleteTaskCommand(GetUserId(), id));
        return deleted ? NoContent() : NotFound();
    }

    /// <summary>Get the activity log for the logged-in user.</summary>
    [HttpGet("activity")]
    [ProducesResponseType(typeof(List<ActivityLogDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActivityLog()
    {
        var logs = await _mediator.Send(new GetActivityLogQuery(GetUserId()));
        return Ok(logs);
    }
}
