using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Configuration;
using Orleans.Runtime;
using Orleans.Storage;
using Stateflows;
using Stateflows.Common;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Registration.Interfaces;
using Stateflows.StateMachines;

namespace Orleans.IntegrationTests;

/// <summary>
/// Discovery, placement and rolling-upgrade contract under test:
/// - a silo's behavior classes and versions are discovered by every silo once it is Active, and dropped once it leaves
///   (gracefully or abruptly); failed or hanging capability pulls are retried without needing a membership change;
/// - when the last silo hosting a class is gone, sends fail fast with "No eligible silo hosts" and persisted state stays
///   intact; the instance resumes once a hosting silo rejoins;
/// - after abrupt termination, an instance reactivates on another eligible silo with its committed state;
/// - placement prefers silos hosting every version of a class known in the cluster; if none does, silos hosting the
///   newest version are used and a warning is logged; an activation never runs a persisted version it does not host.
/// </summary>
[TestClass]
public sealed class BehaviorPlacementResilienceTests
{
    private static readonly TimeSpan ConvergenceTimeout = TimeSpan.FromSeconds(30);
    private const string Workflow = "workflow";

    [TestInitialize]
    public void Initialize() => Trace.Reset();

    [TestMethod]
    [Timeout(180_000)]
    public async Task LosingLastHostingSiloFailsFastKeepsStateAndResumesWhenHostRejoins()
    {
        await using var cluster = new PlacementCluster();
        var primary = await cluster.StartHostAsync("primary", _ => { });
        var hosting = await cluster.StartHostAsync("hosting-1", WorkflowVersions("hosting-1", 1));
        await WaitUntilAsync(() => cluster.CanLocate(primary, Workflow), "The workflow class must be discovered.");

        var machine = cluster.Locate(primary, new StateMachineId(Workflow, Guid.NewGuid().ToString("N")));
        Assert.AreEqual(EventStatus.Initialized, (await machine.SendAsync(new Initialize())).Status);
        Assert.AreEqual(EventStatus.Consumed, (await machine.SendAsync(new Next())).Status);
        Assert.AreEqual("v1-middle", await StateOfAsync(machine));

        await cluster.KillHostAsync(hosting);
        await WaitUntilAsync(() => !cluster.CanLocate(primary, Workflow), "The class of the lost silo must be undiscovered.");

        var failure = await WaitForFailureAsync(() => machine.SendAsync(new Next()), "No eligible silo hosts Stateflows behavior");
        var failFast = Stopwatch.StartNew();
        failure = await CaptureFailureAsync(() => machine.SendAsync(new Next()));
        failFast.Stop();
        StringAssert.Contains(Messages(failure), "No eligible silo hosts Stateflows behavior");
        Assert.IsTrue(failFast.Elapsed < TimeSpan.FromSeconds(5), $"Placement must fail fast, took {failFast.Elapsed}.");

        var persisted = cluster.Storage.GetContext(machine.Id)!;
        Assert.AreEqual(1, persisted.Version, "Persisted state must be left intact.");

        await cluster.StartHostAsync("hosting-2", WorkflowVersions("hosting-2", 1));
        await WaitUntilAsync(() => cluster.CanLocate(primary, Workflow), "The rejoined class must be discovered.");

        Assert.AreEqual(EventStatus.Consumed, (await SendWithRetryAsync(() => machine.SendAsync(new Next()))).Status);
        Assert.AreEqual("v1-end", await StateOfAsync(machine));
        CollectionAssert.AreEqual(
            new[] { ("hosting-1", 1), ("hosting-2", 1) },
            Trace.Of(machine.Id).ToArray(),
            "The instance must resume from its committed state on the rejoined silo.");
        machine.Dispose();
    }

    [TestMethod]
    [Timeout(180_000)]
    public async Task InstanceReactivatesOnAnotherEligibleSiloAfterAbruptTermination()
    {
        await using var cluster = new PlacementCluster();
        var primary = await cluster.StartHostAsync("primary", _ => { });
        var a = await cluster.StartHostAsync("a", WorkflowVersions("a", 1));
        var b = await cluster.StartHostAsync("b", WorkflowVersions("b", 1));
        await WaitUntilAsync(() => cluster.KnownSilos(primary) == 3, "All silos must be discovered.");

        var machine = cluster.Locate(primary, new StateMachineId(Workflow, Guid.NewGuid().ToString("N")));
        Assert.AreEqual(EventStatus.Initialized, (await machine.SendAsync(new Initialize())).Status);
        Assert.AreEqual(EventStatus.Consumed, (await machine.SendAsync(new Next())).Status);
        var (victimName, _) = Trace.Of(machine.Id).Single();
        var (victim, survivorName) = victimName == "a" ? (a, "b") : (b, "a");

        await cluster.KillHostAsync(victim);

        var result = await SendWithRetryAsync(() => machine.SendAsync(new Next()));
        Assert.AreEqual(EventStatus.Consumed, result.Status);
        Assert.AreEqual("v1-end", await StateOfAsync(machine));
        CollectionAssert.AreEqual(
            new[] { (victimName, 1), (survivorName, 1) },
            Trace.Of(machine.Id).ToArray(),
            "The instance must continue on the survivor from its committed state, exactly once per event.");
        machine.Dispose();
    }

    [TestMethod]
    [Timeout(180_000)]
    public async Task DiscoveryConvergesAcrossJoinsAbruptDeparturesRejoinsAndGracefulDepartures()
    {
        await using var cluster = new PlacementCluster();
        var primary = await cluster.StartHostAsync("primary", _ => { });
        var x = await cluster.StartHostAsync("x", Machine("x-machine", "x"));
        var y = await cluster.StartHostAsync("y", Machine("y-machine", "y"));

        await WaitUntilAsync(
            () => new[] { primary, x, y }.All(host =>
                cluster.CanLocate(host, "x-machine") && cluster.CanLocate(host, "y-machine") && cluster.KnownSilos(host) == 3),
            "Joins must converge on every silo.");

        await cluster.KillHostAsync(x);
        await WaitUntilAsync(
            () => new[] { primary, y }.All(host =>
                !cluster.CanLocate(host, "x-machine") && cluster.CanLocate(host, "y-machine") && cluster.KnownSilos(host) == 2),
            "An abrupt departure must converge on every survivor.");

        var x2 = await cluster.StartHostAsync("x2", Machine("x-machine", "x2"));
        await WaitUntilAsync(
            () => new[] { primary, y, x2 }.All(host =>
                cluster.CanLocate(host, "x-machine") && cluster.CanLocate(host, "y-machine") && cluster.KnownSilos(host) == 3),
            "A rejoin must converge on every silo.");

        using (var fromY = cluster.Locate(y, new StateMachineId("x-machine", Guid.NewGuid().ToString("N"))))
        {
            Assert.AreEqual(EventStatus.Initialized, (await SendWithRetryAsync(() => fromY.SendAsync(new Initialize()))).Status);
            Assert.AreEqual(EventStatus.Consumed, (await fromY.SendAsync(new Next())).Status);
            Assert.AreEqual(("x2", 1), Trace.Of(fromY.Id).Single());
        }

        await cluster.StopHostAsync(y);
        await WaitUntilAsync(
            () => new[] { primary, x2 }.All(host =>
                cluster.CanLocate(host, "x-machine") && !cluster.CanLocate(host, "y-machine") && cluster.KnownSilos(host) == 2),
            "A graceful departure must converge on every survivor.");

        // Scoped locators must not subscribe to the singleton discovery provider (they would leak with every scope).
        for (var i = 0; i < 100; i++)
        {
            using var scope = primary.Services.CreateScope();
            scope.ServiceProvider.GetRequiredService<IStateMachineLocator>().TryLocateStateMachine(new StateMachineId("x-machine", "probe"), out _);
        }

        Assert.AreEqual(0, ChangeHandlerCount(primary.Services.GetRequiredService<ClusterBehaviorProvider>()));
    }

    [TestMethod]
    [Timeout(180_000)]
    public async Task FailedAndHangingCapabilityPullsAreRetriedWithoutMembershipChange()
    {
        var gate = new CapabilitiesGate();
        await using var cluster = new PlacementCluster();
        var primary = await cluster.StartHostAsync("primary", _ => { }, services => services
            .AddSingleton<ISiloCapabilitiesClient>(provider => new GatedCapabilitiesClient(
                new SiloCapabilitiesClient(provider.GetRequiredService<IGrainFactory>()), gate))
            .Configure<BehaviorDiscoveryOptions>(options => options.FetchTimeout = TimeSpan.FromSeconds(1)));

        var discovery = primary.Services.GetRequiredService<ClusterBehaviorProvider>();
        discovery.BehaviorClassesChanged += _ => throw new InvalidOperationException("Expected synchronous handler failure.");
        discovery.BehaviorClassesChanged += async _ =>
        {
            await Task.Yield();
            throw new InvalidOperationException("Expected asynchronous handler failure.");
        };

        var ports = GetFreeTcpPorts(6);
        gate.Fail(ports[0]);
        gate.Hang(ports[2]);
        var failing = await cluster.StartHostAsync("failing", Machine("failing-machine", "failing"), siloPort: ports[0], gatewayPort: ports[1]);
        await cluster.StartHostAsync("hanging", Machine("hanging-machine", "hanging"), siloPort: ports[2], gatewayPort: ports[3]);
        var healthy = await cluster.StartHostAsync("healthy", Machine("healthy-machine", "healthy"), siloPort: ports[4], gatewayPort: ports[5]);

        await WaitUntilAsync(() => cluster.CanLocate(primary, "healthy-machine"), "A healthy silo must be discovered while others fail or hang.");
        await WaitUntilAsync(() => gate.Attempts(ports[0]) >= 3 && gate.Attempts(ports[2]) >= 3, "Failed and hanging pulls must be retried.");
        Assert.IsFalse(cluster.CanLocate(primary, "failing-machine"));
        Assert.IsFalse(cluster.CanLocate(primary, "hanging-machine"));

        var membership = primary.Services.GetRequiredService<IClusterMembershipService>();
        var versionBeforeRecovery = membership.CurrentSnapshot.Version;
        gate.Open();

        await WaitUntilAsync(
            () => cluster.CanLocate(primary, "failing-machine") && cluster.CanLocate(primary, "hanging-machine"),
            "Recovered silos must be discovered.");
        Assert.AreEqual(versionBeforeRecovery, membership.CurrentSnapshot.Version, "Recovery must not depend on a membership change.");

        using (var machine = cluster.Locate(primary, new StateMachineId("failing-machine", Guid.NewGuid().ToString("N"))))
        {
            Assert.AreEqual(EventStatus.Consumed, (await machine.SendAsync(new Next())).Status);
            Assert.AreEqual(("failing", 1), Trace.Of(machine.Id).Single());
        }

        // The refresh loop survived the throwing handlers and keeps tracking departures.
        await cluster.KillHostAsync(healthy);
        await WaitUntilAsync(() => !cluster.CanLocate(primary, "healthy-machine"), "Departures must still be tracked.");
        Assert.IsTrue(cluster.CanLocate(primary, "failing-machine"));
        _ = failing;
    }

    [TestMethod]
    [Timeout(180_000)]
    public async Task RollingUpgradePlacesOnSilosHostingAllVersionsAndKeepsPersistedVersions()
    {
        await using var cluster = new PlacementCluster();
        var primary = await cluster.StartHostAsync("primary", _ => { });
        var old = await cluster.StartHostAsync("old", WorkflowVersions("old", 1));
        await WaitUntilAsync(() => cluster.CanLocate(primary, Workflow), "The workflow class must be discovered.");

        var legacy = cluster.Locate(primary, new StateMachineId(Workflow, Guid.NewGuid().ToString("N")));
        Assert.AreEqual(EventStatus.Initialized, (await legacy.SendAsync(new Initialize())).Status);
        Assert.AreEqual(1, cluster.Storage.GetContext(legacy.Id)!.Version);

        var upgraded = await cluster.StartHostAsync("upgraded", WorkflowVersions("upgraded", 1, 2));
        await WaitUntilAsync(() => cluster.KnowsVersions(primary, upgraded, Workflow), "The upgraded silo's versions must be discovered.");

        for (var i = 0; i < 10; i++)
        {
            using var machine = cluster.Locate(primary, new StateMachineId(Workflow, Guid.NewGuid().ToString("N")));
            Assert.AreEqual(EventStatus.Initialized, (await machine.SendAsync(new Initialize())).Status);
            Assert.AreEqual(EventStatus.Consumed, (await machine.SendAsync(new Next())).Status);
            Assert.AreEqual(2, cluster.Storage.GetContext(machine.Id)!.Version, "New instances must use the newest cluster version.");
            Assert.AreEqual(("upgraded", 2), Trace.Of(machine.Id).Single(), "New instances must be placed on silos hosting every version.");
        }

        // Existing activations keep running where they are.
        Assert.AreEqual(EventStatus.Consumed, (await legacy.SendAsync(new Next())).Status);
        Assert.AreEqual(("old", 1), Trace.Of(legacy.Id).Last());

        await cluster.StopHostAsync(old);

        Assert.AreEqual(EventStatus.Consumed, (await SendWithRetryAsync(() => legacy.SendAsync(new Next()))).Status);
        Assert.AreEqual("v1-end", await StateOfAsync(legacy));
        Assert.AreEqual(("upgraded", 1), Trace.Of(legacy.Id).Last(), "A persisted v1 instance must keep running its v1 definition.");
        Assert.AreEqual(1, cluster.Storage.GetContext(legacy.Id)!.Version);
        legacy.Dispose();
    }

    [TestMethod]
    [Timeout(180_000)]
    public async Task IncompatibleDeploymentFallsBackToNewestVersionAndNeverRunsMissingPersistedVersion()
    {
        await using var cluster = new PlacementCluster();
        var primary = await cluster.StartHostAsync("primary", _ => { });
        var old = await cluster.StartHostAsync("old", WorkflowVersions("old", 1));
        await WaitUntilAsync(() => cluster.CanLocate(primary, Workflow), "The workflow class must be discovered.");

        var legacy = cluster.Locate(primary, new StateMachineId(Workflow, Guid.NewGuid().ToString("N")));
        Assert.AreEqual(EventStatus.Initialized, (await legacy.SendAsync(new Initialize())).Status);

        var next = await cluster.StartHostAsync("next", WorkflowVersions("next", 2));
        await WaitUntilAsync(() => cluster.KnowsVersions(primary, next, Workflow), "The v2-only silo's versions must be discovered.");

        for (var i = 0; i < 5; i++)
        {
            using var machine = cluster.Locate(primary, new StateMachineId(Workflow, Guid.NewGuid().ToString("N")));
            Assert.AreEqual(EventStatus.Initialized, (await machine.SendAsync(new Initialize())).Status);
            Assert.AreEqual(EventStatus.Consumed, (await machine.SendAsync(new Next())).Status);
            Assert.AreEqual(("next", 2), Trace.Of(machine.Id).Single(), "Without a superset silo, the newest version must be used.");
        }

        Assert.IsTrue(
            cluster.Logs.Any(log => log.Contains("Incompatible Stateflows deployment", StringComparison.Ordinal)),
            "The incompatible deployment must be reported.");

        await cluster.StopHostAsync(old);

        var rejection = await WaitForFailureAsync(() => legacy.SendAsync(new Next()), "does not host version 1");
        StringAssert.Contains(Messages(rejection), Workflow);
        var rejected = cluster.Storage.GetContext(legacy.Id)!;
        Assert.AreEqual(1, rejected.Version, "A rejected activation must not touch persisted state.");
        Assert.AreEqual(0, Trace.Of(legacy.Id).Count, "The persisted v1 instance must never run under v2.");

        // A v1-only silo does not help: v2-only and v1-only silos are not superset hosts, so the newest version wins.
        await cluster.StartHostAsync("old-again", WorkflowVersions("old-again", 1));
        await WaitUntilAsync(() => cluster.KnownSilos(primary) == 3, "The v1-only silo must be discovered.");
        StringAssert.Contains(Messages(await CaptureFailureAsync(() => legacy.SendAsync(new Next()))), "does not host version 1");

        var fixedHost = await cluster.StartHostAsync("fixed", WorkflowVersions("fixed", 1, 2));
        await WaitUntilAsync(() => cluster.KnowsVersions(primary, fixedHost, Workflow), "The fixed silo's versions must be discovered.");

        Assert.AreEqual(EventStatus.Consumed, (await SendWithRetryAsync(() => legacy.SendAsync(new Next()))).Status);
        Assert.AreEqual("v1-middle", await StateOfAsync(legacy));
        Assert.AreEqual(("fixed", 1), Trace.Of(legacy.Id).Single());
        legacy.Dispose();
    }

    [TestMethod]
    [Timeout(180_000)]
    public async Task ResetInstanceIsReinitializedOnNewestVersionWhileLiveInstanceKeepsItsVersion()
    {
        await using var cluster = new PlacementCluster();
        var primary = await cluster.StartHostAsync("primary", _ => { });
        var old = await cluster.StartHostAsync("old", WorkflowVersions("old", 1));
        await WaitUntilAsync(() => cluster.CanLocate(primary, Workflow), "The workflow class must be discovered.");

        using var machine = cluster.Locate(primary, new StateMachineId(Workflow, Guid.NewGuid().ToString("N")));
        Assert.AreEqual(EventStatus.Initialized, (await machine.SendAsync(new Initialize())).Status);
        Assert.AreEqual("v1-start", await StateOfAsync(machine));
        Assert.AreEqual(1, cluster.Storage.GetContext(machine.Id)!.Version);

        // Version 2 is registered by redeploying: the v1-only silo is replaced by one hosting both versions.
        await cluster.StopHostAsync(old);
        var upgraded = await cluster.StartHostAsync("upgraded", WorkflowVersions("upgraded", 1, 2));
        await WaitUntilAsync(() => cluster.KnowsVersions(primary, upgraded, Workflow), "The upgraded silo's versions must be discovered.");

        Assert.AreEqual(EventStatus.Consumed, (await SendWithRetryAsync(() => machine.SendAsync(new Next()))).Status);
        Assert.AreEqual("v1-middle", await StateOfAsync(machine), "The initialized instance must keep running version 1.");
        Assert.AreEqual(("upgraded", 1), Trace.Of(machine.Id).Last());
        Assert.AreEqual(1, cluster.Storage.GetContext(machine.Id)!.Version);

        Assert.AreEqual(EventStatus.Consumed, (await machine.SendAsync(new Reset())).Status);

        Assert.AreEqual(EventStatus.Initialized, (await machine.SendAsync(new Initialize())).Status);
        Assert.AreEqual("v2-start", await StateOfAsync(machine), "A reinitialized instance must use the newest version.");
        Assert.AreEqual(EventStatus.Consumed, (await machine.SendAsync(new Next())).Status);
        Assert.AreEqual("v2-middle", await StateOfAsync(machine));
        Assert.AreEqual(("upgraded", 2), Trace.Of(machine.Id).Last());
        Assert.AreEqual(2, cluster.Storage.GetContext(machine.Id)!.Version);

        // The new version is persisted, not just held by the activation.
        await primary.Services.GetRequiredService<IGrainFactory>().GetGrain<IManagementGrain>(0).ForceActivationCollection(TimeSpan.Zero);
        Assert.AreEqual(EventStatus.Consumed, (await SendWithRetryAsync(() => machine.SendAsync(new Next()))).Status);
        Assert.AreEqual("v2-end", await StateOfAsync(machine));
        Assert.AreEqual(("upgraded", 2), Trace.Of(machine.Id).Last());
    }

    private static Action<IStateflowsBuilder> WorkflowVersions(string silo, params int[] versions) => stateflows => stateflows
        .AddStateMachines(machines =>
        {
            foreach (var version in versions)
            {
                machines.AddStateMachine(Workflow, version, machine => machine
                    .AddInitialState($"v{version}-start", state => state
                        .AddTransition<Next>($"v{version}-middle", transition => transition.AddEffect(c => Trace.Record(c.Behavior.Id, silo, version))))
                    .AddState($"v{version}-middle", state => state
                        .AddTransition<Next>($"v{version}-end", transition => transition.AddEffect(c => Trace.Record(c.Behavior.Id, silo, version))))
                    .AddState($"v{version}-end"));
            }
        });

    private static Action<IStateflowsBuilder> Machine(string name, string silo) => stateflows => stateflows
        .AddStateMachines(machines => machines.AddStateMachine(name, machine => machine
            .AddInitialState("ready", state => state
                .AddTransition<Next>("done", transition => transition.AddEffect(c => Trace.Record(c.Behavior.Id, silo, 1))))
            .AddState("done")));

    private static async Task<string?> StateOfAsync(IStateMachineBehavior machine)
        => (await machine.GetStatusAsync()).Response.CurrentStates.Value;

    private static async Task<SendResult> SendWithRetryAsync(Func<Task<SendResult>> send)
    {
        var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(60);
        while (true)
        {
            try
            {
                return await send();
            }
            catch (Exception) when (DateTime.UtcNow < deadline)
            {
                await Task.Delay(250);
            }
        }
    }

    private static async Task<Exception> CaptureFailureAsync(Func<Task> send)
    {
        try
        {
            await send().WaitAsync(TimeSpan.FromSeconds(30));
        }
        catch (Exception exception)
        {
            return exception;
        }

        Assert.Fail("The send was expected to fail.");
        return null!;
    }

    private static async Task<Exception> WaitForFailureAsync(Func<Task> send, string expectedMessage)
    {
        var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(60);
        while (true)
        {
            var failure = await CaptureFailureAsync(send);
            if (Messages(failure).Contains(expectedMessage, StringComparison.Ordinal))
            {
                return failure;
            }

            if (DateTime.UtcNow > deadline)
            {
                Assert.Fail($"Expected failure containing '{expectedMessage}', last failure: {failure}");
            }

            await Task.Delay(250);
        }
    }

    private static async Task WaitUntilAsync(Func<bool> condition, string message)
    {
        var timeout = Stopwatch.StartNew();
        while (!condition())
        {
            if (timeout.Elapsed > ConvergenceTimeout) Assert.Fail($"{message} (not within {ConvergenceTimeout})");
            await Task.Delay(100);
        }
    }

    private static string Messages(Exception exception)
    {
        var messages = new List<string>();
        for (Exception? current = exception; current is not null; current = current.InnerException)
        {
            messages.Add(current.Message);
        }

        return string.Join(Environment.NewLine, messages);
    }

    private static int ChangeHandlerCount(ClusterBehaviorProvider provider)
        => (typeof(ClusterBehaviorProvider)
                .GetField(nameof(ClusterBehaviorProvider.BehaviorClassesChanged), BindingFlags.Instance | BindingFlags.NonPublic)!
                .GetValue(provider) as Delegate)?.GetInvocationList().Length ?? 0;

    private static int[] GetFreeTcpPorts(int count)
    {
        var listeners = Enumerable.Range(0, count).Select(_ => new TcpListener(IPAddress.Loopback, 0)).ToArray();
        try
        {
            foreach (var listener in listeners) listener.Start();
            return listeners.Select(l => ((IPEndPoint)l.LocalEndpoint).Port).ToArray();
        }
        finally
        {
            foreach (var listener in listeners) listener.Stop();
        }
    }

    private sealed class PlacementCluster : IAsyncDisposable
    {
        private readonly List<IHost> hosts = [];
        private readonly Dictionary<IHost, IServiceScope> scopes = [];
        private readonly string clusterId = $"placement-{Guid.NewGuid():N}";
        private readonly string serviceId = $"placement-{Guid.NewGuid():N}";
        private readonly LogSink logSink = new();
        private IPEndPoint? primary;

        public BehaviorSchedulingTests.SharedGrainStorage Storage { get; } = new();
        public BehaviorSchedulingTests.SharedReminderTable Reminders { get; } = new();
        public IReadOnlyCollection<string> Logs => logSink.Messages;

        public async Task<IHost> StartHostAsync(
            string name,
            Action<IStateflowsBuilder> build,
            Action<IServiceCollection>? configure = null,
            int? siloPort = null,
            int? gatewayPort = null)
        {
            var ports = GetFreeTcpPorts(2);
            var silo = siloPort ?? ports[0];
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((_, services) =>
                {
                    services.AddStateflows(build);
                })
                .ConfigureLogging(logging => logging
                    .ClearProviders()
                    .AddProvider(logSink)
                    .SetMinimumLevel(LogLevel.Warning))
                .UseOrleans(siloBuilder => siloBuilder
                    .UseLocalhostClustering(silo, gatewayPort ?? ports[1], primary, serviceId, clusterId)
                    .Configure<ClusterMembershipOptions>(options =>
                    {
                        options.ProbeTimeout = TimeSpan.FromSeconds(1);
                        options.NumMissedProbesLimit = 1;
                        options.NumVotesForDeathDeclaration = 1;
                    })
                    .AddReminders()
                    .ConfigureServices(services => services
                        // Random placement makes eligibility violations observable (resource-optimized placement may hide them).
                        .AddSingleton<PlacementStrategy, RandomPlacement>()
                        .AddSingleton<IReminderTable>(Reminders)
                        .AddKeyedSingleton<IGrainStorage>("Default",
                            (provider, _) => Storage.Attach(provider.GetRequiredService<IGrainStorageSerializer>())))
                    .AddStateflowsRuntime()
                    .ConfigureServices(services => configure?.Invoke(services)))
                .Build();
            hosts.Add(host);
            await host.StartAsync();
            primary ??= new IPEndPoint(IPAddress.Loopback, silo);
            return host;
        }

        public bool CanLocate(IHost host, string stateMachineName)
            => Services(host).GetRequiredService<IStateMachineLocator>()
                .TryLocateStateMachine(new StateMachineId(stateMachineName, "probe"), out _);

        public IStateMachineBehavior Locate(IHost host, StateMachineId id)
        {
            Assert.IsTrue(Services(host).GetRequiredService<IStateMachineLocator>().TryLocateStateMachine(id, out var machine));
            return machine;
        }

        public int KnownSilos(IHost host)
            => host.Services.GetRequiredService<ClusterBehaviorProvider>().KnownSilos.Count;

        public bool KnowsVersions(IHost observer, IHost target, string stateMachineName)
            => observer.Services.GetRequiredService<ClusterBehaviorProvider>().TryGetVersions(
                target.Services.GetRequiredService<ILocalSiloDetails>().SiloAddress,
                new BehaviorClass(BehaviorType.StateMachine, stateMachineName),
                out _);

        private IServiceProvider Services(IHost host)
        {
            if (!scopes.TryGetValue(host, out var scope))
            {
                scope = host.Services.CreateScope();
                scopes[host] = scope;
            }

            return scope.ServiceProvider;
        }

        private void Forget(IHost host)
        {
            hosts.Remove(host);
            if (scopes.Remove(host, out var scope)) scope.Dispose();
        }

        public async Task StopHostAsync(IHost host)
        {
            Forget(host);
            try
            {
                await host.StopAsync(TimeSpan.FromSeconds(10));
            }
            finally
            {
                host.Dispose();
            }
        }

        // A cancelled token makes Orleans stop without graceful deactivation, like a crashed process.
        public async Task KillHostAsync(IHost host)
        {
            Forget(host);
            try
            {
                await host.StopAsync(new CancellationToken(true));
            }
            catch (OperationCanceledException)
            {
            }
        }

        public async ValueTask DisposeAsync()
        {
            foreach (var host in hosts.AsEnumerable().Reverse().ToArray())
            {
                await StopHostAsync(host);
            }
        }
    }

    private sealed class CapabilitiesGate
    {
        private readonly ConcurrentDictionary<int, string> modes = new();
        private readonly ConcurrentDictionary<int, int> attempts = new();

        public void Fail(int siloPort) => modes[siloPort] = "fail";
        public void Hang(int siloPort) => modes[siloPort] = "hang";
        public void Open() => modes.Clear();
        public int Attempts(int siloPort) => attempts.GetValueOrDefault(siloPort);

        public async Task PassAsync(SiloAddress silo, CancellationToken cancellationToken)
        {
            var port = silo.Endpoint.Port;
            if (!modes.TryGetValue(port, out var mode)) return;
            attempts.AddOrUpdate(port, 1, (_, count) => count + 1);
            if (mode == "fail") throw new InvalidOperationException($"Expected capability pull failure for {silo}.");
            await Task.Delay(Timeout.Infinite, cancellationToken);
        }
    }

    private sealed class GatedCapabilitiesClient(ISiloCapabilitiesClient inner, CapabilitiesGate gate) : ISiloCapabilitiesClient
    {
        public async Task<SiloCapabilities> GetCapabilitiesAsync(SiloAddress silo, CancellationToken cancellationToken)
        {
            await gate.PassAsync(silo, cancellationToken);
            return await inner.GetCapabilitiesAsync(silo, cancellationToken);
        }
    }

    private sealed class LogSink : ILoggerProvider
    {
        private readonly ConcurrentQueue<string> messages = new();
        public IReadOnlyCollection<string> Messages => messages.ToArray();
        public ILogger CreateLogger(string categoryName) => new Logger(this);
        public void Dispose() { }

        private sealed class Logger(LogSink sink) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
            public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Warning;

            public void Log<TState>(LogLevel logLevel, Microsoft.Extensions.Logging.EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                if (IsEnabled(logLevel)) sink.messages.Enqueue(formatter(state, exception));
            }
        }
    }

    private static class Trace
    {
        private static ConcurrentDictionary<BehaviorId, ConcurrentQueue<(string Silo, int Version)>> records = new();

        public static void Reset() => records = new();

        public static Task Record(BehaviorId id, string silo, int version)
        {
            records.GetOrAdd(id, _ => new()).Enqueue((silo, version));
            return Task.CompletedTask;
        }

        public static IReadOnlyList<(string Silo, int Version)> Of(BehaviorId id)
            => records.TryGetValue(id, out var queue) ? queue.ToArray() : [];
    }

    public sealed class Next;
}
