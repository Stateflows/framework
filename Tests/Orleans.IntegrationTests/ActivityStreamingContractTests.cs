using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Stateflows;
using Stateflows.Activities;
using Stateflows.Activities.Context.Classes;
using Stateflows.Common;
using Stateflows.Common.Interfaces;

namespace Orleans.IntegrationTests;

/// <summary>
/// Provenance contracts of the activation-owned Orleans Activity session: a regular event already running shares the
/// live network with a joining stream, transactional event sources stay transactional when delivered by the live
/// timer pump, and mixed acceptance inside structured/iterative/parallel scopes hands off like a flat graph.
/// </summary>
[TestClass]
public sealed class ActivityStreamingContractTests
{
    private static readonly Dictionary<string, EventHeader> StreamHeader = new()
    {
        [nameof(Stateflows.Common.Stream)] = new Stateflows.Common.Stream()
    };

    private static bool IsStreamed(Stateflows.Activities.Context.Interfaces.IActionContext context)
    {
        var streamed = ((ActionContext)context).IsStreamed;
        Assert.AreEqual(streamed ? ExecutionMode.Streamed : ExecutionMode.Transactional, context.ExecutionMode);
        if (streamed)
        {
            Assert.IsNull(context.ExecutionTrigger);
            Assert.IsNull(context.ExecutionTriggerId);
            Assert.IsNull(context.Headers);
        }
        else
        {
            Assert.IsNotNull(context.ExecutionTrigger);
            Assert.IsNotNull(context.ExecutionTriggerId);
            Assert.IsNotNull(context.Headers);
        }
        return streamed;
    }

    [TestMethod]
    [Timeout(60_000)]
    public async Task TypedPinAdmissionRejectsWholeUnsupportedBatchesAndPreservesStreamedOutputs()
    {
        var probe = new ContractProbe();
        await using var cluster = await ContractCluster.StartAsync(probe, activities => activities
            .AddActivity("stream-pins", activity => activity
                .AddDefaultInitializer(context =>
                {
                    probe.Count(context.Behavior.Id.Instance, "initialize");
                    return Task.FromResult(true);
                })
                .AddInputPin<int>(pin => pin.AddFlow("number"))
                .AddInputPin<string>(pin => pin.AddFlow("text"))
                .AddOutputPin<int>().AddOutputPin<string>()
                .AddAction("number", context =>
                {
                    probe.Record($"{context.Behavior.Id.Instance}:number-mode", IsStreamed(context).ToString());
                    probe.Count(context.Behavior.Id.Instance, "number");
                    context.PassTokensOfTypeOn<int>();
                    return Task.CompletedTask;
                }, node => node.AddFlow<int, OutputPin<int>>())
                .AddAction("text", context =>
                {
                    probe.Record($"{context.Behavior.Id.Instance}:text-mode", IsStreamed(context).ToString());
                    probe.Count(context.Behavior.Id.Instance, "text");
                    context.PassTokensOfTypeOn<string>();
                    return Task.CompletedTask;
                }, node => node.AddFlow<string, OutputPin<string>>())));

        using var behavior = cluster.Factory.CreateBehavior(new ActivityId("stream-pins", "a"));
        var invalid = new TokensInput().Add(42).Add(1.5);
        Assert.AreEqual(EventStatus.Rejected, (await behavior.SendAsync(invalid, StreamHeader)).Status);
        Assert.AreEqual(0, probe.Get("a", "initialize"));
        Assert.AreEqual(0, probe.Get("a", "number"));
        Assert.AreEqual(EventStatus.Rejected, (await behavior.SendAsync(new TokensInput().Add(42).Add(1.5))).Status);
        Assert.AreEqual(0, probe.Get("a", "initialize"));
        Assert.AreEqual(EventStatus.Accepted, (await behavior.SendAsync(new TokensInput().Add(42).Add("hello"), StreamHeader)).Status);
        await probe.WaitForCountAsync("a", "number", 1);
        await probe.WaitForCountAsync("a", "text", 1);
        Assert.AreEqual(EventStatus.Consumed, (await behavior.SendAsync(new ActivityCheckpoint())).Status);
        Assert.AreEqual(1, probe.Get("a", "initialize"));
        Assert.AreEqual("True", probe.Recorded("a:number-mode"));
        Assert.AreEqual("True", probe.Recorded("a:text-mode"));
        Assert.AreEqual(EventStatus.Rejected, (await behavior.SendAsync(new TokensInput().Add(43).Add(1.5), StreamHeader)).Status);
        Assert.AreEqual(1, probe.Get("a", "number"));
        var output = await behavior.RequestAsync(new TokensOutputRequest());
        CollectionAssert.AreEqual(new[] { 42 }, output.Response.GetOfType<int>().ToArray());
        CollectionAssert.AreEqual(new[] { "hello" }, output.Response.GetOfType<string>().ToArray());
    }

    [TestMethod]
    [Timeout(60_000)]
    public async Task StreamJoiningRunningRegularEventSharesNetworkAndKeepsRegularProvenance()
    {
        var probe = new ContractProbe();
        await using var cluster = await ContractCluster.StartAsync(probe, activities => activities
            .AddActivity("join-shared", activity => activity
                .AddInput(input => input.AddFlow<int>("work"))
                .AddAction("work", async context =>
                {
                    var instance = context.Behavior.Id.Instance;
                    var value = context.GetTokensOfType<int>().Single();
                    probe.Record($"{instance}:streamed:{value}", IsStreamed(context).ToString());
                    if (value == 10)
                    {
                        await context.Behavior.Values.SetAsync("regular-wrote", 10);
                        context.Behavior.Publish(new ContractNotice { Value = 10 });
                        probe.Started($"{instance}:10").TrySetResult();
                        await probe.Gate($"{instance}:10").Task;
                        probe.Count(instance, "regular-done");
                    }
                    else
                    {
                        var (found, written) = await context.Behavior.Values.TryGetAsync<int>("regular-wrote");
                        probe.Record($"{instance}:observed", found ? written.ToString() : "none");
                        context.Behavior.Publish(new ContractNotice { Value = value });
                        probe.Started($"{instance}:{value}").TrySetResult();
                        await probe.Gate($"{instance}:{value}").Task;
                        probe.Count(instance, "stream-done");
                    }
                }, node => node.Reentrant())));

        using var behavior = cluster.Factory.CreateBehavior(new ActivityId("join-shared", "a"));
        var notices = Channel.CreateUnbounded<int>();
        await using var watcher = await behavior.WatchAsync<ContractNotice>(notice =>
        {
            notices.Writer.TryWrite(notice.Value);
            return Task.CompletedTask;
        });

        var regular = behavior.SendAsync(new TokensInput().Add(10));
        await probe.Started("a:10").Task.WaitAsync(TimeSpan.FromSeconds(10));

        Assert.AreEqual(EventStatus.Accepted, (await behavior.SendAsync(new TokensInput().Add(11), StreamHeader)
            .WaitAsync(TimeSpan.FromSeconds(10))).Status, "The stream is admitted while the regular firing is running.");
        await probe.Started("a:11").Task.WaitAsync(TimeSpan.FromSeconds(10));

        Assert.AreEqual("10", probe.Recorded("a:observed"),
            "The streamed firing runs in the same live network and observes the running regular firing's write.");
        Assert.AreEqual(11, await notices.Reader.ReadAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(10)),
            "A streamed firing publishes immediately.");
        Assert.IsFalse(notices.Reader.TryRead(out _), "The regular firing's publication stays deferred to its call.");
        Assert.IsFalse(regular.IsCompleted, "The regular call is still owned by its gated transactional firing.");
        Assert.AreEqual(bool.FalseString, probe.Recorded("a:streamed:10"), "A pure regular firing keeps its provenance.");
        Assert.AreEqual(bool.TrueString, probe.Recorded("a:streamed:11"));

        probe.Gate("a:10").TrySetResult();
        Assert.AreEqual(EventStatus.Consumed, (await regular.WaitAsync(TimeSpan.FromSeconds(10))).Status);
        Assert.AreEqual(10, await notices.Reader.ReadAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(10)),
            "The regular publication is committed when the regular call completes.");
        Assert.AreEqual(0, probe.Get("a", "stream-done"), "The streamed firing is still live after the regular call.");
        Assert.AreEqual(bool.FalseString, probe.Recorded("a:streamed:10"));

        probe.Gate("a:11").TrySetResult();
        await probe.WaitForCountAsync("a", "stream-done", 1);
        Assert.AreEqual(1, probe.Get("a", "regular-done"));
    }

    [TestMethod]
    [Timeout(60_000)]
    public async Task LiveTimerEventSourceStaysTransactionalDuringStream()
    {
        var probe = new ContractProbe();
        await using var cluster = await ContractCluster.StartAsync(probe, activities => activities
            .AddActivity("timer-source", activity => activity
                .AddTimeEventAction<ContractDelay>("timer", async context =>
                {
                    var instance = context.Behavior.Id.Instance;
                    if (probe.Count(instance, "timer") > 1)
                        return;
                    probe.Record($"{instance}:timer-streamed", IsStreamed(context).ToString());
                    context.Behavior.Publish(new ContractNotice { Value = 1 });
                    probe.Started($"{instance}:timer").TrySetResult();
                    await probe.Gate($"{instance}:timer").Task;
                })
                .AddInput(input => input.AddFlow<int>("work"))
                .AddAction("work", async context =>
                {
                    var instance = context.Behavior.Id.Instance;
                    probe.Record($"{instance}:work-streamed", IsStreamed(context).ToString());
                    await probe.Gate($"{instance}:work").Task;
                })));

        using var behavior = cluster.Factory.CreateBehavior(new ActivityId("timer-source", "a"));
        var notices = Channel.CreateUnbounded<int>();
        await using var watcher = await behavior.WatchAsync<ContractNotice>(notice =>
        {
            notices.Writer.TryWrite(notice.Value);
            return Task.CompletedTask;
        });

        Assert.AreEqual(EventStatus.Accepted, (await behavior.SendAsync(new TokensInput().Add(1), StreamHeader)
            .WaitAsync(TimeSpan.FromSeconds(10))).Status);
        await probe.Started("a:timer").Task.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.AreEqual(bool.TrueString, probe.Recorded("a:work-streamed"));
        Assert.AreEqual(bool.FalseString, probe.Recorded("a:timer-streamed"),
            "A time event delivered by the live timer pump is a transactional source.");
        await Task.Delay(200);
        Assert.IsFalse(notices.Reader.TryRead(out _),
            "The transactional timer firing defers its publication until it completes successfully.");

        probe.Gate("a:timer").TrySetResult();
        Assert.AreEqual(1, await notices.Reader.ReadAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(10)));
        probe.Gate("a:work").TrySetResult();
    }

    [TestMethod]
    [Timeout(120_000)]
    public async Task RegularSendHandsOffMixedAcceptanceInsideStructuralScopes()
    {
        var probe = new ContractProbe();
        await using var cluster = await ContractCluster.StartAsync(probe, activities => activities
            .AddActivity("handoff-structured", activity => activity
                .AddInput(input => input.AddFlow<int>("scope").AddFlow<string>("keepalive"))
                .AddAction("keepalive", context => KeepAliveAsync(probe, context))
                .AddStructuredActivity("scope", scope => scope
                    .AddOnInitialize(context => Count(probe, context, "initialize"))
                    .AddOnFinalize(context => Count(probe, context, "finalize"))
                    .AddInput(input => input.AddFlow<int>("pure").AddFlow<int>("store"))
                    .AddAction("pure", context => Count(probe, context, $"pure:{context.GetTokensOfType<int>().Single()}"))
                    .AddDataStore("store", store => store.AddFlow<int>("b"))
                    .AddAction("b", context => GatedDownstreamAsync(probe, context), node => node.AddFlow<int, OutputNode>())
                    .AddOutput()
                    .AddFlow<int>("after"))
                .AddAction("after", context => AfterAsync(probe, context)))
            .AddActivity("handoff-iterative", activity => activity
                .AddInput(input => input.AddFlow<int>("scope").AddFlow<string>("keepalive"))
                .AddAction("keepalive", context => KeepAliveAsync(probe, context))
                .AddIterativeActivity<int>("scope", scope => scope
                    .AddOnInitialize(context => Count(probe, context, "initialize"))
                    .AddOnFinalize(context => Count(probe, context, "finalize"))
                    .AddInput(input => input.AddFlow<int>("pure").AddFlow<int>("store"))
                    .AddAction("pure", context => Count(probe, context, $"pure:{context.GetTokensOfType<int>().Single()}"))
                    .AddDataStore("store", store => store.AddFlow<int>("b"))
                    .AddAction("b", context => GatedDownstreamAsync(probe, context), node => node.AddFlow<int, OutputNode>())
                    .AddOutput()
                    .AddFlow<int>("after"))
                .AddAction("after", context => AfterAsync(probe, context)))
            .AddActivity("handoff-parallel", activity => activity
                .AddInput(input => input.AddFlow<int>("scope").AddFlow<string>("keepalive"))
                .AddAction("keepalive", context => KeepAliveAsync(probe, context))
                .AddParallelActivity<int>("scope", scope => scope
                    .AddOnInitialize(context => Count(probe, context, "initialize"))
                    .AddOnFinalize(context => Count(probe, context, "finalize"))
                    .AddInput(input => input.AddFlow<int>("pure").AddFlow<int>("store"))
                    .AddAction("pure", context => Count(probe, context, $"pure:{context.GetTokensOfType<int>().Single()}"))
                    .AddDataStore("store", store => store.AddFlow<int>("b"))
                    .AddAction("b", context => GatedDownstreamAsync(probe, context), node => node.AddFlow<int, OutputNode>())
                    .AddOutput()
                    .AddFlow<int>("after"), 1)
                .AddAction("after", context => AfterAsync(probe, context))));

        var cases = new (string Activity, int[] Input)[]
        {
            ("handoff-structured", [1]),
            ("handoff-iterative", [1, 2]),
            ("handoff-parallel", [1, 2]),
        };

        foreach (var (activity, input) in cases)
        {
            const string instance = "a";
            using var behavior = cluster.Factory.CreateBehavior(new ActivityId(activity, instance));
            var key = $"{activity}";
            Assert.AreEqual(EventStatus.Accepted, (await behavior.SendAsync(new TokensInput().Add("k"), StreamHeader)
                .WaitAsync(TimeSpan.FromSeconds(10))).Status, activity);
            await probe.WaitForCountAsync(key, "keepalive", 1);

            var tokens = new TokensInput();
            foreach (var value in input)
                tokens.Add(value);
            var regular = await behavior.SendAsync(tokens).WaitAsync(TimeSpan.FromSeconds(10));
            Assert.AreEqual(EventStatus.Consumed, regular.Status, activity);

            Assert.AreEqual(0, probe.Get(key, "b-done"),
                $"{activity}: the regular send must complete before the mixed downstream gate opens.");
            Assert.AreEqual(1, probe.Get(key, "initialize"), $"{activity}: the scope is initialized once.");
            Assert.AreEqual(0, probe.Get(key, "finalize"), $"{activity}: the handed-off scope is still live.");
            Assert.AreEqual(1, probe.Get(key, $"pure:{input[0]}"),
                $"{activity}: the pure transactional branch completes within the regular call.");
            Assert.AreEqual(0, probe.Get(key, "keepalive-done"), $"{activity}: the pure streamed branch remains live.");

            await probe.WaitForCountAsync(key, "b", 1);
            Assert.AreEqual(bool.TrueString, probe.Recorded($"{key}:b-streamed"),
                $"{activity}: the handed-off downstream firing is streamed.");
            probe.Gate($"{key}:b").TrySetResult();
            await probe.Completion(key).Task.WaitAsync(TimeSpan.FromSeconds(10));

            foreach (var value in input)
            {
                Assert.AreEqual(1, probe.Get(key, $"pure:{value}"), $"{activity}: pure:{value}");
                Assert.AreEqual(1, probe.Get(key, $"b-done:{value}"), $"{activity}: b-done:{value}");
            }
            Assert.AreEqual(1, probe.Get(key, "initialize"), $"{activity}: initialize must not repeat on handoff.");
            Assert.AreEqual(1, probe.Get(key, "finalize"), $"{activity}: finalize runs once.");
            Assert.AreEqual(1, probe.Get(key, "after"), $"{activity}: the scope's successor runs once.");
            Assert.AreEqual(string.Join(",", input), probe.Recorded($"{key}:after"), $"{activity}: accumulated output.");

            probe.Gate($"{key}:keepalive").TrySetResult();
            await probe.WaitForCountAsync(key, "keepalive-done", 1);
        }
    }

    private static Task Count(ContractProbe probe, Stateflows.Activities.IActivityNodeContext context, string key)
    {
        probe.Count(context.Behavior.Id.Name, key);
        return Task.CompletedTask;
    }

    private static Task Count(ContractProbe probe, Stateflows.Activities.IActivityActionContext context, string key)
    {
        probe.Count(context.Behavior.Id.Name, key);
        return Task.CompletedTask;
    }

    private static async Task KeepAliveAsync(ContractProbe probe, Stateflows.Activities.Context.Interfaces.IActionContext context)
    {
        var key = context.Behavior.Id.Name;
        probe.Count(key, "keepalive");
        await probe.Gate($"{key}:keepalive").Task;
        probe.Count(key, "keepalive-done");
    }

    private static async Task GatedDownstreamAsync(ContractProbe probe, Stateflows.Activities.Context.Interfaces.IActionContext context)
    {
        var key = context.Behavior.Id.Name;
        probe.Count(key, "b");
        probe.Record($"{key}:b-streamed", IsStreamed(context).ToString());
        await probe.Gate($"{key}:b").Task;
        foreach (var value in context.GetTokensOfType<int>())
            probe.Count(key, $"b-done:{value}");
        probe.Count(key, "b-done");
        context.PassAllTokensOn();
    }

    private static Task AfterAsync(ContractProbe probe, Stateflows.Activities.Context.Interfaces.IActionContext context)
    {
        var key = context.Behavior.Id.Name;
        probe.Count(key, "after");
        probe.Record($"{key}:after", string.Join(",", context.GetTokensOfType<int>().OrderBy(value => value)));
        probe.Completion(key).TrySetResult();
        return Task.CompletedTask;
    }

    private sealed class ContractCluster : IAsyncDisposable
    {
        private readonly IHost host;
        private readonly ContractProbe probe;

        private ContractCluster(IHost host, ContractProbe probe)
        {
            this.host = host;
            this.probe = probe;
        }

        public IBehaviorFactory Factory => host.Services.GetRequiredService<IBehaviorFactory>();

        public static async Task<ContractCluster> StartAsync(ContractProbe probe, Stateflows.Activities.Registration.Interfaces.ActivitiesBuildAction build)
        {
            var ports = GetPorts();
            var cluster = Guid.NewGuid().ToString("N");
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((_, services) =>
                {
                    services.AddSingleton(probe);
                    services.AddStateflows(sf => sf.AddActivities(build));
                })
                .ConfigureLogging(logging => logging.SetMinimumLevel(LogLevel.Error))
                .UseOrleans(silo => silo
                    .UseLocalhostClustering(ports[0], ports[1], null, cluster, cluster)
                    .UseInMemoryReminderService()
                    .AddMemoryGrainStorageAsDefault()
                    .AddStateflowsRuntime())
                .Build();
            await host.StartAsync();
            return new ContractCluster(host, probe);
        }

        public async ValueTask DisposeAsync()
        {
            probe.ReleaseAll();
            await host.StopAsync();
            host.Dispose();
        }
    }

    private static int[] GetPorts()
    {
        var listeners = new List<TcpListener>();
        try
        {
            for (var index = 0; index < 2; index++)
            {
                var listener = new TcpListener(IPAddress.Loopback, 0);
                listener.Start();
                listeners.Add(listener);
            }
            return listeners.Select(listener => ((IPEndPoint)listener.LocalEndpoint).Port).ToArray();
        }
        finally
        {
            foreach (var listener in listeners)
                listener.Stop();
        }
    }

    private sealed class ContractNotice
    {
        public int Value { get; set; }
    }

    public sealed class ContractDelay : DelayEvent
    {
        protected override TimeSpan Delay => TimeSpan.FromMilliseconds(300);
    }

    private sealed class ContractProbe
    {
        private readonly ConcurrentDictionary<string, int> counts = new();
        private readonly ConcurrentDictionary<string, string> records = new();
        private readonly ConcurrentDictionary<string, TaskCompletionSource> signals = new();

        private TaskCompletionSource Signal(string key)
            => signals.GetOrAdd(key, _ => new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously));

        public TaskCompletionSource Gate(string key) => Signal($"gate:{key}");
        public TaskCompletionSource Started(string key) => Signal($"started:{key}");
        public TaskCompletionSource Completion(string key) => Signal($"done:{key}");

        public int Count(string instance, string key) => counts.AddOrUpdate($"{instance}|{key}", 1, (_, value) => value + 1);

        public int Get(string instance, string key) => counts.TryGetValue($"{instance}|{key}", out var value) ? value : 0;

        public void Record(string key, string value) => records[key] = value;

        public string? Recorded(string key) => records.TryGetValue(key, out var value) ? value : null;

        public async Task WaitForCountAsync(string instance, string key, int expected)
        {
            var deadline = DateTime.UtcNow.AddSeconds(10);
            while (Get(instance, key) < expected)
            {
                if (DateTime.UtcNow > deadline)
                    throw new TimeoutException($"{instance}|{key} did not reach {expected}.");
                await Task.Delay(10);
            }
        }

        public void ReleaseAll()
        {
            foreach (var signal in signals.Values)
                signal.TrySetResult();
        }
    }
}
