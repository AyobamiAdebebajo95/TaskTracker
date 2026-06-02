using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskTracker.CQRS.Queries;
using TaskTracker.Data;

namespace TaskTracker.CQRS.Handlers;

public class EmailExistsQueryHandler : IRequestHandler<EmailExistsQuery, bool>
{
    private readonly AppDbContext _context;

    public EmailExistsQueryHandler(AppDbContext context) => _context = context;

    public async Task<bool> Handle(EmailExistsQuery request, CancellationToken cancellationToken) =>
        await _context.Users.AnyAsync(u => u.Email == request.Email, cancellationToken);
}
