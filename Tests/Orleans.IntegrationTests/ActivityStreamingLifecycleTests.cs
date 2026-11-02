using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Stateflows;
using Stateflows.Activities;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Common;
using Stateflows.Common.Interfaces;

namespace Orleans.IntegrationTests;

[TestClass]
[DoNotParallelize]
public sealed class ActivityStreamingLifecycleTests
{
    private static readonly TimeSpan Limit = TimeSpan.FromSeconds(10);
    private static readonly Dictionary<string, EventHeader> StreamHeader = new()
    {
        [nameof(Stateflows.Common.Stream)] = new Stateflows.Common.Stream()
    };

    [TestMethod]
    [Timeout(45_000)]
    public async Task FailedRegularCallRetainsValueWritesWhileStreamIsLive()
    {
        var probe = new LifecycleProbe();
        await using var cluster = await LifecycleCluster.StartAsync(probe, ValueActivity(probe));
        using var behavior = cluster.Factory.CreateBehavior(new ActivityId("lifecycle-values", "busy"));

        Assert.AreEqual(EventStatus.Accepted,
            (await behavior.SendAsync(new TokensInput().Add("keepalive"), StreamHeader).WaitAsync(Limit)).Status);
        await probe.Signal("stream-started").Task.WaitAsync(Limit);
        Assert.AreEqual(EventStatus.Consumed,
            (await behavior.SendAsync(new TokensInput().Add(0)).WaitAsync(Limit)).Status);
        Assert.AreEqual(EventStatus.Failed,
            (await behavior.SendAsync(new TokensInput().Add(1)).WaitAsync(Limit)).Status);
        Assert.AreEqual(EventStatus.Consumed,
            (await behavior.SendAsync(new TokensInput().Add(2)).WaitAsync(Limit)).Status);
        Assert.AreEqual(99, probe.ObservedValue,
            "A later regular call must see the failed call's live write, not its pre-call snapshot.");
        Assert.IsFalse(probe.Signal("stream-observed").Task.IsCompleted);

        probe.Gate("stream").TrySetResult();
        await probe.Signal("stream-observed").Task.WaitAsync(Limit);
        Assert.AreEqual(99, probe.StreamObservedValue,
            "The still-running streamed action must share the value modified by the failed regular call.");
    }

    [TestMethod]
    [Timeout(45_000)]
    public async Task FailedRegularCallRollsBackValueWritesAfterStreamNaturallyQuiesces()
    {
        var probe = new LifecycleProbe();
        await using var cluster = await LifecycleCluster.StartAsync(probe, ValueActivity(probe));
        using var behavior = cluster.Factory.CreateBehavior(new ActivityId("lifecycle-values", "idle"));

        Assert.AreEqual(EventStatus.Accepted,
            (await behavior.SendAsync(new TokensInput().Add("keepalive"), StreamHeader).WaitAsync(Limit)).Status);
        await probe.Signal("stream-started").Task.WaitAsync(Limit);
        Assert.AreEqual(EventStatus.Consumed,
            (await behavior.SendAsync(new TokensInput().Add(0)).WaitAsync(Limit)).Status);
        probe.Gate("stream").TrySetResult();
        await probe.Signal("stream-observed").Task.WaitAsync(Limit);
        Assert.AreEqual(7, probe.StreamObservedValue);

        await WaitForNaturalQuiescenceAsync(behavior);
        Assert.AreEqual(EventStatus.Failed,
            (await behavior.SendAsync(new TokensInput().Add(1)).WaitAsync(Limit)).Status);
        Assert.AreEqual(EventStatus.Consumed,
            (await behavior.SendAsync(new TokensInput().Add(2)).WaitAsync(Limit)).Status);
        Assert.AreEqual(7, probe.ObservedValue,
            "After natural quiescence an ordinary failed call must roll back to the durable value.");
    }

    [TestMethod]
    [Timeout(45_000)]
    public async Task RegularTransactionsRunOneAtATimeInFifoOrderWhileStreamRemainsLive()
    {
        var probe = new LifecycleProbe();
        await using var cluster = await LifecycleCluster.StartAsync(probe, activities => activities
            .AddActivity("lifecycle-fifo", activity => activity
                .AddInput(input => input.AddFlow<string>("stream").AddFlow<int>("regular"))
                .AddAction("stream", async _ =>
                {
                    probe.Signal("stream-started").TrySetResult();
                    await probe.Gate("stream").Task;
                    probe.Signal("stream-completed").TrySetResult();
                })
                .AddAction("regular", async context =>
                {
                    var value = context.GetTokensOfType<int>().Single();
                    var active = Interlocked.Increment(ref probe.ActiveRegular);
                    probe.RegularConcurrency.Enqueue(active);
                    probe.Order.Enqueue(value);
                    context.Behavior.Publish(new LifecycleNotice { Value = value });
                    probe.Signal($"regular:{value}").TrySetResult();
                    try
                    {
                        await probe.Gate($"regular:{value}").Task;
                    }
                    finally
                    {
                        Interlocked.Decrement(ref probe.ActiveRegular);
                    }
                }, node => node.Reentrant())));
        using var behavior = cluster.Factory.CreateBehavior(new ActivityId("lifecycle-fifo", "fifo"));
        var notices = Channel.CreateUnbounded<int>();
        await using var watcher = await WatchNoticesAsync(behavior, notices);

        Assert.AreEqual(EventStatus.Accepted,
            (await behavior.SendAsync(new TokensInput().Add("keepalive"), StreamHeader).WaitAsync(Limit)).Status);
        await probe.Signal("stream-started").Task.WaitAsync(Limit);
        var first = behavior.SendAsync(new TokensInput().Add(1));
        await probe.Signal("regular:1").Task.WaitAsync(Limit);
        var second = behavior.SendAsync(new TokensInput().Add(2));
        await AssertStillPendingAsync(probe.Signal("regular:2").Task,
            "The second regular firing must not enter while the first transaction owns its gate.");
        // Both later calls are dispatched before the first gate opens. The publication of the first
        // transaction is a commit witness; the second must not publish or enter its action before it.
        Assert.IsFalse(notices.Reader.TryRead(out _));
        Assert.IsFalse(second.IsCompleted);
        probe.Gate("regular:1").TrySetResult();
        Assert.AreEqual(EventStatus.Consumed, (await first.WaitAsync(Limit)).Status);
        Assert.AreEqual(1, await ReadNoticeAsync(notices));
        await probe.Signal("regular:2").Task.WaitAsync(Limit);
        Assert.IsFalse(second.IsCompleted);
        Assert.IsFalse(probe.Signal("stream-completed").Task.IsCompleted,
            "A regular transaction must not wait for unrelated streamed work to exhaust.");
        probe.Gate("regular:2").TrySetResult();
        Assert.AreEqual(EventStatus.Consumed, (await second.WaitAsync(Limit)).Status);
        Assert.AreEqual(2, await ReadNoticeAsync(notices));
        CollectionAssert.AreEqual(new[] { 1, 2 }, probe.Order.ToArray());
        CollectionAssert.AreEqual(new[] { 1, 1 }, probe.RegularConcurrency.ToArray(),
            "Node reentrancy must not allow overlapping regular transactions.");
    }

    [TestMethod]
    [Timeout(45_000)]
    public async Task ImmediateFinalizeAcknowledgesBeforeNoncooperativeDrainAndFencesLateEngineEffects()
    {
        var probe = new LifecycleProbe();
        await using var cluster = await LifecycleCluster.StartAsync(probe, activities => activities
            .AddActivity("lifecycle-immediate", activity => activity
                .AddInput(input => input.AddFlow<int>("late").AddFlow<string>("read"))
                .AddAction("late", async context =>
                {
                    await context.Behavior.Values.SetAsync("value", 7);
                    probe.Signal("stream-started").TrySetResult();
                    await probe.Gate("late").Task;
                    Interlocked.Increment(ref probe.ExternalEffects);
                    await context.Behavior.Values.UpdateAsync<int>("value", value => value + 1);
                    await context.Behavior.Values.RemoveAsync("value");
                    await context.Behavior.Values.RemovePrefixedAsync("val");
                    await context.Behavior.Values.ClearAsync();
                    await context.Behavior.Values.SetAsync("value", 99);
                    context.Behavior.Publish(new LifecycleNotice { Value = 99 });
                    context.PassAllTokensOn();
                    probe.Signal("late-body-ended").TrySetResult();
                }, node => node.AddFlow<int>("downstream"))
                .AddAction("downstream", context =>
                {
                    Interlocked.Increment(ref probe.DownstreamEffects);
                    context.PassAllTokensOn();
                    return Task.CompletedTask;
                }, node => node.AddFlow<int, OutputNode>())
                .AddOutput()
                .AddAction("read", async context =>
                {
                    var (found, value) = await context.Behavior.Values.TryGetAsync<int>("value");
                    probe.ObservedValue = found ? value : -1;
                    context.Behavior.Publish(new LifecycleNotice { Value = 7 });
                })
                .AddFinalizer(async context =>
                {
                    var (found, value) = await context.Behavior.Values.TryGetAsync<int>("value");
                    probe.FinalizedValue = found ? value : -1;
                    probe.Signal("finalized").TrySetResult();
                })));
        using var behavior = cluster.Factory.CreateBehavior(new ActivityId("lifecycle-immediate", "cancel"));
        var notices = Channel.CreateUnbounded<int>();
        var outputs = Channel.CreateUnbounded<TokensOutput>();
        await using var watcher = await WatchNoticesAsync(behavior, notices);
        await using var outputWatcher = await behavior.WatchAsync<TokensOutput>(output =>
        {
            outputs.Writer.TryWrite(output);
            return Task.CompletedTask;
        });

        Assert.AreEqual(EventStatus.Accepted,
            (await behavior.SendAsync(new TokensInput().Add(1), StreamHeader).WaitAsync(Limit)).Status);
        await probe.Signal("stream-started").Task.WaitAsync(Limit);
        Assert.AreEqual(EventStatus.Cancelled,
            (await behavior.SendAsync(new Finalize { Mode = FinalizationMode.Immediate }).WaitAsync(Limit)).Status);
        Assert.IsFalse(probe.Signal("late-body-ended").Task.IsCompleted,
            "Immediate cancellation acknowledges without waiting for non-cooperative user code.");
        Assert.IsFalse(probe.Signal("finalized").Task.IsCompleted,
            "The finalizer must wait for active scopes to drain, even after cancellation is acknowledged.");

        probe.Gate("late").TrySetResult();
        await probe.Signal("late-body-ended").Task.WaitAsync(Limit);
        await probe.Signal("finalized").Task.WaitAsync(Limit);
        Assert.AreEqual(1, System.Threading.Volatile.Read(ref probe.ExternalEffects),
            "Arbitrary external effects of non-cooperative code cannot be fenced.");
        Assert.AreEqual(7, probe.FinalizedValue,
            "The finalizer must see the pre-cancellation value, not the non-cooperative body's late write.");
        Assert.AreEqual(0, System.Threading.Volatile.Read(ref probe.DownstreamEffects),
            "Late outputs from the cancelled generation must not start successor nodes.");
        Assert.IsFalse(outputs.Reader.TryRead(out _), "Late output tokens must not be published.");
        Assert.IsFalse(notices.Reader.TryRead(out _), "A cancelled generation must not publish late notices.");
        var cancelledOutput = await behavior.RequestAsync(new TokensOutputRequest()).WaitAsync(Limit);
        Assert.AreEqual(EventStatus.Consumed, cancelledOutput.Status);
        Assert.AreEqual(0, cancelledOutput.Response!.Tokens.Count,
            "The finalized marking must not retain cancelled-generation output tokens.");

        // Reset preserves the watcher but clears the finalized marking so a new generation can probe it.
        Assert.AreEqual(EventStatus.Consumed,
            (await behavior.SendAsync(new Reset { Mode = ResetMode.KeepVersionAndSubscriptions }).WaitAsync(Limit)).Status);
        Assert.AreEqual(EventStatus.Consumed,
            (await behavior.SendAsync(new TokensInput().Add("read")).WaitAsync(Limit)).Status);
        Assert.AreEqual(-1, probe.ObservedValue, "The reset generation must not inherit cancelled writes.");
        Assert.AreEqual(7, await ReadNoticeAsync(notices),
            "A live-generation publication must be delivered, with no cancelled notice preceding it.");
    }

    [TestMethod]
    [Timeout(45_000)]
    public async Task QueuedFinalizeWaitsEarlierRegularTransactionThenCancelsStreamRatherThanNaturalDrain()
    {
        var probe = new LifecycleProbe();
        await using var cluster = await LifecycleCluster.StartAsync(probe, activities => activities
            .AddActivity("lifecycle-queued", activity => activity
                .AddInput(input => input.AddFlow<string>("stream").AddFlow<int>("regular"))
                .AddAction("stream", async context =>
                {
                    using var registration = context.CancellationToken.Register(() =>
                        probe.Signal("stream-cancelled").TrySetResult());
                    using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(
                        context.CancellationToken, probe.Cleanup.Token);
                    probe.Signal("stream-started").TrySetResult();
                    await Task.Delay(Timeout.InfiniteTimeSpan, cancellation.Token);
                    probe.Signal("stream-naturally-ended").TrySetResult();
                })
                .AddAction("regular", async context =>
                {
                    probe.Signal("regular-started").TrySetResult();
                    await probe.Gate("regular").Task;
                    context.Behavior.Publish(new LifecycleNotice { Value = 1 });
                    probe.Order.Enqueue(1);
                })
                .AddFinalizer(_ =>
                {
                    probe.Order.Enqueue(2);
                    probe.Signal("finalized").TrySetResult();
                    return Task.CompletedTask;
                })));
        using var behavior = cluster.Factory.CreateBehavior(new ActivityId("lifecycle-queued", "queued"));
        var notices = Channel.CreateUnbounded<int>();
        await using var watcher = await WatchNoticesAsync(behavior, notices);

        Assert.AreEqual(EventStatus.Accepted,
            (await behavior.SendAsync(new TokensInput().Add("keepalive"), StreamHeader).WaitAsync(Limit)).Status);
        await probe.Signal("stream-started").Task.WaitAsync(Limit);
        var regular = behavior.SendAsync(new TokensInput().Add(1));
        await probe.Signal("regular-started").Task.WaitAsync(Limit);
        var finalize = behavior.SendAsync(new Finalize { Mode = FinalizationMode.Queued });
        await AssertStillPendingAsync(probe.Signal("stream-cancelled").Task,
            "Queued finalization must not cancel streamed scopes ahead of the earlier regular transaction.");
        Assert.IsFalse(finalize.IsCompleted);
        Assert.IsFalse(probe.Signal("stream-cancelled").Task.IsCompleted);
        probe.Gate("regular").TrySetResult();
        Assert.AreEqual(EventStatus.Consumed, (await regular.WaitAsync(Limit)).Status);
        Assert.AreEqual(1, await ReadNoticeAsync(notices));
        await probe.Signal("stream-cancelled").Task.WaitAsync(Limit);
        Assert.AreEqual(EventStatus.Consumed, (await finalize.WaitAsync(Limit)).Status);
        await probe.Signal("finalized").Task.WaitAsync(Limit);
        CollectionAssert.AreEqual(new[] { 1, 2 }, probe.Order.ToArray(),
            "Queued finalization must follow the earlier transaction, not interrupt it.");
        Assert.IsFalse(probe.Signal("stream-naturally-ended").Task.IsCompleted,
            "Finalization must cancel the infinite streamed action, not await natural stream exhaustion.");
    }

    [TestMethod]
    [Timeout(45_000)]
    public async Task CheckpointResetClearsDrainedValuesAndDiscardsReadySuccessors()
    {
        var probe = new LifecycleProbe();
        await using var cluster = await LifecycleCluster.StartAsync(probe, activities => activities
            .AddActivity("lifecycle-reset", activity => activity
                .AddInput(input => input.AddFlow<int>("first").AddFlow<string>("read"))
                .AddAction("first", async context =>
                {
                    await context.Behavior.Values.SetAsync("value", 99);
                    probe.Signal("stream-started").TrySetResult();
                    await probe.Gate("first").Task;
                    context.PassAllTokensOn();
                }, node => node.AddFlow<int>("successor"))
                .AddAction("successor", context =>
                {
                    Interlocked.Increment(ref probe.DownstreamEffects);
                    context.Behavior.Publish(new LifecycleNotice { Value = 99 });
                    context.PassAllTokensOn();
                    return Task.CompletedTask;
                }, node => node.AddFlow<int, OutputNode>())
                .AddOutput()
                .AddAction("read", async context =>
                {
                    var (found, value) = await context.Behavior.Values.TryGetAsync<int>("value");
                    probe.ObservedValue = found ? value : -1;
                    context.Behavior.Publish(new LifecycleNotice { Value = 7 });
                })), options =>
                    options.ActivityCheckpointPauseRequestedForTesting = _ =>
                        probe.Signal("checkpoint-requested").TrySetResult());
        using var behavior = cluster.Factory.CreateBehavior(new ActivityId("lifecycle-reset", "reset"));
        var notices = Channel.CreateUnbounded<int>();
        await using var watcher = await WatchNoticesAsync(behavior, notices);

        Assert.AreEqual(EventStatus.Accepted,
            (await behavior.SendAsync(new TokensInput().Add(1), StreamHeader).WaitAsync(Limit)).Status);
        await probe.Signal("stream-started").Task.WaitAsync(Limit);
        var reset = behavior.ResetAsync(ResetMode.KeepVersionAndSubscriptions,
            new Dictionary<string, EventHeader> { [nameof(Checkpoint)] = new Checkpoint() });
        await probe.Signal("checkpoint-requested").Task.WaitAsync(Limit);
        Assert.IsFalse(reset.IsCompleted, "Checkpoint reset waits for the currently active firing boundary.");
        probe.Gate("first").TrySetResult();
        Assert.AreEqual(EventStatus.Consumed, (await reset.WaitAsync(Limit)).Status);
        await WaitForNaturalQuiescenceAsync(behavior);
        Assert.AreEqual(EventStatus.Consumed,
            (await behavior.SendAsync(new TokensInput().Add("read")).WaitAsync(Limit)).Status);
        Assert.AreEqual(-1, probe.ObservedValue, "Reset must clear values written before the barrier.");
        Assert.AreEqual(0, System.Threading.Volatile.Read(ref probe.DownstreamEffects),
            "Ready work from the reset generation must not be resumed after the checkpoint barrier.");
        Assert.AreEqual(7, await ReadNoticeAsync(notices),
            "The reset generation's publication must arrive without a discarded successor's notice.");
        var output = await behavior.RequestAsync(new TokensOutputRequest()).WaitAsync(Limit);
        Assert.AreEqual(EventStatus.Consumed, output.Status);
        Assert.AreEqual(0, output.Response!.Tokens.Count, "Reset must discard old marking and output tokens.");
    }

    private static ActivitiesBuildAction ValueActivity(LifecycleProbe probe) => activities => activities
        .AddActivity("lifecycle-values", activity => activity
            .AddInput(input => input.AddFlow<string>("stream").AddFlow<int>("regular"))
            .AddAction("stream", async context =>
            {
                probe.Signal("stream-started").TrySetResult();
                await probe.Gate("stream").Task;
                var (found, value) = await context.Behavior.Values.TryGetAsync<int>("value");
                probe.StreamObservedValue = found ? value : -1;
                probe.Signal("stream-observed").TrySetResult();
            })
            .AddAction("regular", async context =>
            {
                switch (context.GetTokensOfType<int>().Single())
                {
                    case 0:
                        await context.Behavior.Values.SetAsync("value", 7);
                        break;
                    case 1:
                        await context.Behavior.Values.SetAsync("value", 99);
                        throw new InvalidOperationException("Expected regular transaction failure.");
                    case 2:
                        var (found, value) = await context.Behavior.Values.TryGetAsync<int>("value");
                        probe.ObservedValue = found ? value : -1;
                        break;
                }
            }));

    private static Task<IWatcher> WatchNoticesAsync(IBehavior behavior, Channel<int> notices)
        => behavior.WatchAsync<LifecycleNotice>(notice =>
        {
            notices.Writer.TryWrite(notice.Value);
            return Task.CompletedTask;
        });

    private static Task<int> ReadNoticeAsync(Channel<int> notices)
        => notices.Reader.ReadAsync().AsTask().WaitAsync(Limit);

    private static async Task AssertStillPendingAsync(Task operation, string message)
    {
        var observation = Task.Delay(150);
        Assert.AreSame(observation, await Task.WhenAny(operation, observation), message);
    }

    private static async Task WaitForNaturalQuiescenceAsync(IBehavior behavior)
    {
        // Token requests are rejected while a stream is active. Successful transport admission of an
        // empty request witnesses natural quiescence without using internal flags or forcing a checkpoint.
        using var timeout = new CancellationTokenSource(Limit);
        while (true)
        {
            var result = await behavior.RequestAsync(new TokensInput()).WaitAsync(timeout.Token);
            if (result.Status != EventStatus.Rejected)
            {
                Assert.AreEqual(EventStatus.Consumed, result.Status);
                return;
            }
            await Task.Delay(10, timeout.Token);
        }
    }

    public sealed class LifecycleNotice
    {
        public int Value { get; set; }
    }

    private sealed class LifecycleProbe
    {
        private readonly ConcurrentDictionary<string, TaskCompletionSource> gates = new();
        private readonly ConcurrentDictionary<string, TaskCompletionSource> signals = new();
        public readonly ConcurrentQueue<int> Order = new();
        public readonly ConcurrentQueue<int> RegularConcurrency = new();
        public int ActiveRegular;
        public int ObservedValue;
        public int StreamObservedValue;
        public int FinalizedValue;
        public int ExternalEffects;
        public int DownstreamEffects;
        public readonly CancellationTokenSource Cleanup = new();

        public TaskCompletionSource Gate(string key)
        {
            var gate = gates.GetOrAdd(key, _ => new(TaskCreationOptions.RunContinuationsAsynchronously));
            if (Cleanup.IsCancellationRequested)
                gate.TrySetResult();
            return gate;
        }

        public TaskCompletionSource Signal(string key)
            => signals.GetOrAdd(key, _ => new(TaskCreationOptions.RunContinuationsAsynchronously));

        public void ReleaseAll()
        {
            Cleanup.Cancel();
            foreach (var gate in gates.Values)
                gate.TrySetResult();
        }
    }

    private sealed class LifecycleCluster(IHost host, LifecycleProbe probe) : IAsyncDisposable
    {
        public IBehaviorFactory Factory => host.Services.GetRequiredService<IBehaviorFactory>();

        public static async Task<LifecycleCluster> StartAsync(
            LifecycleProbe probe, ActivitiesBuildAction build, Action<StateflowsRuntimeOptions>? configure = null)
        {
            var listeners = new[] { new TcpListener(IPAddress.Loopback, 0), new TcpListener(IPAddress.Loopback, 0) };
            int[] ports;
            try
            {
                foreach (var listener in listeners)
                    listener.Start();
                ports = listeners.Select(listener => ((IPEndPoint)listener.LocalEndpoint).Port).ToArray();
            }
            finally
            {
                foreach (var listener in listeners)
                    listener.Stop();
            }
            var clusterId = Guid.NewGuid().ToString("N");
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((_, services) =>
                {
                    services.AddSingleton(probe);
                    services.AddStateflows(sf => sf.AddActivities(build));
                    if (configure != null)
                        services.Configure(configure);
                })
                .ConfigureLogging(logging => logging.SetMinimumLevel(LogLevel.Error))
                .UseOrleans(silo => silo
                    .UseLocalhostClustering(ports[0], ports[1], null, clusterId, clusterId)
                    .UseInMemoryReminderService()
                    .AddMemoryGrainStorageAsDefault()
                    .AddStateflowsRuntime())
                .Build();
            try
            {
                await host.StartAsync().WaitAsync(Limit);
                return new LifecycleCluster(host, probe);
            }
            catch
            {
                host.Dispose();
                throw;
            }
        }

        public async ValueTask DisposeAsync()
        {
            probe.ReleaseAll();
            try
            {
                await host.StopAsync().WaitAsync(Limit);
            }
            finally
            {
                host.Dispose();
                probe.Cleanup.Dispose();
            }
        }
    }
}
