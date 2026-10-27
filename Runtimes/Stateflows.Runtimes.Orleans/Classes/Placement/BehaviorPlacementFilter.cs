using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Orleans.Placement;
using Orleans.Runtime.Placement;
using Stateflows.Common.Utilities;

namespace Stateflows;

internal sealed class BehaviorPlacementFilterAttribute()
    : PlacementFilterAttribute(new BehaviorPlacementFilterStrategy());

internal sealed class BehaviorPlacementFilterStrategy() : PlacementFilterStrategy(0);

/// <summary>
/// Restricts behavior activations to silos hosting the behavior class. Among those, only silos registering every
/// definition version of the class known in the cluster are eligible, so any persisted version can be processed and
/// new instances initialize with the same (newest) version wherever they land. When no silo hosts every version
/// (an incompatible deployment), silos hosting the newest version are used instead.
/// </summary>
internal sealed class BehaviorPlacementFilterDirector(
    ClusterBehaviorProvider cluster,
    ILocalSiloDetails localSilo,
    LocalBehaviorCatalog localCatalog,
    ILogger<BehaviorPlacementFilterDirector> logger)
    : IPlacementFilterDirector
{
    private readonly ConcurrentDictionary<(BehaviorClass, string), bool> reportedIncompatibilities = new();

    public IEnumerable<SiloAddress> Filter(
        PlacementFilterStrategy filterStrategy,
        PlacementTarget target,
        IEnumerable<SiloAddress> silos)
    {
        var identity = StateflowsGrainKeys.ParseBehavior(target.GrainIdentity.Key.ToString()!);
        var behaviorClass = identity.BehaviorId.BehaviorClass;

        var hosting = new List<(SiloAddress Silo, int[] Versions)>();
        foreach (var silo in silos)
        {
            // The local catalog is authoritative for the local silo, even before discovery has completed.
            if (silo.Equals(localSilo.SiloAddress)
                ? localCatalog.Versions.TryGetValue(behaviorClass, out var versions)
                : cluster.TryGetVersions(silo, behaviorClass, out versions))
            {
                hosting.Add((silo, versions));
            }
        }

        if (hosting.Count == 0)
        {
            throw new InvalidOperationException(
                $"No eligible silo hosts Stateflows behavior '{behaviorClass}'.");
        }

        // Silos not reporting versions are compatible with any version.
        var known = hosting.Where(entry => entry.Versions.Length > 0).ToArray();
        if (known.Length == 0)
        {
            return hosting.Select(entry => entry.Silo).ToArray();
        }

        var clusterVersions = known.SelectMany(entry => entry.Versions).ToHashSet();
        var eligible = hosting
            .Where(entry => entry.Versions.Length == 0 || clusterVersions.IsSubsetOf(entry.Versions))
            .Select(entry => entry.Silo)
            .ToArray();

        if (eligible.Length > 0)
        {
            return eligible;
        }

        var newest = clusterVersions.Max();
        var signature = string.Join(";", known
            .Select(entry => $"{entry.Silo}=[{string.Join(",", entry.Versions)}]")
            .Order());
        if (reportedIncompatibilities.TryAdd((behaviorClass, signature), true))
        {
            logger.LogWarning(
                "Incompatible Stateflows deployment: no silo hosts every version ({Versions}) of behavior {BehaviorClass}; " +
                "placing on silos hosting the newest version {Newest}. Silo versions: {SiloVersions}.",
                string.Join(",", clusterVersions.Order()), behaviorClass, newest, signature);
        }

        return hosting
            .Where(entry => entry.Versions.Length == 0 || entry.Versions.Contains(newest))
            .Select(entry => entry.Silo)
            .ToArray();
    }
}
