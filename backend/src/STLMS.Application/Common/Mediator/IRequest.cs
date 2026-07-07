namespace STLMS.Application.Common.Mediator;

/// <summary>
/// Minimal, fully in-house CQRS mediator - deliberately not using the MediatR NuGet package.
/// MediatR (and AutoMapper) moved to a paid commercial license for v13+/v15+ in 2025; staying on
/// the last free version isn't a safe option either (AutoMapper 14.0.0 has an unpatched,
/// unauthenticated DoS advisory - GHSA-rvv3-g6hj-g44x - that was only fixed in the paid editions).
/// This is a small enough pattern (marker interface + handler + pipeline) to own outright instead
/// of taking on that licensing/security risk for something this central to the architecture.
/// </summary>
public interface IRequest<TResponse> { }

public interface IRequestHandler<in TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    Task<TResponse> HandleAsync(TRequest request, CancellationToken ct);
}

public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();

public interface IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct);
}

public interface IAppMediator
{
    Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken ct = default);
}
