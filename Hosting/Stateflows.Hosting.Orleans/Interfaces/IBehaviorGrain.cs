namespace Stateflows.Interfaces;

[Alias("Stateflows.Interfaces.IBehaviorGrain")]
internal interface IBehaviorGrain : IGrainWithStringKey
{
    [Alias("ProcessEventAsync")]
    Task<OrleansRequestResult> ProcessEventAsync(OrleansEventHolder eventHolder);
}