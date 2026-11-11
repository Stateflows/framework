using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Stateflows;
using Stateflows.Activities;
using Stateflows.Common;
using Stateflows.Common.Interfaces;

namespace Orleans.IntegrationTests;

/// <summary>
/// Durable checkpoint recovery across a real deactivation/reactivation and mixed transaction handoff of the
/// activation-owned Orleans Activity streaming session.
/// </summary>
[TestClass]
public sealed class ActivityStreamingRecoveryTests
{
    private static readonly Dictionary<string, EventHeader> StreamHeader = new()
    {
        [nameof(Stateflows.Common.Stream)] = new Stateflows.Common.Stream()
    };

    [TestMethod]
    [Timeout(120_000)]
    [DataRow(false, false)]
    [DataRow(true, false)]
    [DataRow(false, true)]
    [DataRow(true, true)]
    public async Task CheckpointedContinuationsRecoverAfterReactivationLikeContinuousRun(bool standalone, bool interleave)
    {
        var probe = new RecoveryProbe();
        var ports = GetPorts();
        var cluster = Guid.NewGuid().ToString("N");
        using var host = Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
            {
                services.AddSingleton(probe);
                services.AddStateflows(sf => sf.AddActivities(activities => activities
                    .AddActivity("recovery-flat", activity => activity
                        .AddInputPin<int>(input => input.AddFlow("first"))
                        .AddAction("first", async context =>
                        {
                            await probe.GatedAsync(context.Behavior.Id.Instance, "first");
                            context.PassAllTokensOn();
                        }, node => node.AddFlow<int>("second"))
                        .AddAction("second", context =>
                        {
                            Assert.AreEqual(ExecutionMode.Streamed, context.ExecutionMode);
                            Assert.IsNull(context.ExecutionTrigger);
                            Assert.IsNull(context.ExecutionTriggerId);
                            Assert.IsNull(context.Headers);
                            probe.Count(context.Behavior.Id.Instance, "second");
                            probe.Done(context.Behavior.Id.Instance);
                            return Task.CompletedTask;
                        }))
                    .AddActivity("recovery-fork", activity => activity
                        .AddInputPin<int>(input => input.AddFlow("first"))
                        .AddAction("first", async context =>
                        {
                            await probe.GatedAsync(context.Behavior.Id.Instance, "first");
                            context.PassAllTokensOn();
                        }, node => node.AddFlow<int>("b").AddFlow<int>("c"))
                        .AddAction("b", context =>
                        {
                            probe.CountBranch(context.Behavior.Id.Instance, "b");
                            return Task.CompletedTask;
                        })
                        .AddAction("c", context =>
                        {
                            probe.CountBranch(context.Behavior.Id.Instance, "c");
                            return Task.CompletedTask;
                        }))
                    .AddActivity("recovery-structured", activity => activity
                        .AddInputPin<int>(input => input.AddFlow("scope"))
                        .AddStructuredActivity("scope", scope => (interleave ? scope.Interleave() : scope)
                            .AddOnInitialize(context =>
                            {
                                probe.Count(context.Behavior.Id.Instance, "initialize");
                                return Task.CompletedTask;
                            })
                            .AddOnFinalize(context =>
                            {
                                probe.Count(context.Behavior.Id.Instance, "finalize");
                                return Task.CompletedTask;
                            })
                            .AddInputPin<int>(input => input.AddFlow("first"))
                            .AddAction("first", async context =>
                            {
                                await probe.GatedAsync(context.Behavior.Id.Instance, "first");
                            }, node => node.AddControlFlow("second"))
                            .AddAction("second", context =>
                            {
                                probe.Count(context.Behavior.Id.Instance, "second");
                                return Task.CompletedTask;
                            })
                            .AddControlFlow("after"))
                        .AddAction("after", context =>
                        {
                            probe.Count(context.Behavior.Id.Instance, "after");
                            probe.Done(context.Behavior.Id.Instance);
                            return Task.CompletedTask;
                        }))
                    .AddActivity("recovery-iterative", activity => activity
                        .AddInputPin<int>(input => input.AddFlow("loop"))
                        .AddIterativeActivity<int>("loop", loop => (interleave ? loop.Interleave() : loop)
                            .AddOnInitialize(context =>
                            {
                                probe.Count(context.Behavior.Id.Instance, "initialize");
                                return Task.CompletedTask;
                            })
                            .AddOnFinalize(context =>
                            {
                                probe.Count(context.Behavior.Id.Instance, "finalize");
                                return Task.CompletedTask;
                            })
                            .AddInputPin<int>(input => input.AddFlow("first"))
                            .AddAction("first", async context =>
                            {
                                var value = context.GetTokensOfType<int>().Single();
                                probe.Count(context.Behavior.Id.Instance, $"first:{value}");
                                probe.Count(context.Behavior.Id.Instance, $"selection-first:{value}:{Selection(context)}");
                                if (value == 1)
                                    await probe.GatedAsync(context.Behavior.Id.Instance, "first");
                                context.PassAllTokensOn();
                            }, node => node.AddFlow<int>("second"))
                            .AddAction("second", context =>
                            {
                                probe.Count(context.Behavior.Id.Instance, $"second:{context.GetTokensOfType<int>().Single()}");
                                probe.Count(context.Behavior.Id.Instance, $"selection-second:{context.GetTokensOfType<int>().Single()}:{Selection(context)}");
                                context.PassAllTokensOn();
                                return Task.CompletedTask;
                            }, node => node.AddFlow<int, OutputPin<int>>())
                            .AddOutputPin<int>()
                            .AddFlow<int>("after"))
                        .AddAction("after", context =>
                        {
                            probe.Count(context.Behavior.Id.Instance, "after");
                            probe.Record(context.Behavior.Id.Instance, string.Join(",", context.GetTokensOfType<int>().OrderBy(value => value)));
                            probe.Done(context.Behavior.Id.Instance);
                            return Task.CompletedTask;
                        }))
                    .AddActivity("recovery-parallel", activity => activity
                        .AddInputPin<int>(input => input.AddFlow("parallel"))
                        .AddParallelActivity<int>("parallel", parallel => (interleave ? parallel.Interleave() : parallel)
                            .AddOnInitialize(context =>
                            {
                                probe.Count(context.Behavior.Id.Instance, "initialize");
                                return Task.CompletedTask;
                            })
                            .AddOnFinalize(context =>
                            {
                                probe.Count(context.Behavior.Id.Instance, "finalize");
                                return Task.CompletedTask;
                            })
                            .AddInputPin<int>(input => input.AddFlow("first"))
                            .AddAction("first", async context =>
                            {
                                var value = context.GetTokensOfType<int>().Single();
                                probe.Count(context.Behavior.Id.Instance, $"first:{value}");
                                probe.Count(context.Behavior.Id.Instance, $"selection-first:{value}:{Selection(context)}");
                                await probe.GatedAsync(context.Behavior.Id.Instance, "first", expectedStarts: 3);
                                context.PassAllTokensOn();
                            }, node => node.AddFlow<int>("second"))
                            .AddAction("second", context =>
                            {
                                probe.Count(context.Behavior.Id.Instance, $"second:{context.GetTokensOfType<int>().Single()}");
                                probe.Count(context.Behavior.Id.Instance, $"selection-second:{context.GetTokensOfType<int>().Single()}:{Selection(context)}");
                                context.PassAllTokensOn();
                                return Task.CompletedTask;
                            }, node => node.AddFlow<int, OutputPin<int>>())
                            .AddOutputPin<int>()
                            .AddFlow<int>("after"), 1)
                        .AddAction("after", context =>
                        {
                            probe.Count(context.Behavior.Id.Instance, "after");
                            probe.Record(context.Behavior.Id.Instance, string.Join(",", context.GetTokensOfType<int>().OrderBy(value => value)));
                            probe.Done(context.Behavior.Id.Instance);
                            return Task.CompletedTask;
                        }))));
                services.Configure<StateflowsRuntimeOptions>(options =>
                {
                    options.SuspendActivityCheckpointResumeForTesting = true;
                    options.ActivityCheckpointPauseRequestedForTesting = id => probe.PauseRequested(id.Instance).TrySetResult();
                });
            })
            .ConfigureLogging(logging => logging.SetMinimumLevel(LogLevel.Error))
            .UseOrleans(silo => silo
                .UseLocalhostClustering(ports[0], ports[1], null, cluster, cluster)
                .UseInMemoryReminderService()
                .AddMemoryGrainStorageAsDefault()
                .AddStateflowsRuntime())
            .Build();

        await host.StartAsync();
        try
        {
            var factory = host.Services.GetRequiredService<IBehaviorFactory>();
            var management = host.Services.GetRequiredService<IGrainFactory>().GetGrain<IManagementGrain>(0);
            var cases = new (string Activity, int[] Input, int ExpectedStarts)[]
            {
                ("recovery-flat", [1], 1),
                ("recovery-fork", [1], 1),
                ("recovery-structured", [1], 1),
                ("recovery-iterative", [1, 2, 3], 1),
                ("recovery-parallel", [1, 2, 3], 3),
            };

            foreach (var (activity, input, expectedStarts) in cases)
            {
                var continuous = $"{activity}-continuous";
                var recovered = $"{activity}-recovered";
                var tokens = new TokensInput();
                foreach (var value in input)
                    tokens.Add(value);

                // Continuous reference run within one activation.
                probe.Gate(continuous).TrySetResult();
                using (var reference = factory.CreateBehavior(new ActivityId(activity, continuous)))
                {
                    Assert.AreEqual(EventStatus.Accepted, (await reference.SendAsync(tokens, StreamHeader)
                        .WaitAsync(TimeSpan.FromSeconds(10))).Status, activity);
                    await probe.Completion(continuous).Task.WaitAsync(TimeSpan.FromSeconds(10));
                }

                using var behavior = factory.CreateBehavior(new ActivityId(activity, recovered));
                Assert.AreEqual(EventStatus.Accepted, (await behavior.SendAsync(tokens, StreamHeader)
                    .WaitAsync(TimeSpan.FromSeconds(10))).Status, activity);
                await probe.Started(recovered).Task.WaitAsync(TimeSpan.FromSeconds(10));
                Assert.AreEqual(expectedStarts, probe.Get(recovered, "first"), activity);

                async Task<EventStatus> TakeCheckpointAsync()
                {
                    if (standalone)
                        return (await behavior.SendAsync(new ActivityCheckpoint())).Status;
                    return (await behavior.GetStatusAsync(new Dictionary<string, EventHeader>
                    {
                        [nameof(Checkpoint)] = new Checkpoint()
                    })).Status;
                }
                var checkpoint = TakeCheckpointAsync();
                await probe.PauseRequested(recovered).Task.WaitAsync(TimeSpan.FromSeconds(10));
                Assert.IsFalse(checkpoint.IsCompleted, $"{activity}: the barrier must wait for the gated firing to drain.");
                probe.Gate(recovered).TrySetResult();
                Assert.AreEqual(EventStatus.Consumed,
                    await checkpoint.WaitAsync(TimeSpan.FromSeconds(10)), activity);

                // The snapshot holds the drained firing's outputs as ready work; its successors have not started.
                var snapshot = probe.Snapshot(recovered);
                Assert.IsFalse(snapshot.Keys.Any(key => key.StartsWith("second") || key is "b" or "c" or "after"),
                    $"{activity}: successors must not start at the checkpoint barrier: {string.Join(";", snapshot)}");

                var collectionDeadline = DateTime.UtcNow + TimeSpan.FromSeconds(10);
                int remainingActivations;
                do
                {
                    await management.ForceActivationCollection(TimeSpan.Zero);
                    remainingActivations = (await management.GetSimpleGrainStatistics())
                        .Where(statistic => statistic.GrainType.Contains(nameof(StateflowsBehavior)))
                        .Sum(statistic => statistic.ActivationCount);
                    if (remainingActivations > 0)
                        await Task.Delay(50);
                } while (remainingActivations > 0 && DateTime.UtcNow < collectionDeadline);
                Assert.AreEqual(0, remainingActivations, $"{activity}: the activation must be collected.");

                // Reactivation automatically resumes checkpointed ready work before serving the call.
                await behavior.GetStatusAsync().WaitAsync(TimeSpan.FromSeconds(10));
                await probe.Completion(recovered).Task.WaitAsync(TimeSpan.FromSeconds(10));

                CollectionAssert.AreEquivalent(
                    probe.Snapshot(continuous).Select(entry => $"{entry.Key}={entry.Value}").ToArray(),
                    probe.Snapshot(recovered).Select(entry => $"{entry.Key}={entry.Value}").ToArray(),
                    $"{activity}: recovered execution must match a continuous run (no replay, no lost work).");
                Assert.AreEqual(probe.Recorded(continuous), probe.Recorded(recovered), $"{activity}: accumulated output.");
            }

            for (var value = 1; value <= 3; value++)
            {
                Assert.AreEqual(1, probe.Get("recovery-iterative-recovered", $"first:{value}"));
                Assert.AreEqual(1, probe.Get("recovery-iterative-recovered", $"second:{value}"));
                Assert.AreEqual(1, probe.Get("recovery-parallel-recovered", $"second:{value}"));
            }
            foreach (var activity in new[] { "recovery-iterative", "recovery-parallel" })
            {
                for (var value = 1; value <= 3; value++)
                {
                    // Typed token-context consumers see the partition selection, also on resumed inner firings.
                    Assert.AreEqual(1, probe.Get($"{activity}-recovered", $"selection-first:{value}:{value}"), activity);
                    Assert.AreEqual(1, probe.Get($"{activity}-recovered", $"selection-second:{value}:{value}"), activity);
                    Assert.AreEqual(1, probe.Get($"{activity}-continuous", $"selection-second:{value}:{value}"), activity);
                }
            }
            Assert.AreEqual("1,2,3", probe.Recorded("recovery-iterative-recovered"));
            Assert.AreEqual("1,2,3", probe.Recorded("recovery-parallel-recovered"));
            Assert.AreEqual(1, probe.Get("recovery-structured-recovered", "initialize"));
            Assert.AreEqual(1, probe.Get("recovery-iterative-recovered", "finalize"));
            Assert.AreEqual(1, probe.Get("recovery-fork-recovered", "b"));
            Assert.AreEqual(1, probe.Get("recovery-fork-recovered", "c"));
        }
        finally
        {
            probe.ReleaseAll();
            await host.StopAsync();
        }
    }

    [TestMethod]
    [Timeout(60_000)]
    public async Task RegularSendHandsOffMixedAcceptanceWithoutWaitingStreamedDescendant()
    {
        var probe = new RecoveryProbe();
        var ports = GetPorts();
        var cluster = Guid.NewGuid().ToString("N");
        using var host = Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
            {
                services.AddSingleton(probe);
                services.AddStateflows(sf => sf.AddActivities(activities => activities
                    .AddActivity("handoff", activity => activity
                        .AddInitial(initial => initial.AddControlFlow("accept"))
                        .AddInputPin<int>(input => input.AddFlow("join").AddFlow("keepalive"))
                        .AddAcceptEventAction<HandoffEvent>("accept", context =>
                        {
                            probe.Count(context.Behavior.Id.Instance, "accept");
                            return Task.CompletedTask;
                        }, node => node.AddControlFlow("join"))
                        .AddAction("join", async context =>
                        {
                            probe.Count(context.Behavior.Id.Instance, "join");
                            probe.Record(context.Behavior.Id.Instance, string.Join(",", context.GetTokensOfType<int>()));
                            await probe.Gate("join").Task;
                            probe.Count(context.Behavior.Id.Instance, "join-done");
                        })
                        .AddAction("keepalive", async context =>
                        {
                            probe.Count(context.Behavior.Id.Instance, "keepalive");
                            await probe.Gate("keepalive").Task;
                            probe.Count(context.Behavior.Id.Instance, "keepalive-done");
                            probe.Done(context.Behavior.Id.Instance);
                        }))));
            })
            .ConfigureLogging(logging => logging.SetMinimumLevel(LogLevel.Error))
            .UseOrleans(silo => silo
                .UseLocalhostClustering(ports[0], ports[1], null, cluster, cluster)
                .UseInMemoryReminderService()
                .AddMemoryGrainStorageAsDefault()
                .AddStateflowsRuntime())
            .Build();

        await host.StartAsync();
        try
        {
            using var behavior = host.Services.GetRequiredService<IBehaviorFactory>()
                .CreateBehavior(new ActivityId("handoff", "mixed"));
            Assert.AreEqual(EventStatus.Accepted, (await behavior.SendAsync(new TokensInput().Add(5), StreamHeader)
                .WaitAsync(TimeSpan.FromSeconds(10))).Status);
            await probe.WaitForCountAsync("mixed", "keepalive", 1);
            Assert.AreEqual(0, probe.Get("mixed", "join"), "The join waits for its regular contribution.");

            // The regular event's transactional responsibility ends at the mixed acceptance.
            var regular = await behavior.SendAsync(new HandoffEvent()).WaitAsync(TimeSpan.FromSeconds(10));
            Assert.AreEqual(EventStatus.Consumed, regular.Status);
            Assert.AreEqual(1, probe.Get("mixed", "accept"));
            await probe.WaitForCountAsync("mixed", "join", 1);
            Assert.AreEqual("5", probe.Recorded("mixed"), "The streamed token joins the regular contribution.");
            Assert.AreEqual(0, probe.Get("mixed", "join-done"), "The regular send must complete before the downstream gate opens.");
            Assert.AreEqual(0, probe.Get("mixed", "keepalive-done"), "The pure streamed branch remains live.");

            probe.Gate("join").TrySetResult();
            await probe.WaitForCountAsync("mixed", "join-done", 1);
            probe.Gate("keepalive").TrySetResult();
            await probe.Completion("mixed").Task.WaitAsync(TimeSpan.FromSeconds(10));
        }
        finally
        {
            probe.ReleaseAll();
            await host.StopAsync();
        }
    }

    private static string Selection(Stateflows.Activities.Context.Interfaces.IActionContext context)
        => context is Stateflows.Activities.Context.Interfaces.ITokenContext<object> { Tokens: { } tokens }
            ? string.Join(",", tokens)
            : "none";

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

    private sealed class HandoffEvent;

    private sealed class RecoveryProbe
    {
        private readonly ConcurrentDictionary<string, int> counts = new();
        private readonly ConcurrentDictionary<string, string> records = new();
        private readonly ConcurrentDictionary<string, TaskCompletionSource> signals = new();

        private TaskCompletionSource Signal(string key)
            => signals.GetOrAdd(key, _ => new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously));

        public TaskCompletionSource Gate(string instance) => Signal($"gate:{instance}");
        public TaskCompletionSource Started(string instance) => Signal($"started:{instance}");
        public TaskCompletionSource Completion(string instance) => Signal($"done:{instance}");
        public TaskCompletionSource PauseRequested(string instance) => Signal($"pause:{instance}");

        public int Count(string instance, string key) => counts.AddOrUpdate($"{instance}|{key}", 1, (_, value) => value + 1);

        public int Get(string instance, string key) => counts.TryGetValue($"{instance}|{key}", out var value) ? value : 0;

        public void Record(string instance, string value) => records[instance] = value;

        public string? Recorded(string instance) => records.TryGetValue(instance, out var value) ? value : null;

        public void Done(string instance) => Completion(instance).TrySetResult();

        public Dictionary<string, int> Snapshot(string instance)
            => counts.Where(entry => entry.Key.StartsWith($"{instance}|"))
                .ToDictionary(entry => entry.Key[(instance.Length + 1)..], entry => entry.Value);

        public async Task GatedAsync(string instance, string key, int expectedStarts = 1)
        {
            if (Count(instance, key) >= expectedStarts)
                Started(instance).TrySetResult();
            await Gate(instance).Task;
        }

        public void CountBranch(string instance, string key)
        {
            Count(instance, key);
            if (Get(instance, "b") > 0 && Get(instance, "c") > 0)
                Done(instance);
        }

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