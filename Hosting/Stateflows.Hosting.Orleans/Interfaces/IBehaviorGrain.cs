using Stateflows.Common;

namespace Stateflows.Interfaces;

public interface IBehaviorGrain : IGrainWithStringKey
{
    [Alias("ProcessAsync")]
    Task<string> ProcessAsync(string serializedEventHolder);
    
    [Alias("ProcessEventAsync")]
    Task<OrleansRequestResult> ProcessEventAsync(OrleansEventHolder eventHolder);
}