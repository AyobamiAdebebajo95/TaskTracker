using MediatR;
using TaskTracker.Models.DTOs;

namespace TaskTracker.CQRS.Commands;

public record RegisterCommand(RegisterDto Dto) : IRequest<AuthResponseDto>;

public record LoginCommand(LoginDto Dto) : IRequest<AuthResponseDto?>;

/// <summary>
/// Revokes the supplied raw JWT string for the given user.
/// Returns false if the token could not be parsed.
/// </summary>
public record LogoutCommand(int UserId, string RawToken) : IRequest<bool>;
