using MediatR;

namespace TaskTracker.CQRS.Queries;

/// <summary>Returns true if the email is already registered.</summary>
public record EmailExistsQuery(string Email) : IRequest<bool>;
