namespace Stateflows.Interfaces;

using Orleans.Concurrency;

[Alias("Stateflows.Interfaces.IBehaviorGrain")]
internal interface IBehaviorGrain : IGrainWithStringKey, IRemindable
{
    [Alias("ProcessEventAsync")]
    Task<OrleansRequestResult> ProcessEventAsync(OrleansEventHolder eventHolder, CancellationToken cancellationToken);

    [Alias("StreamEventAsync")]
    [AlwaysInterleave]
    Task<OrleansRequestResult> StreamEventAsync(OrleansEventHolder eventHolder, CancellationToken cancellationToken);
}