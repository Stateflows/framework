using Stateflows.Interfaces;

namespace Stateflows;

/// <summary>
/// Retrieves the behavior capabilities of a specific silo.
/// </summary>
internal interface ISiloCapabilitiesClient
{
    Task<SiloCapabilities> GetCapabilitiesAsync(SiloAddress silo, CancellationToken cancellationToken);
}

internal sealed class SiloCapabilitiesClient(IGrainFactory grainFactory) : ISiloCapabilitiesClient
{
    public Task<SiloCapabilities> GetCapabilitiesAsync(SiloAddress silo, CancellationToken cancellationToken)
        => grainFactory
            .GetGrain<ISiloCapabilitiesGrain>(silo.ToParsableString())
            .GetCapabilitiesAsync()
            .WaitAsync(cancellationToken);
}
