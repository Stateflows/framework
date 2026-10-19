using Stateflows.Common.Interfaces;

namespace Stateflows;

/// <summary>
/// Behavior classes and definition versions hosted by the local silo.
/// </summary>
internal sealed class LocalBehaviorCatalog
{
    public LocalBehaviorCatalog(IEnumerable<IBehaviorProvider> providers)
    {
        var localProviders = providers.Where(provider => provider.IsLocal).ToArray();
        var versions = localProviders
            .SelectMany(provider => provider.BehaviorClasses)
            .Distinct()
            .ToDictionary(behaviorClass => behaviorClass, _ => Array.Empty<int>());

        foreach (var provider in localProviders.OfType<IVersionedBehaviorProvider>())
        {
            foreach (var (behaviorClass, classVersions) in provider.BehaviorVersions)
            {
                versions[behaviorClass] = versions.TryGetValue(behaviorClass, out var known)
                    ? known.Union(classVersions).Order().ToArray()
                    : classVersions.Order().ToArray();
            }
        }

        Versions = versions;
        Classes = versions.Keys.ToArray();
    }

    public BehaviorClass[] Classes { get; }

    /// <summary>
    /// Registered versions per class; an empty array means the provider does not report versions.
    /// </summary>
    public IReadOnlyDictionary<BehaviorClass, int[]> Versions { get; }

    public bool Supports(BehaviorClass behaviorClass, int version)
        => Versions.TryGetValue(behaviorClass, out var versions)
           && (version == 0 || versions.Length == 0 || versions.Contains(version));

    public SiloCapabilities ToCapabilities(SiloAddress silo)
        => new()
        {
            Silo = silo,
            Behaviors = Versions
                .Select(entry => new BehaviorCapability { Class = entry.Key, Versions = entry.Value })
                .ToArray()
        };
}

[GenerateSerializer]
[Immutable]
[Alias("Stateflows.BehaviorCapability")]
internal sealed class BehaviorCapability
{
    [Id(0)]
    public OrleansBehaviorClass Class { get; init; }

    [Id(1)]
    public int[] Versions { get; init; } = [];
}

[GenerateSerializer]
[Immutable]
[Alias("Stateflows.SiloCapabilities")]
internal sealed class SiloCapabilities
{
    [Id(0)]
    public SiloAddress Silo { get; init; } = null!;

    [Id(1)]
    public BehaviorCapability[] Behaviors { get; init; } = [];

    internal IReadOnlyDictionary<BehaviorClass, int[]> ToVersions()
        => Behaviors
            .GroupBy(capability => (BehaviorClass)capability.Class)
            .ToDictionary(group => group.Key, group => group.SelectMany(capability => capability.Versions).Distinct().Order().ToArray());
}
