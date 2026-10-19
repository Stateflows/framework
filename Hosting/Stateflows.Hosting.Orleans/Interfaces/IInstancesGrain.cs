namespace Stateflows.Interfaces;

/// <summary>
/// Index of the instances of one behavior class within one tenant, keyed by <see cref="StateflowsGrainKeys.ForBehaviorClass"/>.
/// </summary>
[Alias("Stateflows.Interfaces.IInstancesGrain")]
internal interface IInstancesGrain : IGrainWithStringKey
{
    [Alias("AddInstanceAsync")]
    Task AddInstanceAsync(string instance);
    [Alias("RemoveInstanceAsync")]
    Task RemoveInstanceAsync(string instance);
    [Alias("GetInstancesAsync")]
    Task<string[]> GetInstancesAsync();
}