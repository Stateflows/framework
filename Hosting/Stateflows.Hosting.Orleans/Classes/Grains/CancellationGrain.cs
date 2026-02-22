using Stateflows.Interfaces;

namespace Stateflows;

[KeepAlive]
public class CancellationGrain : Grain, ICancellationGrain
{
    private bool isCancellationRequested = false;

    public Task RequestCancellationAsync()
    {
        isCancellationRequested = true;
        
        return Task.CompletedTask;
    }

    public Task<bool> IsCancellationRequestedAsync()
        => Task.FromResult(isCancellationRequested);
}