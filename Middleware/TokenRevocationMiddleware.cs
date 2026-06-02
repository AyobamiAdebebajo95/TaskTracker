using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TaskTracker.Data;
using TaskTracker.Settings;

namespace TaskTracker.Middleware;

public class TokenRevocationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly TokenValidationParameters _validationParameters;

    // RequestDelegate and singleton/transient services go in the constructor.
    // Scoped services (AppDbContext) are resolved per-request in InvokeAsync.
    public TokenRevocationMiddleware(RequestDelegate next, IOptions<JwtSettings> jwtOptions)
    {
        _next = next;

        var jwt = jwtOptions.Value;

        // Mirror exactly the same parameters used in Program.cs AddJwtBearer
        _validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwt.SecretKey)),
            // Keep clock skew tight — we want expiry to be exact
            ClockSkew = TimeSpan.Zero
        };
    }

    public async Task InvokeAsync(HttpContext context, AppDbContext db)
    {
        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();

        // No Bearer token present — let the pipeline continue.
        // UseAuthentication will handle anonymous/missing-token cases.
        if (authHeader == null || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        var rawToken = authHeader["Bearer ".Length..].Trim();
        var handler = new JwtSecurityTokenHandler();

        // ── Step 1: Validate signature, issuer, audience, and expiry ─────────
        ClaimsPrincipal principal;
        try
        {
            principal = handler.ValidateToken(rawToken, _validationParameters, out _);
        }
        catch (SecurityTokenExpiredException)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new
            {
                message = "Token has expired. Please log in again."
            });
            return;
        }
        catch (SecurityTokenInvalidSignatureException)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new
            {
                message = "Token signature is invalid."
            });
            return;
        }
        catch (SecurityTokenException ex)
        {
            // Covers invalid issuer, audience, not-yet-valid, malformed, etc.
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new
            {
                message = $"Token validation failed: {ex.Message}"
            });
            return;
        }

        // ── Step 2: Check revocation (only runs on a structurally valid token) ─
        var jti = principal.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

        if (!string.IsNullOrEmpty(jti))
        {
            var isRevoked = await db.RevokedTokens.AnyAsync(r => r.Jti == jti);

            if (isRevoked)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new
                {
                    message = "Token has been revoked. Please log in again."
                });
                return;
            }
        }

        await _next(context);
    }
}
