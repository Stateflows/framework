using System.Collections.Concurrent;
using Stateflows.Common;

namespace Orleans.IntegrationTests;

public sealed class BehaviorOperationProbe : IBehaviorObserver
{
    public ConcurrentQueue<BehaviorCallContext> Stages { get; } = new();
    public ConcurrentQueue<BehaviorCallContext> Dispatches { get; } = new();
    public ConcurrentQueue<BehaviorCallContext> Calls { get; } = new();
    public ConcurrentQueue<(EntityOperationContext Context, object? Result)> Entities { get; } = new();
    public TaskCompletionSource DispatchStarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public async Task OnSendAsync(BehaviorCallContext context, Func<Task<SendResult>> next)
    {
        Calls.Enqueue(context);
        await next();
    }

    public async Task OnRequestAsync<T>(BehaviorCallContext context, Func<Task<RequestResult<T>>> next)
    {
        Calls.Enqueue(context);
        await next();
    }

    public async Task OnEntityOperationAsync<T>(EntityOperationContext context, Func<Task<T>> next)
    {
        var result = await next();
        Entities.Enqueue((context, result));
    }

    public void OnDispatch(BehaviorCallContext context, System.Action next)
    {
        next();
        Dispatches.Enqueue(context);
        DispatchStarted.TrySetResult();
    }

    public void OnStage(BehaviorCallContext context, System.Action next)
    {
        next();
        Stages.Enqueue(context);
    }
}
