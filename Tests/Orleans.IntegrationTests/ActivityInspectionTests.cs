using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Stateflows;
using Stateflows.Activities;
using Stateflows.Activities.Inspection.Interfaces;
using Stateflows.Common;
using Stateflows.Common.Interfaces;

namespace Orleans.IntegrationTests;

[TestClass]
public sealed class ActivityInspectionTests
{
    private const string ActivityName = "inspection-session";
    private static readonly Dictionary<string, EventHeader> StreamHeaders = new()
    {
        [nameof(Stateflows.Common.Stream)] = new Stateflows.Common.Stream()
    };

    [TestMethod]
    [DataRow("cold")]
    [DataRow("initialized")]
    [DataRow("transactional")]
    [DataRow("streaming")]
    [Timeout(60_000)]
    public async Task PlantUmlRequestsResolveInspectionInNewAndReusedSessions(string state)
    {
        var probe = new InspectionProbe();
        using var host = CreateHost(probe);
        await host.StartAsync();
        try
        {
            using var behavior = host.Services.GetRequiredService<IBehaviorFactory>()
                .CreateBehavior(new ActivityId(ActivityName, state));
            if (state == "initialized")
                Assert.AreEqual(EventStatus.Initialized, (await behavior.SendAsync(new Initialize())).Status);
            if (state == "transactional")
            {
                probe.Gate(state).TrySetResult();
                Assert.AreEqual(EventStatus.Consumed, (await behavior.SendAsync(new TokensInput().Add(1))).Status);
                await probe.Completed(state).Task.WaitAsync(TimeSpan.FromSeconds(10));
            }
            if (state == "streaming")
            {
                Assert.AreEqual(EventStatus.Accepted,
                    (await behavior.SendAsync(new TokensInput().Add(1), StreamHeaders)).Status);
                await probe.Started(state).Task.WaitAsync(TimeSpan.FromSeconds(10));
            }

            for (var request = 0; request < 2; request++)
            {
                var result = await behavior.GetPlantUmlAsync().WaitAsync(TimeSpan.FromSeconds(10));
                Assert.AreEqual(EventStatus.Consumed, result.Status);
                Assert.IsNotNull(result.Response);
                StringAssert.Contains(result.Response.PlantUml, "Input pin\\nInt32");
                StringAssert.Contains(result.Response.PlantUml, nameof(InspectAction));
            }
        }
        finally
        {
            probe.ReleaseAll();
            await host.StopAsync();
        }
    }

    [TestMethod]
    [Timeout(60_000)]
    public async Task ConcurrentStreamedNodesResolveTheirOwnInspectionAfterAwait()
    {
        var probe = new InspectionProbe();
        using var host = CreateHost(probe);
        await host.StartAsync();
        try
        {
            var factory = host.Services.GetRequiredService<IBehaviorFactory>();
            using var first = factory.CreateBehavior(new ActivityId(ActivityName, "first"));
            using var second = factory.CreateBehavior(new ActivityId(ActivityName, "second"));
            await first.SendAsync(new Initialize());
            await second.SendAsync(new Initialize());
            Assert.AreEqual(EventStatus.Accepted, (await first.SendAsync(new TokensInput().Add(1), StreamHeaders)).Status);
            Assert.AreEqual(EventStatus.Accepted, (await second.SendAsync(new TokensInput().Add(2), StreamHeaders)).Status);
            await Task.WhenAll(probe.Started("first").Task, probe.Started("second").Task)
                .WaitAsync(TimeSpan.FromSeconds(10));

            await Task.WhenAll(first.GetPlantUmlAsync(), second.GetPlantUmlAsync())
                .WaitAsync(TimeSpan.FromSeconds(10));
            probe.ReleaseAll();
            await Task.WhenAll(probe.Completed("first").Task, probe.Completed("second").Task)
                .WaitAsync(TimeSpan.FromSeconds(10));
        }
        finally
        {
            probe.ReleaseAll();
            await host.StopAsync();
        }
    }

    [TestMethod]
    [Timeout(60_000)]
    public async Task MixedHandoffBindsInspectionForHostedSuccessors()
    {
        var probe = new InspectionProbe();
        using var host = CreateHost(probe);
        await host.StartAsync();
        try
        {
            using var behavior = host.Services.GetRequiredService<IBehaviorFactory>()
                .CreateBehavior(new ActivityId("inspection-handoff", "mixed"));
            Assert.AreEqual(EventStatus.Accepted,
                (await behavior.SendAsync(new TokensInput().Add(1), StreamHeaders)).Status);
            await probe.Started("keepalive").Task.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.AreEqual(EventStatus.Consumed,
                (await behavior.SendAsync(new InspectionHandoff()).WaitAsync(TimeSpan.FromSeconds(10))).Status);
            await probe.Started("mixed").Task.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.IsFalse(probe.Completed("mixed").Task.IsCompleted);
            probe.Gate("mixed").TrySetResult();
            await probe.Completed("mixed").Task.WaitAsync(TimeSpan.FromSeconds(10));
        }
        finally
        {
            probe.ReleaseAll();
            await host.StopAsync();
        }
    }

    [TestMethod]
    [Timeout(60_000)]
    public async Task CheckpointRecoveryBindsInspectionBeforeResumingNodes()
    {
        var probe = new InspectionProbe();
        using var host = CreateHost(probe);
        await host.StartAsync();
        try
        {
            using var behavior = host.Services.GetRequiredService<IBehaviorFactory>()
                .CreateBehavior(new ActivityId(ActivityName, "recovery"));
            Assert.AreEqual(EventStatus.Accepted,
                (await behavior.SendAsync(new TokensInput().Add(1), StreamHeaders)).Status);
            await probe.Started("recovery").Task.WaitAsync(TimeSpan.FromSeconds(10));
            var checkpoint = behavior.SendAsync(new ActivityCheckpoint());
            await probe.PauseRequested.Task.WaitAsync(TimeSpan.FromSeconds(10));
            probe.ReleaseAll();
            Assert.AreEqual(EventStatus.Consumed, (await checkpoint.WaitAsync(TimeSpan.FromSeconds(10))).Status);
            Assert.IsFalse(probe.Completed("recovery").Task.IsCompleted);

            var management = host.Services.GetRequiredService<IGrainFactory>().GetGrain<IManagementGrain>(0);
            await management.ForceActivationCollection(TimeSpan.Zero);
            Assert.AreEqual(0, (await management.GetSimpleGrainStatistics())
                .Where(statistic => statistic.GrainType.Contains(nameof(StateflowsBehavior)))
                .Sum(statistic => statistic.ActivationCount));
            await behavior.GetStatusAsync().WaitAsync(TimeSpan.FromSeconds(10));
            await probe.Completed("recovery").Task.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.AreEqual(EventStatus.Consumed, (await behavior.GetPlantUmlAsync()).Status);
        }
        finally
        {
            probe.ReleaseAll();
            await host.StopAsync();
        }
    }

    private static IHost CreateHost(InspectionProbe probe)
    {
        using var siloPort = new TcpListener(IPAddress.Loopback, 0);
        using var gatewayPort = new TcpListener(IPAddress.Loopback, 0);
        siloPort.Start();
        gatewayPort.Start();
        var silo = ((IPEndPoint)siloPort.LocalEndpoint).Port;
        var gateway = ((IPEndPoint)gatewayPort.LocalEndpoint).Port;
        var cluster = Guid.NewGuid().ToString("N");
        return Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddSingleton(probe);
                services.AddStateflows(sf => sf.AddPlantUml().AddActivities(activities => activities
                    .AddActivity(ActivityName, activity => activity
                        .AddInputPin<int>(pin => pin.AddFlow("gate"))
                        .AddAction("gate", async context =>
                        {
                            var instance = context.Behavior.Id.Instance;
                            probe.Started(instance).TrySetResult();
                            await probe.Gate(instance).Task;
                            context.PassAllTokensOn();
                        }, node => node.AddControlFlow<InspectAction>())
                        .AddAction<InspectAction>())
                    .AddActivity("inspection-handoff", activity => activity
                        .AddInitial(initial => initial.AddControlFlow("accept"))
                        .AddAcceptEventAction<InspectionHandoff>("accept", _ => Task.CompletedTask,
                            node => node.AddControlFlow("join"))
                        .AddInputPin<int>(pin => pin.AddFlow("join").AddFlow("keepalive"))
                        .AddAction("join", async context =>
                        {
                            probe.Started(context.Behavior.Id.Instance).TrySetResult();
                            await probe.Gate(context.Behavior.Id.Instance).Task;
                        }, node => node.AddControlFlow<InspectAction>())
                        .AddAction<InspectAction>()
                        .AddAction("keepalive", async _ =>
                        {
                            probe.Started("keepalive").TrySetResult();
                            await probe.Gate("keepalive").Task;
                        }))));
                services.Configure<StateflowsRuntimeOptions>(options =>
                {
                    options.SuspendActivityCheckpointResumeForTesting = true;
                    options.ActivityCheckpointPauseRequestedForTesting = _ => probe.PauseRequested.TrySetResult();
                });
            })
            .ConfigureLogging(logging => logging.SetMinimumLevel(LogLevel.Error))
            .UseOrleans(builder => builder
                .UseLocalhostClustering(silo, gateway, null, cluster, cluster)
                .UseInMemoryReminderService()
                .AddMemoryGrainStorageAsDefault()
                .AddStateflowsRuntime())
            .Build();
    }

    private sealed class InspectionHandoff;

    public sealed class InspectAction(
        IActivityInspection inspection, IActivityContext activity, IServiceProvider services, InspectionProbe probe) : IActionNode
    {
        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await Task.Yield();
            try
            {
                Assert.AreEqual(activity.Id, inspection.Id);
                Assert.AreSame(inspection, services.GetRequiredService<IActivityInspection>());
                probe.Completed(activity.Id.Instance).TrySetResult();
            }
            catch (Exception exception)
            {
                probe.Completed(activity.Id.Instance).TrySetException(exception);
                throw;
            }
        }
    }

    public sealed class InspectionProbe
    {
        private readonly ConcurrentDictionary<string, TaskCompletionSource> gates = new();
        private readonly ConcurrentDictionary<string, TaskCompletionSource> starts = new();
        private readonly ConcurrentDictionary<string, TaskCompletionSource> completions = new();
        public TaskCompletionSource PauseRequested { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource Gate(string instance) => Get(gates, instance);
        public TaskCompletionSource Started(string instance) => Get(starts, instance);
        public TaskCompletionSource Completed(string instance) => Get(completions, instance);
        private static TaskCompletionSource Get(ConcurrentDictionary<string, TaskCompletionSource> tasks, string instance)
            => tasks.GetOrAdd(instance, _ => new(TaskCreationOptions.RunContinuationsAsynchronously));
        public void ReleaseAll()
        {
            foreach (var gate in gates.Values)
                gate.TrySetResult();
        }
    }
}
