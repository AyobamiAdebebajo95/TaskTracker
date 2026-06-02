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
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Register a new user account.</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var emailTaken = await _mediator.Send(new EmailExistsQuery(dto.Email));
        if (emailTaken) return BadRequest("Email already exists");

        var result = await _mediator.Send(new RegisterCommand(dto));
        return Ok(result);
    }

    /// <summary>Log in and receive a JWT token.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _mediator.Send(new LoginCommand(dto));
        return result is null
            ? Unauthorized("Invalid email or password")
            : Ok(result);
    }

    /// <summary>Invalidate the current JWT token (logout).</summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Logout()
    {
        var authHeader = Request.Headers.Authorization.FirstOrDefault();
        if (authHeader == null || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return BadRequest("No token provided");

        var rawToken = authHeader["Bearer ".Length..].Trim();
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var success = await _mediator.Send(new LogoutCommand(userId, rawToken));
        return success
            ? Ok(new { message = "Logged out successfully" })
            : BadRequest("Invalid token format");
    }
}
