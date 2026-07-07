using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace STLMS.Application.Common.Mediator;

public class AppMediator(IServiceProvider serviceProvider) : IAppMediator
{
    private static readonly ConcurrentDictionary<Type, MethodInfo> HandleMethodCache = new();

    public async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken ct = default)
    {
        var requestType = request.GetType();
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));
        var behaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, typeof(TResponse));

        var handler = serviceProvider.GetService(handlerType)
            ?? throw new InvalidOperationException($"No handler registered for request type '{requestType.Name}'.");

        var handleMethod = HandleMethodCache.GetOrAdd(handlerType, t => t.GetMethod("HandleAsync")!);

        RequestHandlerDelegate<TResponse> handlerDelegate = () =>
            (Task<TResponse>)handleMethod.Invoke(handler, [request, ct])!;

        var behaviorHandleMethod = HandleMethodCache.GetOrAdd(behaviorType, t => t.GetMethod("HandleAsync")!);
        var behaviors = serviceProvider.GetServices(behaviorType).Cast<object>().Reverse();
        foreach (var behavior in behaviors)
        {
            var next = handlerDelegate;
            handlerDelegate = () => (Task<TResponse>)behaviorHandleMethod.Invoke(behavior, [request, next, ct])!;
        }

        return await handlerDelegate();
    }
}
