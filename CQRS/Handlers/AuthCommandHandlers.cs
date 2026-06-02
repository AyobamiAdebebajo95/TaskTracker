using System.IdentityModel.Tokens.Jwt;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskTracker.CQRS.Commands;
using TaskTracker.Data;
using TaskTracker.Models;
using TaskTracker.Models.DTOs;
using TaskTracker.Services;

namespace TaskTracker.CQRS.Handlers;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponseDto>
{
    private readonly AppDbContext _context;
    private readonly IAuthService _authService;
    private readonly IActivityLoggerService _logger;

    public RegisterCommandHandler(AppDbContext context, IAuthService authService, IActivityLoggerService logger)
    {
        _context = context;
        _authService = authService;
        _logger = logger;
    }

    public async Task<AuthResponseDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var user = new User
        {
            Email = request.Dto.Email,
            PasswordHash = _authService.HashPassword(request.Dto.Password)
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        await _logger.LogAsync(user.Id, "Register",
            $"New account created for {user.Email}", cancellationToken);

        return new AuthResponseDto
        {
            UserId = user.Id,
            Email = user.Email,
            Token = _authService.GenerateJwtToken(user)
        };
    }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto?>
{
    private readonly AppDbContext _context;
    private readonly IAuthService _authService;
    private readonly IActivityLoggerService _logger;

    public LoginCommandHandler(AppDbContext context, IAuthService authService, IActivityLoggerService logger)
    {
        _context = context;
        _authService = authService;
        _logger = logger;
    }

    public async Task<AuthResponseDto?> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Dto.Email, cancellationToken);

        if (user == null || !_authService.VerifyPassword(request.Dto.Password, user.PasswordHash))
            return null;

        await _logger.LogAsync(user.Id, "Login",
            $"User {user.Email} logged in", cancellationToken);

        return new AuthResponseDto
        {
            UserId = user.Id,
            Email = user.Email,
            Token = _authService.GenerateJwtToken(user)
        };
    }
}

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, bool>
{
    private readonly AppDbContext _context;
    private readonly IActivityLoggerService _logger;

    public LogoutCommandHandler(AppDbContext context, IActivityLoggerService logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var handler = new JwtSecurityTokenHandler();

        if (!handler.CanReadToken(request.RawToken))
            return false;

        var jwt = handler.ReadJwtToken(request.RawToken);

        _context.RevokedTokens.Add(new RevokedToken
        {
            Jti = jwt.Id,
            UserId = request.UserId,
            RevokedAt = DateTime.UtcNow,
            ExpiresAt = jwt.ValidTo
        });

        await _context.SaveChangesAsync(cancellationToken);

        await _logger.LogAsync(request.UserId, "Logout",
            "User logged out and token revoked", cancellationToken);

        return true;
    }
}
