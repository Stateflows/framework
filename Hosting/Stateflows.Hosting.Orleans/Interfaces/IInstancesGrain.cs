namespace Stateflows.Interfaces;

[Alias("Stateflows.Interfaces.IInstancesGrain")]
internal interface IInstancesGrain : IGrainWithStringKey
{
    [Alias("AddInstanceAsync")]
    Task AddInstanceAsync(OrleansBehaviorId behaviorId);
    [Alias("RemoveInstanceAsync")]
    Task RemoveInstanceAsync(OrleansBehaviorId behaviorId);
    [Alias("GetInstancesAsync")]
    Task<string[]> GetInstancesAsync(OrleansBehaviorClass behaviorClass);
}