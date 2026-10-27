using Orleans.Placement;
using Orleans.Runtime.Placement;

namespace Stateflows;

/// <summary>
/// Places a grain on the silo whose parsable address is the grain's string key.
/// </summary>
[Serializable]
[GenerateSerializer]
[Immutable]
[Alias("Stateflows.SiloPinnedPlacement")]
internal sealed class SiloPinnedPlacement : PlacementStrategy
{
    internal static readonly SiloPinnedPlacement Instance = new();
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
internal sealed class SiloPinnedPlacementAttribute() : PlacementAttribute(SiloPinnedPlacement.Instance);

internal sealed class SiloPinnedPlacementDirector : IPlacementDirector
{
    public Task<SiloAddress> OnAddActivation(PlacementStrategy strategy, PlacementTarget target, IPlacementContext context)
    {
        var silo = SiloAddress.FromParsableString(target.GrainIdentity.Key.ToString()!);
        if (!context.GetCompatibleSilos(target).Contains(silo))
        {
            throw new SiloUnavailableException($"Silo {silo} is not an active member of the cluster.");
        }

        return Task.FromResult(silo);
    }
}
