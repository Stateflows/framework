using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Hosting;
using Stateflows;
using Stateflows.Actions;
using Stateflows.Activities;
using Stateflows.Common;
using Stateflows.Common.Interfaces;
using Stateflows.StateMachines;

namespace Orleans.IntegrationTests;

[TestClass]
public sealed class TwoSiloBehaviorDiscoveryTests
{
    private static readonly TimeSpan DiscoveryTimeout = TimeSpan.FromSeconds(15);

    [TestMethod]
    public async Task RemoteOnlyBehaviorClassesCanBeLocatedAndExecutedAcrossTwoSilos()
    {
        var executions = new ConcurrentDictionary<string, int>();
        var clusterId = $"orleans-integration-{Guid.NewGuid():N}";
        var serviceId = $"stateflows-integration-{Guid.NewGuid():N}";
        var ports = GetFreeTcpPorts(4);
        var siloPortA = ports[0];
        var siloA = CreateSilo("silo-a", siloPortA, ports[1], null, clusterId, serviceId, executions);
        var siloB = CreateSilo(
            "silo-b",
            ports[2],
            ports[3],
            new IPEndPoint(IPAddress.Loopback, siloPortA),
            clusterId,
            serviceId,
            executions);

        try
        {
            await siloA.StartAsync();
            await siloB.StartAsync();

            using var scopeA = siloA.Services.CreateScope();
            using var scopeB = siloB.Services.CreateScope();
            var stateMachinesA = scopeA.ServiceProvider.GetRequiredService<IStateMachineLocator>();
            var activitiesA = scopeA.ServiceProvider.GetRequiredService<IActivityLocator>();
            var actionsA = scopeA.ServiceProvider.GetRequiredService<IActionLocator>();
            var stateMachinesB = scopeB.ServiceProvider.GetRequiredService<IStateMachineLocator>();
            var activitiesB = scopeB.ServiceProvider.GetRequiredService<IActivityLocator>();
            var actionsB = scopeB.ServiceProvider.GetRequiredService<IActionLocator>();

            await WaitForDiscoveryAsync(
                () => actionsA.TryLocateAction(new ActionId("silo-b-action", "remote"), out _)
                    && activitiesA.TryLocateActivity(new ActivityId("silo-b-activity", "remote"), out _)
                    && stateMachinesA.TryLocateStateMachine(new StateMachineId("silo-b-machine", "remote"), out _)
                    && actionsB.TryLocateAction(new ActionId("silo-a-action", "remote"), out _)
                    && activitiesB.TryLocateActivity(new ActivityId("silo-a-activity", "remote"), out _)
                    && stateMachinesB.TryLocateStateMachine(new StateMachineId("silo-a-machine", "remote"), out _));

            var executionsBeforeUnhostedBehavior = executions.ToArray();
            using (var unhostedBehavior = siloA.Services.GetRequiredService<IBehaviorFactory>()
                .CreateBehavior(new ActionId("unhosted-action", "remote")))
            {
                Exception? placementFailure = null;
                try
                {
                    await unhostedBehavior.SendAsync(new Initialize()).WaitAsync(TimeSpan.FromSeconds(10));
                }
                catch (Exception exception)
                {
                    placementFailure = exception;
                }

                Assert.IsNotNull(placementFailure, "Sending to an unhosted behavior should fail placement.");
                Assert.IsTrue(
                    GetExceptionMessages(placementFailure).Contains("No eligible silo hosts Stateflows behavior", StringComparison.Ordinal),
                    $"Unexpected placement failure: {placementFailure}");
                CollectionAssert.AreEquivalent(executionsBeforeUnhostedBehavior, executions.ToArray());
            }

            Assert.IsTrue(actionsA.TryLocateAction(new ActionId("silo-b-action", "remote"), out var remoteAction));
            await remoteAction.ExecuteAsync();

            Assert.IsTrue(activitiesA.TryLocateActivity(new ActivityId("silo-b-activity", "remote"), out var remoteActivity));
            Assert.AreEqual(EventStatus.Initialized, (await remoteActivity.SendAsync(new Initialize())).Status);

            Assert.IsTrue(stateMachinesA.TryLocateStateMachine(new StateMachineId("silo-b-machine", "remote"), out var remoteStateMachine));
            Assert.AreEqual(EventStatus.Initialized, (await remoteStateMachine.SendAsync(new Initialize())).Status);
            Assert.AreEqual("ready", (await remoteStateMachine.GetStatusAsync()).Response.CurrentStates.Value);
            Assert.AreEqual(EventStatus.Consumed, (await remoteStateMachine.SendAsync(new RemoteSiloEvent())).Status);
            Assert.AreEqual("complete", (await remoteStateMachine.GetStatusAsync()).Response.CurrentStates.Value);

            Assert.AreEqual(1, executions.GetValueOrDefault("silo-b-action"));
            Assert.AreEqual(1, executions.GetValueOrDefault("silo-b-activity"));
            Assert.AreEqual(1, executions.GetValueOrDefault("silo-b-machine"));

            Assert.IsTrue(actionsB.TryLocateAction(new ActionId("silo-a-action", "remote"), out var remoteActionOnA));
            await remoteActionOnA.ExecuteAsync();
            Assert.IsTrue(activitiesB.TryLocateActivity(new ActivityId("silo-a-activity", "remote"), out var remoteActivityOnA));
            Assert.AreEqual(EventStatus.Initialized, (await remoteActivityOnA.SendAsync(new Initialize())).Status);
            Assert.IsTrue(stateMachinesB.TryLocateStateMachine(new StateMachineId("silo-a-machine", "remote"), out var remoteStateMachineOnA));
            Assert.AreEqual(EventStatus.Initialized, (await remoteStateMachineOnA.SendAsync(new Initialize())).Status);

            Assert.AreEqual(1, executions.GetValueOrDefault("silo-a-action"));
            Assert.AreEqual(1, executions.GetValueOrDefault("silo-a-activity"));
            Assert.AreEqual(EventStatus.Consumed, (await remoteStateMachineOnA.SendAsync(new RemoteSiloEvent())).Status);
            Assert.AreEqual(1, executions.GetValueOrDefault("silo-a-machine"));

            await siloB.StopAsync();
            await WaitForDiscoveryAsync(
                () => !actionsA.TryLocateAction(new ActionId("silo-b-action", "remote"), out _)
                    && !activitiesA.TryLocateActivity(new ActivityId("silo-b-activity", "remote"), out _)
                    && !stateMachinesA.TryLocateStateMachine(new StateMachineId("silo-b-machine", "remote"), out _));
        }
        finally
        {
            try
            {
                await DisposeHostAsync(siloA);
            }
            finally
            {
                await DisposeHostAsync(siloB);
            }
        }
    }

    private static IHost CreateSilo(
        string siloName,
        int siloPort,
        int gatewayPort,
        IPEndPoint? primarySiloEndpoint,
        string clusterId,
        string serviceId,
        ConcurrentDictionary<string, int> executions)
    {
        return Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) => services
                .AddStateflows(stateflows => stateflows
                    .AddActions(actions => actions.AddAction(
                        $"{siloName}-action",
                        _ =>
                        {
                            executions.AddOrUpdate($"{siloName}-action", 1, (_, count) => count + 1);
                            return Task.CompletedTask;
                        }))
                    .AddActivities(activities => activities.AddActivity(
                        $"{siloName}-activity",
                        activity => activity
                            .AddInitial(initial => initial.AddControlFlow("mark-executed"))
                            .AddAction(
                                "mark-executed",
                                _ =>
                                {
                                    executions.AddOrUpdate($"{siloName}-activity", 1, (_, count) => count + 1);
                                    return Task.CompletedTask;
                                },
                                action => action.AddControlFlow<FinalNode>())
                            .AddFinal()))
                    .AddStateMachines(machines => machines.AddStateMachine(
                        $"{siloName}-machine",
                        machine => machine
                            .AddInitialState("ready", state => state.AddTransition<RemoteSiloEvent>(
                                "complete",
                                transition => transition.AddEffect(_ =>
                                {
                                    executions.AddOrUpdate($"{siloName}-machine", 1, (_, count) => count + 1);
                                    return Task.CompletedTask;
                                })))
                            .AddState("complete")))
                )
            )
            .ConfigureLogging(logging => logging.SetMinimumLevel(LogLevel.Warning))
            .UseOrleans(silo => silo
                .UseLocalhostClustering(siloPort, gatewayPort, primarySiloEndpoint, serviceId, clusterId)
                .AddMemoryGrainStorage("stateflows")
                .UseInMemoryReminderService()
                .AddStateflowsRuntime())
            .Build();
    }

    private static async Task WaitForDiscoveryAsync(Func<bool> isDiscovered)
    {
        var timeout = Stopwatch.StartNew();
        while (!isDiscovered())
        {
            if (timeout.Elapsed >= DiscoveryTimeout)
            {
                Assert.Fail($"Behavior metadata did not converge within {DiscoveryTimeout}.");
            }

            await Task.Delay(TimeSpan.FromMilliseconds(100));
        }
    }

    private static int[] GetFreeTcpPorts(int count)
    {
        var ports = new HashSet<int>();
        while (ports.Count < count)
        {
            using var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            ports.Add(((IPEndPoint)listener.LocalEndpoint).Port);
        }

        return ports.ToArray();
    }

    private static string GetExceptionMessages(Exception exception)
    {
        var messages = new List<string>();
        for (Exception? current = exception; current is not null; current = current.InnerException)
        {
            messages.Add(current.Message);
        }

        return string.Join(Environment.NewLine, messages);
    }

    private static async Task DisposeHostAsync(IHost host)
    {
        try
        {
            await host.StopAsync(TimeSpan.FromSeconds(10));
        }
        finally
        {
            host.Dispose();
        }
    }

    public sealed class RemoteSiloEvent
    {
    }
}
