using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Stateflows;
using Stateflows.Activities;
using Stateflows.Activities.Streams;
using Stateflows.Common;
using Stateflows.Common.Interfaces;
using Stateflows.StateMachines;
using System.Threading.Channels;

namespace Orleans.IntegrationTests;

[TestClass]
public sealed class ActivityStreamingTests
{
    [TestMethod]
    [DataRow("state-machine")]
    [DataRow("wrong-payload")]
    [DataRow("request")]
    [DataRow("checkpoint")]
    [DataRow("checkpoint-state-machine")]
    [Timeout(30_000)]
    public async Task StreamingDeliveryValidationDoesNotInitializeBehavior(string delivery)
    {
        var initializations = 0;
        var executions = 0;
        var ports = GetPorts();
        var cluster = Guid.NewGuid().ToString("N");
        using var host = Host.CreateDefaultBuilder()
            .ConfigureServices(services => services.AddStateflows(sf => sf
                .AddActivities(activities => activities.AddActivity("stream-validation", activity => activity
                    .AddDefaultInitializer(_ =>
                    {
                        Interlocked.Increment(ref initializations);
                        return Task.FromResult(true);
                    })
                    .AddInputPin<int>(input => input.AddFlow("consume"))
                    .AddAction("consume", _ =>
                    {
                        Interlocked.Increment(ref executions);
                        return Task.CompletedTask;
                    })))
                .AddStateMachines(machines => machines.AddStateMachine("stream-validation", machine => machine
                    .AddInitialState("idle", state => state.AddOnEntry(_ =>
                    {
                        Interlocked.Increment(ref initializations);
                        return Task.CompletedTask;
                    }))))))
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
            BehaviorId id = delivery is "state-machine" or "checkpoint-state-machine"
                ? new BehaviorId(BehaviorType.StateMachine, "stream-validation", "invalid")
                : new ActivityId("stream-validation", "invalid");
            using var behavior = host.Services.GetRequiredService<IBehaviorFactory>().CreateBehavior(id);
            var headers = new Dictionary<string, EventHeader>
            {
                [nameof(Stateflows.Common.Stream)] = new Stateflows.Common.Stream()
            };
            EventStatus status;
            if (delivery is "checkpoint" or "checkpoint-state-machine")
                status = (await behavior.SendAsync(new ActivityCheckpoint()).WaitAsync(TimeSpan.FromSeconds(10))).Status;
            else if (delivery == "request")
            {
                var result = await behavior.RequestAsync(new TokensInput().Add(42), headers)
                    .WaitAsync(TimeSpan.FromSeconds(10));
                status = result.Status;
                Assert.IsNull(result.Response, "Admission rejection must not fabricate a TokensOutput response.");
            }
            else if (delivery == "wrong-payload")
                status = (await behavior.SendAsync(new Initialize(), headers).WaitAsync(TimeSpan.FromSeconds(10))).Status;
            else
                status = (await behavior.SendAsync(new TokensInput().Add(42), headers).WaitAsync(TimeSpan.FromSeconds(10))).Status;

            Assert.AreEqual(delivery == "checkpoint" ? EventStatus.Consumed : EventStatus.Rejected, status);
            var info = await behavior.GetStatusAsync(new Dictionary<string, EventHeader>
            {
                [nameof(NoImplicitInitialization)] = new NoImplicitInitialization()
            }).WaitAsync(TimeSpan.FromSeconds(10));
            Assert.AreEqual(BehaviorStatus.NotInitialized, info.Response!.BehaviorStatus);
            Assert.AreEqual(0, System.Threading.Volatile.Read(ref initializations));
            Assert.AreEqual(0, System.Threading.Volatile.Read(ref executions));
        }
        finally
        {
            await host.StopAsync();
        }
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    [Timeout(30_000)]
    public async Task CheckpointTimeoutReleasesBarrierAndResumesReadyWork(bool standalone)
    {
        var probe = new StreamProbe();
        var ports = GetPorts();
        var cluster = Guid.NewGuid().ToString("N");
        using var host = Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
            {
                services.AddSingleton(probe);
                services.AddStateflows(sf => sf.AddActivities(activities =>
                    activities.AddActivity("streaming-checkpoint-timeout", activity => activity
                        .AddInputPin<int>(input => input.AddFlow("first"))
                        .AddAction("first", async context =>
                        {
                            probe.CheckpointFirstStarted.TrySetResult();
                            await probe.CheckpointFirstGate.Task;
                            context.PassAllTokensOn();
                        }, node => node.AddFlow<int>("second"))
                        .AddAction("second", _ =>
                        {
                            probe.CheckpointSecondStarted.TrySetResult();
                            return Task.CompletedTask;
                        }))));
                services.Configure<StateflowsRuntimeOptions>(options =>
                    options.ActivityStreamBarrierTimeout = TimeSpan.FromMilliseconds(100));
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
                .CreateBehavior(new ActivityId("streaming-checkpoint-timeout", "barrier"));
            Assert.AreEqual(EventStatus.Accepted,
                (await behavior.SendAsync(new TokensInput().Add(1),
                    new Dictionary<string, EventHeader> { [nameof(Stateflows.Common.Stream)] = new Stateflows.Common.Stream() })
                    .WaitAsync(TimeSpan.FromSeconds(10))).Status);
            await probe.CheckpointFirstStarted.Task.WaitAsync(TimeSpan.FromSeconds(10));
            var checkpointTimedOut = false;
            try
            {
                if (standalone)
                    await behavior.SendAsync(new ActivityCheckpoint()).WaitAsync(TimeSpan.FromSeconds(10));
                else
                    await behavior.GetStatusAsync(new Dictionary<string, EventHeader>
                    {
                        [nameof(Checkpoint)] = new Checkpoint()
                    }).WaitAsync(TimeSpan.FromSeconds(10));
            }
            catch (TimeoutException)
            {
                checkpointTimedOut = true;
            }
            Assert.IsTrue(checkpointTimedOut, "The checkpointing call must fail when its configured barrier timeout expires.");

            probe.CheckpointFirstGate.TrySetResult();
            await probe.CheckpointSecondStarted.Task.WaitAsync(TimeSpan.FromSeconds(10));
        }
        finally
        {
            probe.CheckpointFirstGate.TrySetResult();
            probe.CheckpointSecondGate.TrySetResult();
            await host.StopAsync();
        }
    }

    [TestMethod]
    [Timeout(60_000)]
    public async Task StreamInputAcknowledgesAdmissionBeforeGatedActionCompletes()
    {
        var probe = new StreamProbe();
        var ports = GetPorts();
        var cluster = Guid.NewGuid().ToString("N");
        using var host = Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
            {
                services.AddSingleton(probe);
                services.AddStateflows(sf => sf.AddActivities(activities =>
                {
                    activities
                        .AddActivity(
                            "streaming-test",
                            activity => activity
                                .AddInputPin<int>(input => input.AddFlow("work"))
                                .AddAction("work", async context =>
                                {
                                    var value = context.GetTokensOfType<int>().Single();
                                    probe.StartedFor(value).TrySetResult();
                                    if (value == 12)
                                        throw new InvalidOperationException("Unrecoverable streamed firing.");
                                    if (value == 99)
                                    {
                                        probe.CancellationStartedFor(context.Behavior.Id.Instance).TrySetResult();
                                        await Task.Delay(Timeout.InfiniteTimeSpan, context.CancellationToken);
                                    }
                                    else
                                    {
                                        await probe.GateFor(value).Task;
                                    }
                                    Interlocked.Increment(ref probe.Completed);
                                }, node => node.Reentrant())
                                .AddFinalizer(_ =>
                                {
                                    probe.FinalizedFor(_.Behavior.Id.Instance).TrySetResult();
                                    return Task.CompletedTask;
                                }))
                        .AddActivity(
                            "streaming-typed-init",
                            activity => activity
                                .AddInitializer<Initialize>(_ => Task.FromResult(true))
                                .AddInputPin<int>(input => input.AddFlow("work"))
                                .AddAction("work", _ => Task.CompletedTask))
                        .AddActivity(
                            "streaming-steering",
                            activity => activity
                                .AddInitial(initial => initial.AddControlFlow("steer"))
                                .AddInputPin<int>(input => input.AddFlow("work"))
                                .AddAction("work", async context =>
                                {
                                    probe.SteeringStarted.TrySetResult();
                                    await probe.SteeringGate.Task;
                                    var (found, value) = await context.Behavior.Values.TryGetAsync<bool>("steered");
                                    probe.ObservedSteering.TrySetResult(found && value);
                                })
                                .AddAcceptEventAction<SteerEvent>("steer", async context =>
                                {
                                    await context.Behavior.Values.SetAsync("steered", true);
                                    var (found, value) = await context.Behavior.Values.TryGetAsync<bool>("steered");
                                    probe.SteeringWriteObserved.TrySetResult(found && value);
                                    probe.SteeringEventHandled.TrySetResult();
                                }))
                        .AddActivity(
                            "streaming-default-init",
                            activity => activity
                                .AddDefaultInitializer(async context =>
                                {
                                    if (context.Behavior.Id.Instance == "slow")
                                    {
                                        probe.InitializerStartedFor("slow").TrySetResult();
                                        await probe.InitializerGate.Task;
                                    }
                                    else if (context.Behavior.Id.Instance == "failing")
                                    {
                                        throw new InvalidOperationException("Initializer failed.");
                                    }

                                    return true;
                                })
                                .AddInputPin<int>(input => input.AddFlow("work"))
                                .AddAction("work", _ => Task.CompletedTask))
                        .AddActivity(
                            "streaming-live-timer",
                            activity => activity
                                .AddTimeEventAction<QuickStreamDelay>("timer",
                                    _ =>
                                    {
                                        probe.LiveTimerFired.TrySetResult();
                                        return Task.CompletedTask;
                                    })
                                .AddInputPin<int>(input => input.AddFlow("work"))
                                .AddAction("work", async _ =>
                                {
                                    probe.LiveTimerWorkStarted.TrySetResult();
                                    await probe.LiveTimerGate.Task;
                                }))
                        .AddActivity(
                            "streaming-publications",
                            activity => activity
                                .AddInputPin<int>(input => input.AddFlow("work"))
                                .AddAction("work", async context =>
                                {
                                    var value = context.GetTokensOfType<int>().Single();
                                    if (value == 1)
                                    {
                                        probe.PublicationStreamStarted.TrySetResult();
                                        await probe.PublicationStreamGate.Task;
                                        var (found, stored) = await context.Behavior.Values.TryGetAsync<bool>("failed-regular-effect");
                                        probe.FailedRegularEffectObserved.TrySetResult(found && stored);
                                    }
                                    else
                                    {
                                        context.Behavior.Publish(new StreamNotice { Value = value });
                                        probe.PublicationStartedFor(value).TrySetResult();
                                        if (value == 2)
                                            await probe.RegularPublicationGate.Task;
                                        else if (value == 3)
                                        {
                                            await context.Behavior.Values.SetAsync("failed-regular-effect", true);
                                            await probe.FailedPublicationGate.Task;
                                            throw new InvalidOperationException("Regular streamed-session event failed.");
                                        }
                                        else if (value == 4)
                                            await probe.MixedPublicationGate.Task;
                                    }
                                }, node => node.Reentrant()))
                        .AddActivity(
                            "streaming-checkpoint",
                            activity => activity
                                .AddOutputPin<int>()
                                .AddInputPin<int>(input => input.AddFlow("first"))
                                .AddAction("first", async context =>
                                {
                                    Interlocked.Increment(ref probe.CheckpointFirstExecutions);
                                    probe.CheckpointFirstStarted.TrySetResult();
                                    await probe.CheckpointFirstGate.Task;
                                    context.PassAllTokensOn();
                                }, node => node.AddFlow<int>("second"))
                                .AddAction("second", async context =>
                                {
                                    Interlocked.Increment(ref probe.CheckpointSecondExecutions);
                                    probe.CheckpointSecondStarted.TrySetResult();
                                    await probe.CheckpointSecondGate.Task;
                                    context.PassAllTokensOn();
                                }, node => node.AddFlow<int, OutputPin<int>>()))
                        .AddActivity(
                            "streaming-checkpoint-structured",
                            activity => activity
                                .AddInputPin<int>(input => input.AddFlow("scope"))
                                .AddStructuredActivity("scope", scope => scope
                                    .AddInputPin<int>(input => input.AddFlow("first"))
                                    .AddInitial(initial => initial.AddControlFlow("first"))
                                    .AddAction("first", async context =>
                                    {
                                        Interlocked.Increment(ref probe.StructuredFirstExecutions);
                                        probe.StructuredFirstStarted.TrySetResult();
                                        await probe.StructuredFirstGate.Task;
                                    }, node => node.AddControlFlow("second"))
                                    .AddAction("second", async context =>
                                    {
                                        Interlocked.Increment(ref probe.StructuredSecondExecutions);
                                        probe.StructuredSecondStarted.TrySetResult();
                                        await probe.StructuredSecondGate.Task;
                                    })))
                        .AddActivity(
                            "streaming-checkpoint-iterative",
                            activity => activity
                                .AddInputPin<int>(input => input.AddFlow("loop"))
                                .AddIterativeActivity<int>("loop", loop => loop
                                    .AddInputPin<int>(input => input.AddFlow("first"))
                                    .AddInitial(initial => initial.AddControlFlow("first"))
                                    .AddAction("first", async _ =>
                                    {
                                        Interlocked.Increment(ref probe.IterativeFirstExecutions);
                                        probe.IterativeFirstStarted.TrySetResult();
                                        await probe.IterativeFirstGate.Task;
                                    }, node => node.AddControlFlow("second"))
                                    .AddAction("second", async _ =>
                                    {
                                        Interlocked.Increment(ref probe.IterativeSecondExecutions);
                                        probe.IterativeSecondStarted.TrySetResult();
                                        await probe.IterativeSecondGate.Task;
                                    })))
                        .AddActivity(
                            "streaming-checkpoint-parallel",
                            activity => activity
                                .AddInputPin<int>(input => input.AddFlow("parallel"))
                                .AddParallelActivity<int>("parallel", parallel => parallel
                                    .AddInputPin<int>(input => input.AddFlow("first"))
                                    .AddInitial(initial => initial.AddControlFlow("first"))
                                    .AddAction("first", async _ =>
                                    {
                                        if (Interlocked.Increment(ref probe.ParallelFirstExecutions) == 2)
                                            probe.ParallelFirstStarted.TrySetResult();
                                        await probe.ParallelFirstGate.Task;
                                    }, node => node.AddControlFlow("second"))
                                    .AddAction("second", async _ =>
                                    {
                                        if (Interlocked.Increment(ref probe.ParallelSecondExecutions) == 2)
                                            probe.ParallelSecondStarted.TrySetResult();
                                        await probe.ParallelSecondGate.Task;
                                    }), 1))
                        .AddActivity(
                            "streaming-checkpoint-steering",
                            activity => activity
                                .AddInitial(initial => initial.AddControlFlow("steer"))
                                .AddInputPin<int>(input => input.AddFlow("first"))
                                .AddAction("first", async context =>
                                {
                                    probe.CheckpointSteeringFirstStarted.TrySetResult();
                                    await probe.CheckpointSteeringFirstGate.Task;
                                    context.PassAllTokensOn();
                                }, node => node.AddFlow<int>("second"))
                                .AddAction("second", async context =>
                                {
                                    var (found, value) = await context.Behavior.Values.TryGetAsync<bool>("checkpoint-steered");
                                    probe.CheckpointSteeringObserved.TrySetResult(found && value);
                                })
                                .AddAcceptEventAction<SteerEvent>("steer", async context =>
                                {
                                    await context.Behavior.Values.SetAsync("checkpoint-steered", true);
                                    probe.CheckpointSteerHandled.TrySetResult();
                                }));
                }));
                services.Configure<StateflowsRuntimeOptions>(options => options.MaxQueuedActivityStreamEvents = 3);
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
                .CreateBehavior(new ActivityId("streaming-test", "admission"));
            using var typedInitializationBehavior = host.Services.GetRequiredService<IBehaviorFactory>()
                .CreateBehavior(new ActivityId("streaming-typed-init", "cold"));
            using var delayedInitializationBehavior = host.Services.GetRequiredService<IBehaviorFactory>()
                .CreateBehavior(new ActivityId("streaming-default-init", "slow"));
            using var failingInitializationBehavior = host.Services.GetRequiredService<IBehaviorFactory>()
                .CreateBehavior(new ActivityId("streaming-default-init", "failing"));
            var typedInitializationAdmission = await typedInitializationBehavior.SendAsync(new TokensInput().Add(1),
                new Dictionary<string, EventHeader> { [nameof(Stateflows.Common.Stream)] = new Stateflows.Common.Stream() });
            Assert.AreEqual(EventStatus.Rejected, typedInitializationAdmission.Status);
            Assert.AreEqual(EventStatus.Initialized, (await typedInitializationBehavior.SendAsync(new Initialize()).WaitAsync(TimeSpan.FromSeconds(10))).Status);
            Assert.AreEqual(EventStatus.Accepted, (await typedInitializationBehavior.SendAsync(new TokensInput().Add(1),
                new Dictionary<string, EventHeader> { [nameof(Stateflows.Common.Stream)] = new Stateflows.Common.Stream() })
                .WaitAsync(TimeSpan.FromSeconds(10))).Status);
            var delayedInitialization = delayedInitializationBehavior.SendAsync(new TokensInput().Add(1),
                new Dictionary<string, EventHeader> { [nameof(Stateflows.Common.Stream)] = new Stateflows.Common.Stream() });
            await probe.InitializerStartedFor("slow").Task.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.IsFalse(delayedInitialization.IsCompleted, "Stream admission must wait for successful default initialization.");
            probe.InitializerGate.TrySetResult();
            Assert.AreEqual(EventStatus.Accepted,
                (await delayedInitialization.WaitAsync(TimeSpan.FromSeconds(10))).Status);
            Assert.AreEqual(EventStatus.Rejected, (await failingInitializationBehavior.SendAsync(new TokensInput().Add(1),
                new Dictionary<string, EventHeader> { [nameof(Stateflows.Common.Stream)] = new Stateflows.Common.Stream() })
                .WaitAsync(TimeSpan.FromSeconds(10))).Status);
            using var liveTimerBehavior = host.Services.GetRequiredService<IBehaviorFactory>()
                .CreateBehavior(new ActivityId("streaming-live-timer", "live-timer"));
            Assert.AreEqual(EventStatus.Accepted, (await liveTimerBehavior.SendAsync(new TokensInput().Add(1),
                new Dictionary<string, EventHeader> { [nameof(Stateflows.Common.Stream)] = new Stateflows.Common.Stream() })
                .WaitAsync(TimeSpan.FromSeconds(10))).Status);
            await probe.LiveTimerWorkStarted.Task.WaitAsync(TimeSpan.FromSeconds(10));
            await probe.LiveTimerFired.Task.WaitAsync(TimeSpan.FromSeconds(10));
            probe.LiveTimerGate.TrySetResult();
            var admission = await behavior.SendAsync(new TokensInput().Add(1),
                new Dictionary<string, EventHeader> { [nameof(Stateflows.Common.Stream)] = new Stateflows.Common.Stream() })
                .WaitAsync(TimeSpan.FromSeconds(10));
            Assert.AreEqual(EventStatus.Accepted, admission.Status);
            await probe.Started.Task.WaitAsync(TimeSpan.FromSeconds(10));

            var second = await behavior.SendAsync(new TokensInput().Add(2),
                new Dictionary<string, EventHeader> { [nameof(Stateflows.Common.Stream)] = new Stateflows.Common.Stream() });
            Assert.AreEqual(EventStatus.Accepted, second.Status);
            await probe.StartedFor(2).Task.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.AreEqual(0, System.Threading.Volatile.Read(ref probe.Completed));

            var tokenRequest = await behavior.RequestAsync<TokensOutput>(new TokensInput().Add(3));
            Assert.AreEqual(EventStatus.Rejected, tokenRequest.Status);
            Assert.AreEqual(0, System.Threading.Volatile.Read(ref probe.Completed));

            var checkpoint = behavior.GetStatusAsync(new Dictionary<string, EventHeader>
            {
                [nameof(Checkpoint)] = new Checkpoint()
            });
            await Task.Delay(50);
            Assert.IsFalse(checkpoint.IsCompleted, "A checkpoint must wait for current stream firings.");
            var nextEpoch = await behavior.SendAsync(new TokensInput().Add(4),
                new Dictionary<string, EventHeader> { [nameof(Stateflows.Common.Stream)] = new Stateflows.Common.Stream() });
            Assert.AreEqual(EventStatus.Accepted, nextEpoch.Status);
            var overloadedBatch = await behavior.SendAsync(new TokensInput().Add(5).Add(6),
                new Dictionary<string, EventHeader> { [nameof(Stateflows.Common.Stream)] = new Stateflows.Common.Stream() });
            Assert.AreEqual(EventStatus.Overloaded, overloadedBatch.Status);

            probe.GateFor(1).TrySetResult();
            probe.GateFor(2).TrySetResult();
            Assert.AreEqual(EventStatus.Consumed, (await checkpoint.WaitAsync(TimeSpan.FromSeconds(10))).Status);
            probe.GateFor(4).TrySetResult();
            await probe.StartedFor(4).Task.WaitAsync(TimeSpan.FromSeconds(10));
            await WaitForCountAsync(probe, 3);
            Assert.AreEqual(3, System.Threading.Volatile.Read(ref probe.Completed));

            using var checkpointActivity = host.Services.GetRequiredService<IBehaviorFactory>()
                .CreateBehavior(new ActivityId("streaming-checkpoint", "ready-state"));
            Assert.AreEqual(EventStatus.Accepted, (await checkpointActivity.SendAsync(new TokensInput().Add(41),
                new Dictionary<string, EventHeader> { [nameof(Stateflows.Common.Stream)] = new Stateflows.Common.Stream() })
                .WaitAsync(TimeSpan.FromSeconds(10))).Status);
            await probe.CheckpointFirstStarted.Task.WaitAsync(TimeSpan.FromSeconds(10));
            var firingBoundaryCheckpoint = checkpointActivity.GetStatusAsync(new Dictionary<string, EventHeader>
            {
                [nameof(Checkpoint)] = new Checkpoint()
            });
            await Task.Delay(50);
            probe.CheckpointFirstGate.TrySetResult();
            Assert.AreEqual(EventStatus.Consumed,
                (await firingBoundaryCheckpoint.WaitAsync(TimeSpan.FromSeconds(10))).Status);
            await probe.CheckpointSecondStarted.Task.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.AreEqual(1, System.Threading.Volatile.Read(ref probe.CheckpointFirstExecutions),
                "Resuming a checkpointed ready firing must not replay its completed predecessor.");
            Assert.AreEqual(1, System.Threading.Volatile.Read(ref probe.CheckpointSecondExecutions));
            probe.CheckpointSecondGate.TrySetResult();

            using var structuredCheckpointActivity = host.Services.GetRequiredService<IBehaviorFactory>()
                .CreateBehavior(new ActivityId("streaming-checkpoint-structured", "ready-state"));
            Assert.AreEqual(EventStatus.Accepted,
                (await structuredCheckpointActivity.SendAsync(new TokensInput().Add(42),
                    new Dictionary<string, EventHeader> { [nameof(Stateflows.Common.Stream)] = new Stateflows.Common.Stream() })
                    .WaitAsync(TimeSpan.FromSeconds(10))).Status);
            await probe.StructuredFirstStarted.Task.WaitAsync(TimeSpan.FromSeconds(10));
            var structuredCheckpoint = structuredCheckpointActivity.GetStatusAsync(new Dictionary<string, EventHeader>
            {
                [nameof(Checkpoint)] = new Checkpoint()
            });
            await Task.Delay(50);
            probe.StructuredFirstGate.TrySetResult();
            Assert.AreEqual(EventStatus.Consumed,
                (await structuredCheckpoint.WaitAsync(TimeSpan.FromSeconds(10))).Status);
            await probe.StructuredSecondStarted.Task.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.AreEqual(1, System.Threading.Volatile.Read(ref probe.StructuredFirstExecutions));
            Assert.AreEqual(1, System.Threading.Volatile.Read(ref probe.StructuredSecondExecutions));
            probe.StructuredSecondGate.TrySetResult();

            using var iterativeCheckpointActivity = host.Services.GetRequiredService<IBehaviorFactory>()
                .CreateBehavior(new ActivityId("streaming-checkpoint-iterative", "ready-state"));
            Assert.AreEqual(EventStatus.Accepted,
                (await iterativeCheckpointActivity.SendAsync(new TokensInput().Add(43),
                    new Dictionary<string, EventHeader> { [nameof(Stateflows.Common.Stream)] = new Stateflows.Common.Stream() })
                    .WaitAsync(TimeSpan.FromSeconds(10))).Status);
            await probe.IterativeFirstStarted.Task.WaitAsync(TimeSpan.FromSeconds(10));
            var iterativeCheckpoint = iterativeCheckpointActivity.GetStatusAsync(new Dictionary<string, EventHeader>
            {
                [nameof(Checkpoint)] = new Checkpoint()
            });
            await Task.Delay(50);
            probe.IterativeFirstGate.TrySetResult();
            Assert.AreEqual(EventStatus.Consumed,
                (await iterativeCheckpoint.WaitAsync(TimeSpan.FromSeconds(10))).Status);
            await probe.IterativeSecondStarted.Task.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.AreEqual(1, System.Threading.Volatile.Read(ref probe.IterativeFirstExecutions));
            Assert.AreEqual(1, System.Threading.Volatile.Read(ref probe.IterativeSecondExecutions));
            probe.IterativeSecondGate.TrySetResult();

            using var parallelCheckpointActivity = host.Services.GetRequiredService<IBehaviorFactory>()
                .CreateBehavior(new ActivityId("streaming-checkpoint-parallel", "ready-state"));
            Assert.AreEqual(EventStatus.Accepted,
                (await parallelCheckpointActivity.SendAsync(new TokensInput().Add(44).Add(46),
                    new Dictionary<string, EventHeader> { [nameof(Stateflows.Common.Stream)] = new Stateflows.Common.Stream() })
                    .WaitAsync(TimeSpan.FromSeconds(10))).Status);
            await probe.ParallelFirstStarted.Task.WaitAsync(TimeSpan.FromSeconds(10));
            var parallelCheckpoint = parallelCheckpointActivity.GetStatusAsync(new Dictionary<string, EventHeader>
            {
                [nameof(Checkpoint)] = new Checkpoint()
            });
            await Task.Delay(50);
            probe.ParallelFirstGate.TrySetResult();
            Assert.AreEqual(EventStatus.Consumed,
                (await parallelCheckpoint.WaitAsync(TimeSpan.FromSeconds(10))).Status);
            await probe.ParallelSecondStarted.Task.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.AreEqual(2, System.Threading.Volatile.Read(ref probe.ParallelFirstExecutions));
            Assert.AreEqual(2, System.Threading.Volatile.Read(ref probe.ParallelSecondExecutions));
            probe.ParallelSecondGate.TrySetResult();

            using var checkpointSteering = host.Services.GetRequiredService<IBehaviorFactory>()
                .CreateBehavior(new ActivityId("streaming-checkpoint-steering", "ready-state"));
            Assert.AreEqual(EventStatus.Accepted,
                (await checkpointSteering.SendAsync(new TokensInput().Add(45),
                    new Dictionary<string, EventHeader> { [nameof(Stateflows.Common.Stream)] = new Stateflows.Common.Stream() })
                    .WaitAsync(TimeSpan.FromSeconds(10))).Status);
            await probe.CheckpointSteeringFirstStarted.Task.WaitAsync(TimeSpan.FromSeconds(10));
            var checkpointedSteerCall = checkpointSteering.SendAsync(new SteerEvent(),
                new Dictionary<string, EventHeader> { [nameof(Checkpoint)] = new Checkpoint() });
            await Task.Delay(50);
            probe.CheckpointSteeringFirstGate.TrySetResult();
            Assert.AreEqual(EventStatus.Consumed,
                (await checkpointedSteerCall.WaitAsync(TimeSpan.FromSeconds(10))).Status);
            await probe.CheckpointSteerHandled.Task.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.IsTrue(await probe.CheckpointSteeringObserved.Task.WaitAsync(TimeSpan.FromSeconds(10)),
                "The checkpointing regular event's committed value must be visible to resumed ready work.");

            using var steering = host.Services.GetRequiredService<IBehaviorFactory>()
                .CreateBehavior(new ActivityId("streaming-steering", "live"));
            Assert.AreEqual(EventStatus.Accepted, (await steering.SendAsync(new TokensInput().Add(7),
                new Dictionary<string, EventHeader> { [nameof(Stateflows.Common.Stream)] = new Stateflows.Common.Stream() })).Status);
            await probe.SteeringStarted.Task.WaitAsync(TimeSpan.FromSeconds(10));
            var steerResult = await steering.SendAsync(new SteerEvent()).WaitAsync(TimeSpan.FromSeconds(10));
            Assert.AreEqual(EventStatus.Consumed, steerResult.Status);
            await probe.SteeringEventHandled.Task.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.IsTrue(await probe.SteeringWriteObserved.Task.WaitAsync(TimeSpan.FromSeconds(10)));
            probe.SteeringGate.TrySetResult();
            Assert.IsTrue(await probe.ObservedSteering.Task.WaitAsync(TimeSpan.FromSeconds(10)),
                "A gated streamed firing must observe values written by a regular event steering the same live Activity.");

            using var publicationBehavior = host.Services.GetRequiredService<IBehaviorFactory>()
                .CreateBehavior(new ActivityId("streaming-publications", "publication"));
            var receivedNotices = Channel.CreateUnbounded<int>();
            await using var publicationWatcher = await publicationBehavior.WatchAsync<StreamNotice>(notice =>
            {
                receivedNotices.Writer.TryWrite(notice.Value);
                return Task.CompletedTask;
            });
            Assert.AreEqual(EventStatus.Accepted, (await publicationBehavior.SendAsync(new TokensInput().Add(1),
                new Dictionary<string, EventHeader> { [nameof(Stateflows.Common.Stream)] = new Stateflows.Common.Stream() })
                .WaitAsync(TimeSpan.FromSeconds(10))).Status);
            await probe.PublicationStreamStarted.Task.WaitAsync(TimeSpan.FromSeconds(10));

            var deferredSuccess = publicationBehavior.SendAsync(new TokensInput().Add(2));
            await probe.PublicationStartedFor(2).Task.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.IsFalse(receivedNotices.Reader.TryRead(out _),
                "A regular-only firing must retain its publication until its transactional call completes.");
            probe.RegularPublicationGate.TrySetResult();
            Assert.AreEqual(EventStatus.Consumed, (await deferredSuccess.WaitAsync(TimeSpan.FromSeconds(10))).Status);
            Assert.AreEqual(2, await receivedNotices.Reader.ReadAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(10)));

            var deferredFailure = publicationBehavior.SendAsync(new TokensInput().Add(3));
            await probe.PublicationStartedFor(3).Task.WaitAsync(TimeSpan.FromSeconds(10));
            probe.FailedPublicationGate.TrySetResult();
            Assert.AreEqual(EventStatus.Failed, (await deferredFailure.WaitAsync(TimeSpan.FromSeconds(10))).Status);
            Assert.IsFalse(receivedNotices.Reader.TryRead(out _),
                "A publication staged by a failed regular-only firing must be discarded.");

            Assert.AreEqual(EventStatus.Accepted, (await publicationBehavior.SendAsync(new TokensInput().Add(4),
                new Dictionary<string, EventHeader> { [nameof(Stateflows.Common.Stream)] = new Stateflows.Common.Stream() })
                .WaitAsync(TimeSpan.FromSeconds(10))).Status);
            await probe.PublicationStartedFor(4).Task.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.AreEqual(4, await receivedNotices.Reader.ReadAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(10)),
                "A mixed/streamed firing must publish immediately without waiting for the action to complete.");
            probe.MixedPublicationGate.TrySetResult();
            probe.PublicationStreamGate.TrySetResult();
            Assert.IsTrue(await probe.FailedRegularEffectObserved.Task.WaitAsync(TimeSpan.FromSeconds(10)),
                "A failed regular event must not roll back its live value effects.");

            using var joined = host.Services.GetRequiredService<IBehaviorFactory>()
                .CreateBehavior(new ActivityId("streaming-test", "regular-join"));
            var regularFiring = joined.SendAsync(new TokensInput().Add(10));
            await probe.StartedFor(10).Task.WaitAsync(TimeSpan.FromSeconds(10));
            var joinedStream = await joined.SendAsync(new TokensInput().Add(11),
                new Dictionary<string, EventHeader> { [nameof(Stateflows.Common.Stream)] = new Stateflows.Common.Stream() })
                .WaitAsync(TimeSpan.FromSeconds(10));
            Assert.AreEqual(EventStatus.Accepted, joinedStream.Status);
            await probe.StartedFor(11).Task.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.IsFalse(regularFiring.IsCompleted,
                "A streamed input must join the retained session while the original ordinary firing is still active.");
            probe.GateFor(10).TrySetResult();
            probe.GateFor(11).TrySetResult();
            await regularFiring.WaitAsync(TimeSpan.FromSeconds(10));
            await WaitForCountAsync(probe, 5);

            using var failureBehavior = host.Services.GetRequiredService<IBehaviorFactory>()
                .CreateBehavior(new ActivityId("streaming-test", "failure-isolation"));
            var failureNotification = new TaskCompletionSource<ActivityStreamFailure>(TaskCreationOptions.RunContinuationsAsynchronously);
            await using var failureWatcher = await failureBehavior.WatchAsync<ActivityStreamFailure>(notification =>
            {
                failureNotification.TrySetResult(notification);
                return Task.CompletedTask;
            });
            Assert.AreEqual(EventStatus.Accepted, (await failureBehavior.SendAsync(new TokensInput().Add(12),
                new Dictionary<string, EventHeader> { [nameof(Stateflows.Common.Stream)] = new Stateflows.Common.Stream() })
                .WaitAsync(TimeSpan.FromSeconds(10))).Status);
            var reportedFailure = await failureNotification.Task.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.AreEqual("failure-isolation", reportedFailure.BehaviorId.Instance);
            Assert.IsTrue(reportedFailure.ExceptionTypes.Any(type => type.Contains(nameof(InvalidOperationException), StringComparison.Ordinal)));
            Assert.AreEqual(EventStatus.Accepted, (await failureBehavior.SendAsync(new TokensInput().Add(13),
                new Dictionary<string, EventHeader> { [nameof(Stateflows.Common.Stream)] = new Stateflows.Common.Stream() })
                .WaitAsync(TimeSpan.FromSeconds(10))).Status);
            await probe.StartedFor(13).Task.WaitAsync(TimeSpan.FromSeconds(10));
            probe.GateFor(13).TrySetResult();
            await WaitForCountAsync(probe, 6);

            using var cancellable = host.Services.GetRequiredService<IBehaviorFactory>()
                .CreateBehavior(new ActivityId("streaming-test", "immediate-finalize"));
            Assert.AreEqual(EventStatus.Accepted, (await cancellable.SendAsync(new TokensInput().Add(99),
                new Dictionary<string, EventHeader> { [nameof(Stateflows.Common.Stream)] = new Stateflows.Common.Stream() })).Status);
            await probe.CancellationStartedFor("immediate-finalize").Task.WaitAsync(TimeSpan.FromSeconds(10));
            var finalize = await cancellable.SendAsync(new Finalize { Mode = FinalizationMode.Immediate })
                .WaitAsync(TimeSpan.FromSeconds(10));
            Assert.AreEqual(EventStatus.Cancelled, finalize.Status);
            await probe.FinalizedFor("immediate-finalize").Task.WaitAsync(TimeSpan.FromSeconds(10));

            using var queuedFinalize = host.Services.GetRequiredService<IBehaviorFactory>()
                .CreateBehavior(new ActivityId("streaming-test", "queued-finalize"));
            Assert.AreEqual(EventStatus.Accepted, (await queuedFinalize.SendAsync(new TokensInput().Add(99),
                new Dictionary<string, EventHeader> { [nameof(Stateflows.Common.Stream)] = new Stateflows.Common.Stream() })).Status);
            await probe.CancellationStartedFor("queued-finalize").Task.WaitAsync(TimeSpan.FromSeconds(10));
            var queuedFinalizeResult = await queuedFinalize.SendAsync(new Finalize { Mode = FinalizationMode.Queued })
                .WaitAsync(TimeSpan.FromSeconds(10));
            Assert.IsTrue(queuedFinalizeResult.Status is EventStatus.Consumed or EventStatus.Cancelled);
            await probe.FinalizedFor("queued-finalize").Task.WaitAsync(TimeSpan.FromSeconds(10));
        }
        finally
        {
            probe.GateFor(99).TrySetResult();
            await host.StopAsync();
        }
    }

    private static async Task WaitForCountAsync(StreamProbe probe, int expected)
    {
        var deadline = DateTime.UtcNow.AddSeconds(10);
        while (System.Threading.Volatile.Read(ref probe.Completed) < expected && DateTime.UtcNow < deadline)
            await Task.Delay(20);
        Assert.AreEqual(expected, System.Threading.Volatile.Read(ref probe.Completed));
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

    private sealed class StreamProbe
    {
        public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly ConcurrentDictionary<int, TaskCompletionSource> gates = new();
        private readonly ConcurrentDictionary<int, TaskCompletionSource> starts = new();
        public TaskCompletionSource SteeringStarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource SteeringGate { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource<bool> ObservedSteering { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource SteeringEventHandled { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource<bool> SteeringWriteObserved { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly ConcurrentDictionary<string, TaskCompletionSource> cancellationStarted = new();
        public TaskCompletionSource Finalized { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource InitializerGate { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource LiveTimerWorkStarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource LiveTimerGate { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource LiveTimerFired { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource PublicationStreamStarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource PublicationStreamGate { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource RegularPublicationGate { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource<bool> FailedRegularEffectObserved { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource FailedPublicationGate { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource MixedPublicationGate { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly ConcurrentDictionary<int, TaskCompletionSource> publicationStarts = new();
        private readonly ConcurrentDictionary<string, TaskCompletionSource> initializers = new();
        private readonly ConcurrentDictionary<string, TaskCompletionSource> finalizers = new();
        public int Completed;
        public int CheckpointFirstExecutions;
        public int CheckpointSecondExecutions;
        public TaskCompletionSource CheckpointFirstStarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource CheckpointFirstGate { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource CheckpointSecondStarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource CheckpointSecondGate { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public int StructuredFirstExecutions;
        public int StructuredSecondExecutions;
        public TaskCompletionSource StructuredFirstStarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource StructuredFirstGate { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource StructuredSecondStarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource StructuredSecondGate { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public int IterativeFirstExecutions;
        public int IterativeSecondExecutions;
        public TaskCompletionSource IterativeFirstStarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource IterativeFirstGate { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource IterativeSecondStarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource IterativeSecondGate { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public int ParallelFirstExecutions;
        public int ParallelSecondExecutions;
        public TaskCompletionSource ParallelFirstStarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource ParallelFirstGate { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource ParallelSecondStarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource ParallelSecondGate { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource CheckpointSteeringFirstStarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource CheckpointSteeringFirstGate { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource<bool> CheckpointSteeringObserved { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource CheckpointSteerHandled { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource GateFor(int value)
            => gates.GetOrAdd(value, _ => new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously));

        public TaskCompletionSource StartedFor(int value)
        {
            var started = starts.GetOrAdd(value, _ => new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously));
            if (value == 1)
                Started.TrySetResult();
            return started;
        }

        public TaskCompletionSource InitializerStartedFor(string instance)
            => initializers.GetOrAdd(instance, _ => new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously));

        public TaskCompletionSource FinalizedFor(string instance)
            => finalizers.GetOrAdd(instance, _ => new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously));

        public TaskCompletionSource CancellationStartedFor(string instance)
            => cancellationStarted.GetOrAdd(instance, _ => new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously));

        public TaskCompletionSource PublicationStartedFor(int value)
            => publicationStarts.GetOrAdd(value, _ => new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously));
    }

    private sealed class SteerEvent;
    private sealed class StreamNotice
    {
        public int Value { get; set; }
    }

    public sealed class QuickStreamDelay : DelayEvent
    {
        protected override TimeSpan Delay => TimeSpan.FromMilliseconds(350);
    }
}
