using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Context;
using Stateflows.Common.Interfaces;
using Stateflows.Interfaces;

namespace Stateflows.Storage;

/// <summary>
/// Orleans implementation of <see cref="IStateflowsStorage"/>. Behavior state is owned by behavior grains:
/// it can be hydrated and dehydrated only inside a behavior grain's execution scope. Listing is served by the instance index.
/// </summary>
internal sealed class GrainStorageBridge(
    IGrainFactory grainFactory,
    IStateflowsTenantProvider tenantProvider
) : IStateflowsStorage
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
        var hydrate = AmbientHydrate.Value
            ?? throw OutOfScope("hydrated", behaviorId);

        return hydrate();
    }

    public Task DehydrateAsync(StateflowsContext context)
    {
        var dehydrate = AmbientDehydrate.Value
            ?? throw OutOfScope("dehydrated", context.Id);

        return dehydrate(context);
    }

    private static InvalidOperationException OutOfScope(string operation, BehaviorId behaviorId)
        => new($"State of Stateflows behavior '{behaviorId}' cannot be {operation} outside of its behavior grain. " +
               "Under Orleans, behavior state is owned by the behavior grain; interact with the behavior by sending events or requests to it.");

    public Task<IEnumerable<StateflowsContext>> GetAllContextsAsync(IEnumerable<BehaviorClass> behaviorClasses)
        => throw new NotSupportedException(
            "Reading behavior contexts outside of behavior grains is not supported under Orleans. Use GetAllContextIdsAsync and send requests to the behaviors.");

    public async Task<IEnumerable<BehaviorId>> GetAllContextIdsAsync(IEnumerable<BehaviorClass> behaviorClasses)
    {
        var tenantId = tenantProvider.GetCurrentTenantId();
        var results = await Task.WhenAll(behaviorClasses
            .Distinct()
            .Select(async behaviorClass =>
            {
                var index = grainFactory.GetGrain<IInstancesGrain>(StateflowsInstances.GetKey(tenantId, behaviorClass));
                var instances = await index.GetInstancesAsync();
                return instances.Select(instance => new BehaviorId(behaviorClass, instance));
            }));

        return results.SelectMany(ids => ids).ToArray();
    }

    // Used only by the in-process scheduler and startup executor; Orleans uses reminders and default-instance initialization instead.
    public Task<IEnumerable<StateflowsContext>> GetTimeTriggeredContextsAsync(IEnumerable<BehaviorClass> behaviorClasses)
        => throw new NotSupportedException("Time-triggered context scanning is not used under Orleans; behavior grains schedule time events with reminders.");

    public Task<IEnumerable<StateflowsContext>> GetStartupTriggeredContextsAsync(IEnumerable<BehaviorClass> behaviorClasses)
        => throw new NotSupportedException("Startup-triggered context scanning is not used under Orleans.");

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