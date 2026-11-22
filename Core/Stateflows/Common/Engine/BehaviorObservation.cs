using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Stateflows.Common.Engine;

internal sealed class BehaviorObservation : IBehaviorDispatchObserver
{
    private sealed class Frame(BehaviorId source, IBehaviorObserver[] observers)
    {
        public readonly BehaviorId Source = source;
        public readonly IBehaviorObserver[] Observers = observers;
        public volatile bool Active = true;
    }

    private sealed class Operation(Guid id)
    {
        public readonly Guid Id = id;
        public volatile bool Active = true;
    }

    private sealed class DispatchMarker
    {
        public volatile bool Active = true;
    }

    private static readonly AsyncLocal<Frame?> Current = new();
    private static readonly AsyncLocal<Operation?> ParentOperation = new();
    private static readonly AsyncLocal<DispatchMarker?> SynchronousDispatch = new();

    public static IDisposable Enter(BehaviorId id, IServiceProvider services)
    {
        var previous = Current.Value;
        var previousOperation = ParentOperation.Value;
        var previousDispatch = SynchronousDispatch.Value;
        var frame = new Frame(id, services.GetServices<IBehaviorObserver>().ToArray());
        Current.Value = frame;
        ParentOperation.Value = null;
        SynchronousDispatch.Value = null;
        var ambient = BehaviorDispatchAmbient.Enter(services.GetRequiredService<IBehaviorDispatchObserver>());
        return new Scope(() =>
        {
            frame.Active = false;
            ambient.Dispose();
            Current.Value = previous;
            ParentOperation.Value = previousOperation;
            SynchronousDispatch.Value = previousDispatch;
        });
    }

    private static Frame? Active => Current.Value is { Active: true } frame ? frame : null;

    public Task<SendResult> SendAsync(BehaviorId target, EventHolder envelope, Func<Task<SendResult>> next)
    {
        var frame = Active;
        if (frame == null || SynchronousDispatch.Value is { Active: true }) return next();
        var context = new BehaviorCallContext(frame.Source, target, envelope, CurrentOperationId);
        return ObserverPipeline.RunAsync(frame.Observers, next, (observer, inner) => observer.OnSendAsync(context, inner));
    }

    public Task<RequestResult<TResponse>> RequestAsync<TResponse>(BehaviorId target, EventHolder envelope, Func<Task<RequestResult<TResponse>>> next)
    {
        var frame = Active;
        if (frame == null) return next();
        var context = new BehaviorCallContext(frame.Source, target, envelope, CurrentOperationId);
        return ObserverPipeline.RunAsync(frame.Observers, next, (observer, inner) => observer.OnRequestAsync(context, inner));
    }

    public static async Task<TResult> EntityAsync<TResult>(BehaviorId? owner, BehaviorId? target,
        string entityName, EntityOperationKind kind, object? request,
        string? definitionElementId, Func<Task<TResult>> next)
    {
        var frame = Active;
        if (frame == null) return await next();
        var context = new EntityOperationContext(Guid.NewGuid(), frame.Source, owner, target, entityName, kind, request, definitionElementId);
        return await ObserverPipeline.RunAsync(frame.Observers, async () =>
        {
            var previous = ParentOperation.Value;
            var operation = new Operation(context.OperationId);
            ParentOperation.Value = operation;
            try { return await next(); }
            finally { operation.Active = false; ParentOperation.Value = previous; }
        }, (observer, inner) => observer.OnEntityOperationAsync(context, inner));
    }

    public static void Dispatch(BehaviorId target, EventHolder envelope, System.Action next)
        => Dispatch(target, envelope, context => { next(); context.Dispatched = true; });

    public static void Dispatch(BehaviorId target, EventHolder envelope, System.Action<BehaviorCallContext> next)
    {
        var frame = Active;
        if (frame == null)
        {
            next(new BehaviorCallContext(default, target, envelope, null));
            return;
        }
        var context = new BehaviorCallContext(frame.Source, target, envelope, CurrentOperationId);
        RunSync(frame.Observers, () =>
        {
            var previous = SynchronousDispatch.Value;
            var marker = new DispatchMarker();
            SynchronousDispatch.Value = marker;
            try { next(context); }
            finally { marker.Active = false; SynchronousDispatch.Value = previous; }
        }, (observer, inner) => observer.OnDispatch(context, inner));
    }

    private static Guid? CurrentOperationId
        => ParentOperation.Value is { Active: true } operation ? operation.Id : null;

    private static void RunSync(IReadOnlyList<IBehaviorObserver> observers, System.Action next, System.Action<IBehaviorObserver, System.Action> observe)
    {
        for (var i = observers.Count - 1; i >= 0; i--)
        {
            var observer = observers[i];
            var inner = next;
            next = () =>
            {
                var count = 0;
                ExceptionDispatchInfo? failure = null;
                observe(observer, () =>
                {
                    if (Interlocked.Increment(ref count) != 1)
                        throw new InvalidOperationException("Observer invoked its continuation more than once.");
                    try { inner(); }
                    catch (Exception exception) { failure = ExceptionDispatchInfo.Capture(exception); throw; }
                });
                if (count != 1) throw new InvalidOperationException("Observer did not invoke its continuation exactly once.");
                failure?.Throw();
            };
        }
        next();
    }

    private sealed class Scope(System.Action dispose) : IDisposable
    {
        public void Dispose() => dispose();
    }
}
