using Stateflows.Interfaces;

namespace Stateflows;

[KeepAlive]
public class SignalsGrain : Grain, ISignalsGrain
{
    public Task EnableSignalAsync(string signalName)
    {
        throw new NotImplementedException();
    }

    public Task DisableSignalAsync(string signalName)
    {
        throw new NotImplementedException();
    }

    public Task<bool> IsSignalEnabledAsync(string signalName)
    {
        throw new NotImplementedException();
    }
}