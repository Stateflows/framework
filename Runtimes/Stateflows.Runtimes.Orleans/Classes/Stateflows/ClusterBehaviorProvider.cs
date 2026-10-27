using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stateflows.Common;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Utilities;

namespace Stateflows;

internal sealed class BehaviorDiscoveryOptions
{
    public TimeSpan RefreshPeriod { get; set; } = TimeSpan.FromSeconds(1);

    public TimeSpan FetchTimeout { get; set; } = TimeSpan.FromSeconds(5);
}

/// <summary>
/// Discovers behavior classes and versions hosted by active silos. Each silo's capabilities are pulled from
/// the silo itself and cached for its lifetime (a restarted silo has a new address); failed pulls are retried
/// on every refresh and departed silos are dropped.
/// </summary>
internal sealed class ClusterBehaviorProvider(
    IServiceProvider services,
    IOptions<BehaviorDiscoveryOptions> options,
    ILogger<ClusterBehaviorProvider> logger)
    : BackgroundService, IBehaviorProvider
{
    private sealed record Snapshot(
        ImmutableDictionary<SiloAddress, IReadOnlyDictionary<BehaviorClass, int[]>> Silos,
        BehaviorClass[] Classes);

    private Snapshot snapshot = new(ImmutableDictionary<SiloAddress, IReadOnlyDictionary<BehaviorClass, int[]>>.Empty, []);
    private readonly HashSet<SiloAddress> failingSilos = [];

    public bool IsLocal => false;

    public IEnumerable<BehaviorClass> BehaviorClasses => System.Threading.Volatile.Read(ref snapshot).Classes;

    public event ActionAsync<IBehaviorProvider>? BehaviorClassesChanged;

    /// <summary>
    /// Silos with known capabilities.
    /// </summary>
    public IReadOnlyCollection<SiloAddress> KnownSilos => System.Threading.Volatile.Read(ref snapshot).Silos.Keys.ToArray();

    public bool TryGetVersions(SiloAddress silo, BehaviorClass behaviorClass, out int[] versions)
    {
        versions = [];
        return System.Threading.Volatile.Read(ref snapshot).Silos.TryGetValue(silo, out var capabilities)
               && capabilities.TryGetValue(behaviorClass, out versions!);
    }

    public bool TryProvideBehavior(BehaviorId id, out IBehavior behavior)
    {
        if (!System.Threading.Volatile.Read(ref snapshot).Classes.Contains(id.BehaviorClass))
        {
            behavior = null!;
            return false;
        }

        behavior = services.GetRequiredService<IBehaviorFactory>().CreateBehavior(id);
        return true;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(options.Value.RefreshPeriod);

        do
        {
            try
            {
                await RefreshAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                logger.LogWarning(exception, "Stateflows behavior discovery refresh failed; retrying on the next tick.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    internal async Task RefreshAsync(CancellationToken cancellationToken)
    {
        var membership = services.GetRequiredService<IClusterMembershipService>();
        var localSilo = services.GetRequiredService<ILocalSiloDetails>().SiloAddress;
        var catalog = services.GetRequiredService<LocalBehaviorCatalog>();
        var current = System.Threading.Volatile.Read(ref snapshot);

        var active = membership.CurrentSnapshot.Members.Values
            .Where(member => member.Status == SiloStatus.Active)
            .Select(member => member.SiloAddress)
            .ToArray();

        var builder = ImmutableDictionary.CreateBuilder<SiloAddress, IReadOnlyDictionary<BehaviorClass, int[]>>();
        var missing = new List<SiloAddress>();
        foreach (var silo in active)
        {
            if (silo.Equals(localSilo))
            {
                builder[silo] = catalog.Versions;
            }
            else if (current.Silos.TryGetValue(silo, out var known))
            {
                builder[silo] = known;
            }
            else
            {
                missing.Add(silo);
            }
        }

        var fetched = await Task.WhenAll(missing.Select(silo => FetchAsync(silo, cancellationToken)));
        foreach (var (silo, capabilities) in missing.Zip(fetched))
        {
            if (capabilities != null)
            {
                builder[silo] = capabilities;
            }
        }

        lock (failingSilos)
        {
            failingSilos.RemoveWhere(silo => !active.Contains(silo));
        }

        var silos = builder.ToImmutable();
        var classes = silos.Values.SelectMany(capabilities => capabilities.Keys).Distinct().ToArray();
        System.Threading.Volatile.Write(ref snapshot, new Snapshot(silos, classes));

        if (!silos.Keys.ToHashSet().SetEquals(current.Silos.Keys) || !classes.ToHashSet().SetEquals(current.Classes))
        {
            logger.LogInformation(
                "Stateflows behavior discovery: {KnownSilos} of {ActiveSilos} active silos known, {ClassCount} behavior classes available.",
                silos.Count, active.Length, classes.Length);
        }

        if (!classes.ToHashSet().SetEquals(current.Classes) && BehaviorClassesChanged is { } handlers)
        {
            foreach (ActionAsync<IBehaviorProvider> handler in handlers.GetInvocationList())
            {
                try
                {
                    await handler(this);
                }
                catch (Exception exception)
                {
                    logger.LogError(exception, "Stateflows behavior classes change handler failed.");
                }
            }
        }
    }

    private async Task<IReadOnlyDictionary<BehaviorClass, int[]>?> FetchAsync(SiloAddress silo, CancellationToken cancellationToken)
    {
        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(options.Value.FetchTimeout);
            var capabilities = await services.GetRequiredService<ISiloCapabilitiesClient>()
                .GetCapabilitiesAsync(silo, timeout.Token);

            if (!silo.Equals(capabilities.Silo))
            {
                throw new InvalidOperationException($"Capabilities requested from silo {silo} were returned by silo {capabilities.Silo}.");
            }

            lock (failingSilos)
            {
                if (failingSilos.Remove(silo))
                {
                    logger.LogInformation("Stateflows capabilities of silo {Silo} retrieved after earlier failures.", silo);
                }
            }

            return capabilities.ToVersions();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            StateflowsOrleansDiagnostics.DiscoveryFailed();
            bool firstFailure;
            lock (failingSilos)
            {
                firstFailure = failingSilos.Add(silo);
            }

            logger.Log(
                firstFailure ? LogLevel.Warning : LogLevel.Debug,
                exception,
                "Failed to retrieve Stateflows capabilities of silo {Silo}; retrying on the next refresh.",
                silo);
            return null;
        }
    }
}
