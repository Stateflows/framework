using System.Net;
using System.Net.Sockets;
using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Hosting;
using Orleans.Runtime;
using Stateflows;
using Stateflows.Common;
using Stateflows.StateMachines;

namespace Orleans.IntegrationTests;

[TestClass]
public sealed class BehaviorContextPersistenceTests
{
    [TestMethod]
    public async Task ContextIsRestoredFromOrleansStorageAfterReactivation()
    {
        var payloads = Channel.CreateUnbounded<UserPayload?>();
        var ports = GetFreeTcpPorts(2);
        var clusterId = Guid.NewGuid().ToString("N");
        using var host = Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
            {
                services.AddStateflows(stateflows => stateflows
                    .AddStateMachines(machines => machines.AddStateMachine(
                        "persisted-machine",
                        machine => machine
                            .AddInitialState("idle", state => state
                                .AddTransition<StorePayload>("waiting", transition => transition
                                    .AddEffect(c => c.Behavior.Values.SetAsync("payload", c.Event.Payload))))
                            .AddState("waiting", state => state
                                .AddTransition<LongDelay>("expired")
                                .AddInternalTransition<ReadPayload>(transition => transition
                                    .AddEffect(async c =>
                                    {
                                        var (success, payload) = await c.Behavior.Values.TryGetAsync<UserPayload>("payload");
                                        payloads.Writer.TryWrite(success ? payload : null);
                                    })))
                            .AddState("expired")))
                    );
            })
            .ConfigureLogging(logging => logging.SetMinimumLevel(LogLevel.Warning))
            .UseOrleans(silo => silo
                .UseLocalhostClustering(ports[0], ports[1], null, clusterId, clusterId)
                .AddMemoryGrainStorageAsDefault()
                .UseInMemoryReminderService()
                .AddStateflowsRuntime()
            )
            .Build();
        try
        {
            await host.StartAsync();
            using var scope = host.Services.CreateScope();
            var locator = scope.ServiceProvider.GetRequiredService<IStateMachineLocator>();
            Assert.IsTrue(locator.TryLocateStateMachine(new StateMachineId("persisted-machine", "one"), out var machine));
            using (machine)
            {
                var stored = new UserPayload { Text = "persisted", Numbers = [4, 2] };
                Assert.AreEqual(EventStatus.Initialized, (await machine.SendAsync(new Initialize())).Status);
                Assert.AreEqual(EventStatus.Consumed, (await machine.SendAsync(new StorePayload { Payload = stored })).Status);

                var management = host.Services.GetRequiredService<IGrainFactory>().GetGrain<IManagementGrain>(0);
                await management.ForceActivationCollection(TimeSpan.Zero);
                var statistics = await management.GetSimpleGrainStatistics();
                Assert.IsFalse(
                    statistics.Any(s => s.GrainType.Contains(nameof(StateflowsBehavior)) && s.ActivationCount > 0),
                    "Behavior grains must be deactivated, so the context is read back from storage.");

                Assert.AreEqual("waiting", (await machine.GetStatusAsync()).Response.CurrentStates.Value);
                Assert.AreEqual(EventStatus.Consumed, (await machine.SendAsync(new ReadPayload())).Status);
                Assert.IsTrue(payloads.Reader.TryRead(out var restored));
                Assert.IsNotNull(restored);
                Assert.AreEqual(stored.Text, restored.Text);
                CollectionAssert.AreEqual(stored.Numbers, restored.Numbers);
            }
        }
        finally
        {
            await host.StopAsync();
        }
    }

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

    public sealed class UserPayload
    {
        public string Text { get; set; } = string.Empty;
        public List<int> Numbers { get; set; } = [];
    }

    public sealed class StorePayload
    {
        public UserPayload Payload { get; set; } = new();
    }

    public sealed class ReadPayload;

    public sealed class LongDelay : DelayEvent
    {
        protected override TimeSpan Delay => TimeSpan.FromHours(1);

        protected override DateTime GetTriggerTime(DateTime startedAt) => startedAt.Add(Delay);
    }
}
