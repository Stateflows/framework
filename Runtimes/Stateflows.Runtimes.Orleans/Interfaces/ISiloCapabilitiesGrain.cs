namespace Stateflows.Interfaces;

/// <summary>
/// Per-silo endpoint keyed by <see cref="SiloAddress.ToParsableString"/>; always activated on the silo named by its key.
/// </summary>
[Alias("Stateflows.Interfaces.ISiloCapabilitiesGrain")]
internal interface ISiloCapabilitiesGrain : IGrainWithStringKey
{
    [Alias("GetCapabilitiesAsync")]
    Task<SiloCapabilities> GetCapabilitiesAsync();
}
