namespace Stateflows.Interfaces;

[Alias("Stateflows.Interfaces.ICancellationGrain")]
public interface ICancellationGrain : IGrainWithStringKey
{
    [Alias("RequestCancellationAsync")]
    Task RequestCancellationAsync();

    [Alias("IsCancellationRequestedAsync")]
    Task<bool> IsCancellationRequestedAsync();
}