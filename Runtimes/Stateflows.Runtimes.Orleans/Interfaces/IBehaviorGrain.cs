namespace Stateflows.Interfaces;

[Alias("Stateflows.Interfaces.IBehaviorGrain")]
internal interface IBehaviorGrain : IGrainWithStringKey, IRemindable
{
    [Alias("ProcessEventAsync")]
    Task<OrleansRequestResult> ProcessEventAsync(OrleansEventHolder eventHolder, CancellationToken cancellationToken);
}