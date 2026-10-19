using Stateflows.Interfaces;

namespace Stateflows;

[SiloPinnedPlacement]
internal sealed class SiloCapabilitiesGrain(LocalBehaviorCatalog catalog, ILocalSiloDetails localSilo)
    : Grain, ISiloCapabilitiesGrain
{
    public Task<SiloCapabilities> GetCapabilitiesAsync()
        => Task.FromResult(catalog.ToCapabilities(localSilo.SiloAddress));
}
