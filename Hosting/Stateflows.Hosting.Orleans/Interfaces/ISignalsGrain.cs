namespace Stateflows.Interfaces;

public static class Signals
{
    public const string Cancel = nameof(Cancel);
}

[Alias("Stateflows.Interfaces.ISignalsGrain")]
public interface ISignalsGrain : IGrainWithStringKey
{
    [Alias("EnableSignalAsync")]
    Task EnableSignalAsync(string signalName);
    [Alias("DisableSignalAsync")]
    Task DisableSignalAsync(string signalName);
    [Alias("IsSignalEnabledAsync")]
    Task<bool> IsSignalEnabledAsync(string signalName);
}