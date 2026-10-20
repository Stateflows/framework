using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Hosting;
using Stateflows;
using Stateflows.Common;
using Stateflows.StateMachines;

namespace Orleans.IntegrationTests;

[TestClass]
public sealed class BehaviorStartupTests
{
    [TestMethod]
    public async Task DefaultInstanceIsInitializedWithoutDispatchingStartup()
    {
        var ports = GetFreeTcpPorts(2);
        using var host = CreateSilo(ports[0], ports[1], null, Guid.NewGuid().ToString("N"));
        try
        {
            await host.StartAsync();
            using var scope = host.Services.CreateScope();
            var locator = scope.ServiceProvider.GetRequiredService<IStateMachineLocator>();
            Assert.IsTrue(locator.TryLocateStateMachine(new StateMachineId("default-machine", string.Empty), out var machine));
            using (machine)
            {
                Assert.AreEqual("waiting", (await machine.GetStatusAsync()).Response.CurrentStates.Value);
                Assert.AreEqual(EventStatus.Consumed, (await machine.SendAsync(new Startup())).Status);
                Assert.AreEqual("running", (await machine.GetStatusAsync()).Response.CurrentStates.Value);
            }
        }
        finally
        {
            await host.StopAsync();
        }
    }

    [TestMethod]
    public async Task JoiningSiloDoesNotDispatchStartupToWaitingBehaviors()
    {
        var ports = GetFreeTcpPorts(4);
        var clusterId = Guid.NewGuid().ToString("N");
        using var first = CreateSilo(ports[0], ports[1], null, clusterId);
        using var second = CreateSilo(ports[2], ports[3], new IPEndPoint(IPAddress.Loopback, ports[0]), clusterId);
        try
        {
            await first.StartAsync();
            using var scope = first.Services.CreateScope();
            var locator = scope.ServiceProvider.GetRequiredService<IStateMachineLocator>();
            Assert.IsTrue(locator.TryLocateStateMachine(new StateMachineId("default-machine", "instance"), out var machine));
            using (machine)
            {
                Assert.AreEqual(EventStatus.Initialized, (await machine.SendAsync(new Initialize())).Status);
                await second.StartAsync();
                Assert.AreEqual("waiting", (await machine.GetStatusAsync()).Response.CurrentStates.Value);
                Assert.IsTrue(locator.TryLocateStateMachine(new StateMachineId("default-machine", string.Empty), out var defaultMachine));
                using (defaultMachine)
                {
                    Assert.AreEqual("waiting", (await defaultMachine.GetStatusAsync()).Response.CurrentStates.Value);
                }
            }
        }
        finally
        {
            await second.StopAsync();
            await first.StopAsync();
        }
    }

    private static IHost CreateSilo(int siloPort, int gatewayPort, IPEndPoint? primary, string clusterId)
        => Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) => { 
                services.AddStateflows(stateflows => stateflows
                    .AddStateMachines(machines => machines.AddStateMachine(
                        "default-machine",
                        machine => machine
                            .AddInitialState("waiting", state => state.AddTransition<Startup>("running"))
                            .AddState("running")))
                    .AddDefaultInstance(new StateMachineClass("default-machine")));
            })
            .ConfigureLogging(logging => logging.SetMinimumLevel(LogLevel.Warning))
            .UseOrleans(silo => silo
                .UseLocalhostClustering(siloPort, gatewayPort, primary, clusterId, clusterId)
                .AddMemoryGrainStorageAsDefault()
                .UseInMemoryReminderService()
                .AddStateflowsRuntime())
            .Build();

    private static int[] GetFreeTcpPorts(int count)
    {
        var listeners = new TcpListener[count];
        var ports = new int[count];
        try
        {
            for (var i = 0; i < count; i++)
            {
                listeners[i] = new TcpListener(IPAddress.Loopback, 0);
                listeners[i].Start();
                ports[i] = ((IPEndPoint)listeners[i].LocalEndpoint).Port;
            }
        }
        finally
        {
            foreach (var listener in listeners)
            {
                listener?.Stop();
            }
        }
        return ports;
    }
}
