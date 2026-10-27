using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Context;
using Stateflows.Common.Interfaces;

namespace Stateflows.Storage;

internal sealed class BeeStorageBridge : IStateflowsStorage
{
    private static readonly AsyncLocal<Func<Task<StateflowsContext>>?> AmbientHydrate = new();
    private static readonly AsyncLocal<Func<StateflowsContext, Task>?> AmbientDehydrate = new();

    public static IDisposable EnterScope(Func<Task<StateflowsContext>> hydrateAsync, Func<StateflowsContext, Task> dehydrateAsync)
    {
        var prevHydrate = AmbientHydrate.Value;
        var prevDehydrate = AmbientDehydrate.Value;

        AmbientHydrate.Value = hydrateAsync;
        AmbientDehydrate.Value = dehydrateAsync;

        return new ScopeGuard(() =>
        {
            AmbientHydrate.Value = prevHydrate;
            AmbientDehydrate.Value = prevDehydrate;
        });
    }

    public Task<StateflowsContext> HydrateAsync(BehaviorId behaviorId)
    {
        var hydrate = AmbientHydrate.Value;
        if (hydrate != null)
        {
            return hydrate();
        }

        return Task.FromResult(new StateflowsContext(behaviorId));
    }

    public Task DehydrateAsync(StateflowsContext context)
    {
        var dehydrate = AmbientDehydrate.Value;
        if (dehydrate != null)
        {
            return dehydrate(context);
        }

        return Task.CompletedTask;
    }

    public Task<IEnumerable<StateflowsContext>> GetAllContextsAsync(IEnumerable<BehaviorClass> behaviorClasses)
        => Task.FromResult<IEnumerable<StateflowsContext>>(Array.Empty<StateflowsContext>());

    public Task<IEnumerable<BehaviorId>> GetAllContextIdsAsync(IEnumerable<BehaviorClass> behaviorClasses)
        => Task.FromResult<IEnumerable<BehaviorId>>(Array.Empty<BehaviorId>());

    public Task<IEnumerable<StateflowsContext>> GetTimeTriggeredContextsAsync(IEnumerable<BehaviorClass> behaviorClasses)
        => Task.FromResult<IEnumerable<StateflowsContext>>(Array.Empty<StateflowsContext>());

    public Task<IEnumerable<StateflowsContext>> GetStartupTriggeredContextsAsync(IEnumerable<BehaviorClass> behaviorClasses)
        => Task.FromResult<IEnumerable<StateflowsContext>>(Array.Empty<StateflowsContext>());

    public void Dispose()
    {
    }

    private sealed class ScopeGuard(System.Action onDispose) : IDisposable
    {
        private System.Action? _onDispose = onDispose;

        public void Dispose()
        {
            var action = Interlocked.Exchange(ref _onDispose, null);
            action?.Invoke();
        }
    }
}
