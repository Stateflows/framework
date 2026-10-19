using Stateflows.Common.Utilities;
using Stateflows.Interfaces;

namespace Stateflows;

internal class StateflowsInstances(
    [PersistentState("instances", "stateflows")]
    IPersistentState<HashSet<string>> instancesState
) : Grain, IInstancesGrain
{
    public static string GetKey(string? tenantId, BehaviorClass behaviorClass)
        => StateflowsGrainKeys.ForBehaviorClass(tenantId, behaviorClass);

    public async Task AddInstanceAsync(string instance)
    {
        instancesState.State ??= [];
        if (instancesState.State.Add(instance))
        {
            await WriteAsync(() => instancesState.State.Remove(instance));
        }
    }

    public async Task RemoveInstanceAsync(string instance)
    {
        if (instancesState.State?.Remove(instance) == true)
        {
            await WriteAsync(() => instancesState.State.Add(instance));
        }
    }

    public Task<string[]> GetInstancesAsync()
        => Task.FromResult(instancesState.State?.ToArray() ?? []);

    private async Task WriteAsync(System.Action rollback)
    {
        try
        {
            await instancesState.WriteStateAsync();
        }
        catch
        {
            StateflowsOrleansDiagnostics.PersistenceFailed("instances");
            rollback();
            // The provider may have committed before failing; reload on the next activation.
            DeactivateOnIdle();
            throw;
        }
    }
}