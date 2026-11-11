using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Stateflows.Activities;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Engine;
using Stateflows.Activities.Registration;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Activities.Streams;
using Stateflows.Common;
using Stateflows.Common.Context;
using Stateflows.Common.Engine;
using Stateflows.Common.Interfaces;

namespace Activity.IntegrationTests.Tests;

[TestClass]
public sealed class CrossingFlows
{
    private static readonly TimeSpan Deadline = TimeSpan.FromSeconds(5);
    private static TaskCompletionSource Signal() => new(TaskCreationOptions.RunContinuationsAsynchronously);

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task IncomingOfferDoesNotInitializeInactiveDestinationAndIsDeliveredOnActivation(bool checkpoint)
    {
        var initialized = false;
        var consumed = Signal();
        var received = 0;
        Harness? harness = null;
        harness = await Harness.CreateAsync(activity => activity
            .AddInitial(initial => initial.AddControlFlow("produce"))
            .AddAction("produce", async context =>
            {
                context.Output(42);
                if (checkpoint)
                    await harness!.Executor.RequestCheckpointPauseAsync();
            }, action => action
                .AddFlow<int>("main")
                .AddControlFlow("activate"))
            .AddAction("activate", context =>
            {
                Assert.IsFalse(initialized);
                Assert.AreEqual(0, received);
                return Task.CompletedTask;
            }, action => action.AddControlFlow("main"))
            .AddStructuredActivity("main", structure => structure
                .AddInputPin<int>(input => input.AddFlow("consume"))
                .AddOnInitialize(context =>
                {
                    initialized = true;
                    return Task.CompletedTask;
                })
                .AddInitial(initial => initial.AddControlFlow("hold"))
                .AddAction("hold", async context => await consumed.Task.WaitAsync(Deadline))
                .AddAction("consume", context =>
                {
                    Assert.IsTrue(initialized);
                    received = context.GetTokensOfType<int>().Single();
                    consumed.TrySetResult();
                    return Task.CompletedTask;
                })));

        await using (harness)
        {
            if (checkpoint)
            {
                await Assert.ThrowsExactlyAsync<ActivityCheckpointPauseException>(() => harness.ExecuteAsync());
                Assert.IsFalse(initialized);
                Assert.AreEqual(0, received);
                await harness.RehydrateAsync();
                await harness.ResumeAsync();
            }
            else
                await harness.ExecuteAsync();
            Assert.AreEqual(42, received);
        }
    }

    [TestMethod]
    public async Task LaterIncomingOfferIsAcceptedWhileDestinationExecutes()
    {
        var started = Signal();
        var consumed = Signal();
        var received = 0;
        await using var harness = await Harness.CreateAsync(activity => activity
            .AddInitial(initial => initial.AddControlFlow("main").AddControlFlow("produce"))
            .AddStructuredActivity("main", structure => structure
                .AddInputPin<int>(_ => { })
                .AddInitial(initial => initial.AddControlFlow("hold"))
                .AddAction("hold", async context =>
                {
                    started.TrySetResult();
                    await consumed.Task.WaitAsync(Deadline);
                })
                .AddAction("consume", context =>
                {
                    received = context.GetTokensOfType<int>().Single();
                    consumed.TrySetResult();
                    return Task.CompletedTask;
                }))
            .AddAction("produce", async context =>
            {
                await started.Task.WaitAsync(Deadline);
                context.Output(7);
            }, action => action.AddFlow<int>("consume")));

        await harness.ExecuteAsync();
        Assert.AreEqual(7, received);
    }

    public sealed class Wake;

    [TestMethod]
    [DataRow("live")]
    [DataRow("rehydrated")]
    [DataRow("runtime")]
    public async Task IncomingToIdleActiveStructureResumesLifecycleAndAggregateOutput(string mode)
    {
        var initialized = 0;
        var finalized = 0;
        var received = 0;
        await using var harness = await Harness.CreateAsync(activity => activity
            .AddInputPin<int>(input => input.AddFlow("produce"))
            .AddInitial(initial => initial.AddControlFlow("main"))
            .AddAction("produce", context =>
            {
                context.PassAllTokensOn();
                return Task.CompletedTask;
            }, action => action.AddFlow<int>("consume"))
            .AddStructuredActivity("main", structure => structure
                .AddOnInitialize(context =>
                {
                    initialized++;
                    return Task.CompletedTask;
                })
                .AddOnFinalize(context =>
                {
                    finalized++;
                    return Task.CompletedTask;
                })
                .AddAcceptEventAction<Wake>("wait", context => Task.CompletedTask)
                .AddAction("consume", context =>
                {
                    context.PassAllTokensOn();
                    return Task.CompletedTask;
                }, action => action.AddFlow<int, OutputPin<int>>().AddControlFlow(FinalNode.Name))
                .AddOutputPin<int>()
                .AddFinal()
                .AddFlow<int>("result"))
            .AddAction("result", context =>
            {
                received = context.GetTokensOfType<int>().Single();
                return Task.CompletedTask;
            }));

        if (mode == "runtime")
            await harness.InitializeBehaviorAsync();
        else
            await harness.ExecuteAsync();
        Assert.AreEqual(1, initialized);
        Assert.AreEqual(0, finalized);
        if (mode == "rehydrated")
            await harness.RehydrateAsync();
        if (mode == "runtime")
            await harness.SendInputAsync(17);
        else
            await harness.InputAsync([new TokenHolder<int> { Payload = 17 }]);
        Assert.AreEqual(1, initialized);
        Assert.AreEqual(1, finalized);
        Assert.AreEqual(17, received);
    }

    [TestMethod]
    public async Task OutgoingWorkEscapesInnerFinalizationAndDoesNotWaitForAggregateOutput()
    {
        var outsideStarted = Signal();
        var innerFinalized = Signal();
        var finalized = false;
        var received = 0;
        await using var harness = await Harness.CreateAsync(activity => activity
            .AddInitial(initial => initial.AddControlFlow("main"))
            .AddStructuredActivity("main", structure => structure
                .AddOnFinalize(context =>
                {
                    finalized = true;
                    innerFinalized.TrySetResult();
                    return Task.CompletedTask;
                })
                .AddInitial(initial => initial.AddControlFlow("emit"))
                .AddAction("emit", context =>
                {
                    context.Output(9);
                    return Task.CompletedTask;
                }, action => action.AddFlow<int>("outside").AddControlFlow("stop"))
                .AddAction("stop", async context =>
                {
                    await outsideStarted.Task.WaitAsync(Deadline);
                }, action => action.AddControlFlow(FinalNode.Name))
                .AddFinal())
            .AddAction("outside", async context =>
            {
                Assert.IsFalse(finalized);
                outsideStarted.TrySetResult();
                await innerFinalized.Task.WaitAsync(Deadline);
                await Task.Yield();
                Assert.IsFalse(((ActionContext)context).NodeScope.CancellationToken.IsCancellationRequested);
                received = context.GetTokensOfType<int>().Single();
            }));

        await harness.ExecuteAsync();
        Assert.AreEqual(9, received);
        Assert.IsTrue(finalized);
    }

    [TestMethod]
    public async Task OutgoingControlFailureIsObservedByDestinationScopeAfterSourceCanFinalize()
    {
        var sourceFinalized = false;
        await using var harness = await Harness.CreateAsync(activity => activity
            .AddInitial(initial => initial.AddControlFlow("main"))
            .AddStructuredActivity("main", structure => structure
                .AddOnFinalize(context =>
                {
                    sourceFinalized = true;
                    return Task.CompletedTask;
                })
                .AddInitial(initial => initial.AddControlFlow("outside")))
            .AddAction("outside", context => throw new InvalidOperationException("outside failure")));

        var exception = await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => harness.ExecuteAsync());
        Assert.AreEqual("outside failure", exception.Message);
        Assert.IsTrue(sourceFinalized);
    }

    [TestMethod]
    public async Task SourceFailureStillDrainsWorkAlreadyAcceptedOutside()
    {
        var outsideStarted = Signal();
        var sourceFailed = Signal();
        var outsideCompleted = false;
        await using var harness = await Harness.CreateAsync(activity => activity
            .AddInitial(initial => initial.AddControlFlow("main"))
            .AddStructuredActivity("main", structure => structure
                .AddInitial(initial => initial.AddControlFlow("outside").AddControlFlow("fail"))
                .AddAction("fail", async context =>
                {
                    await outsideStarted.Task.WaitAsync(Deadline);
                    sourceFailed.TrySetResult();
                    throw new InvalidOperationException("source failure");
                }))
            .AddAction("outside", async context =>
            {
                outsideStarted.TrySetResult();
                await sourceFailed.Task.WaitAsync(Deadline);
                await Task.Yield();
                Assert.IsFalse(((ActionContext)context).NodeScope.CancellationToken.IsCancellationRequested);
                outsideCompleted = true;
            }));

        var exception = await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => harness.ExecuteAsync());
        Assert.AreEqual("source failure", exception.Message);
        Assert.IsTrue(outsideCompleted);
    }

    [TestMethod]
    public async Task SiblingAndNestedDestinationWaitForEveryContainingScopeActivation()
    {
        var outerInitialized = false;
        var innerInitialized = false;
        var received = 0;
        await using var harness = await Harness.CreateAsync(activity => activity
            .AddInitial(initial => initial.AddControlFlow("left"))
            .AddStructuredActivity("left", structure => structure
                .AddInitial(initial => initial.AddControlFlow("emit"))
                .AddAction("emit", context =>
                {
                    context.Output(11);
                    return Task.CompletedTask;
                }, action => action.AddFlow<int>("consume"))
                .AddControlFlow("activateRight"))
            .AddAction("activateRight", context =>
            {
                Assert.IsFalse(outerInitialized);
                Assert.IsFalse(innerInitialized);
                Assert.AreEqual(0, received);
                return Task.CompletedTask;
            }, action => action.AddControlFlow("right"))
            .AddStructuredActivity("right", structure => structure
                .AddOnInitialize(context =>
                {
                    outerInitialized = true;
                    return Task.CompletedTask;
                })
                .AddInitial(initial => initial.AddControlFlow("inner"))
                .AddStructuredActivity("inner", inner => inner
                    .AddOnInitialize(context =>
                    {
                        innerInitialized = true;
                        return Task.CompletedTask;
                    })
                    .AddAction("consume", context =>
                    {
                        Assert.IsTrue(outerInitialized && innerInitialized);
                        received = context.GetTokensOfType<int>().Single();
                        return Task.CompletedTask;
                    }))));

        await harness.ExecuteAsync();
        Assert.AreEqual(11, received);
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task PartitionOutgoingUsesEnclosingStreamScopeWithoutSelectionLeakage(bool parallel)
    {
        var received = new ConcurrentBag<int>();
        Guid outerThread = Guid.Empty;
        await using var harness = await Harness.CreateAsync(activity =>
        {
            activity.AddInitial(initial => initial.AddControlFlow("produce"));
            activity.AddAction("produce", context =>
            {
                outerThread = ((ActionContext)context).NodeScope.ThreadId;
                context.OutputRange(Enumerable.Range(1, 4));
                return Task.CompletedTask;
            }, action => action.AddFlow<int>("main"));
            void Configure(IStructuredActivityBuilder structure) => structure
                .AddInputPin<int>(input => input.AddFlow("emit"))
                .AddInitial(initial => initial.AddControlFlow("emit"))
                .AddAction("emit", context =>
                {
                    foreach (var value in ((ActionContext)context).Tokens.Cast<int>())
                        context.Output(value);
                    return Task.CompletedTask;
                }, action => action.AddFlow<int>("outside", flow => flow.AddGuard(async context =>
                {
                    await Task.Yield();
                    return true;
                })));
            if (parallel)
                activity.AddParallelActivity<int>("main", Configure);
            else
                activity.AddIterativeActivity<int>("main", structure => structure
                    .AddInputPin<int>(input => input.AddFlow("emit"))
                    .AddInitial(initial => initial.AddControlFlow("emit"))
                    .AddAction("emit", context =>
                    {
                        foreach (var value in ((ActionContext)context).Tokens.Cast<int>())
                            context.Output(value);
                        return Task.CompletedTask;
                    }, action => action.AddFlow<int>("outside")));
            activity.AddAction("outside", context =>
            {
                Assert.AreEqual(outerThread, ((ActionContext)context).NodeScope.ThreadId);
                Assert.IsNull(((ActionContext)context).Tokens);
                foreach (var value in context.GetTokensOfType<int>())
                    received.Add(value);
                return Task.CompletedTask;
            });
        });

        await harness.ExecuteAsync();
        CollectionAssert.AreEquivalent(new[] { 1, 2, 3, 4 }, received.ToArray());
    }

    [TestMethod]
    public async Task OrdinaryCrossingsInsideParallelPartitionsRemainInTheirOwnEnclosingInvocation()
    {
        var received = new ConcurrentBag<int>();
        await using var harness = await Harness.CreateAsync(activity => activity
            .AddInitial(initial => initial.AddControlFlow("produce"))
            .AddAction("produce", context =>
            {
                context.OutputRange(Enumerable.Range(1, 8));
                return Task.CompletedTask;
            }, action => action.AddFlow<int>("main"))
            .AddParallelActivity<int>("main", partition => partition
                .AddInputPin<int>(input => input.AddFlow("inner"))
                .AddInitial(initial => initial.AddControlFlow("inner"))
                .AddStructuredActivity("inner", inner => inner
                    .AddInputPin<int>(input => input.AddFlow("emit"))
                    .AddInitial(initial => initial.AddControlFlow("emit"))
                    .AddAction("emit", context =>
                    {
                        context.Output(((ActionContext)context).Tokens.Cast<int>().Single());
                        return Task.CompletedTask;
                    }, action => action.AddFlow<int>("consume")))
                .AddAction("consume", async context =>
                {
                    await Task.Yield();
                    var value = context.GetTokensOfType<int>().Single();
                    Assert.AreEqual(((ActionContext)context).Tokens.Cast<int>().Single(), value);
                    received.Add(value);
                })));

        await harness.ExecuteAsync();
        CollectionAssert.AreEquivalent(Enumerable.Range(1, 8).ToArray(), received.ToArray());
    }

    [TestMethod]
    public async Task PendingCrossingsKeepWeightsAndImplicitJoinAcrossIdleScopeRecovery()
    {
        var received = Array.Empty<int>();
        await using var harness = await Harness.CreateAsync(activity => activity
            .AddInputPin<int>(input => input
                .AddFlow("first", flow => flow.AddGuard(context => Task.FromResult(context.Token == 1)))
                .AddFlow("second", flow => flow.AddGuard(context => Task.FromResult(context.Token == 2))))
            .AddInitial(initial => initial.AddControlFlow("main"))
            .AddAction("first", context =>
            {
                context.PassAllTokensOn();
                return Task.CompletedTask;
            }, action => action.AddFlow<int>("consume", flow => flow.SetWeight(2)))
            .AddAction("second", context =>
            {
                context.PassAllTokensOn();
                return Task.CompletedTask;
            }, action => action.AddFlow<int>("consume"))
            .AddStructuredActivity("main", structure => structure
                .AddAcceptEventAction<Wake>("wait", context => Task.CompletedTask)
                .AddAction("consume", context =>
                {
                    received = context.GetTokensOfType<int>().Order().ToArray();
                    return Task.CompletedTask;
                })));

        await harness.ExecuteAsync();
        await harness.InputAsync([new TokenHolder<int> { Payload = 1 }]);
        Assert.AreEqual(0, received.Length);
        await harness.InputAsync([new TokenHolder<int> { Payload = 1 }]);
        Assert.AreEqual(0, received.Length);
        await harness.RehydrateAsync();
        await harness.InputAsync([new TokenHolder<int> { Payload = 2 }]);
        CollectionAssert.AreEqual(new[] { 1, 1, 2 }, received);
    }

    [TestMethod]
    [DataRow("ordinary", false)]
    [DataRow("ordinary", true)]
    [DataRow("parallel", false)]
    [DataRow("parallel", true)]
    [DataRow("iterative", false)]
    [DataRow("iterative", true)]
    public async Task CrossingCheckpointResumesInDestinationFrameAndPreservesStreamedProvenance(string kind, bool rehydrate)
    {
        var received = 0;
        var initialized = 0;
        Harness? harness = null;
        harness = await Harness.CreateAsync(activity =>
        {
            activity.AddInitial(initial => initial.AddControlFlow("produce"));
            activity.AddAction("produce", context =>
            {
                context.Output(13);
                return Task.CompletedTask;
            }, action => action.AddFlow<int>("main"));
            void Configure(IStructuredActivityBuilder structure) => structure
                .AddInputPin<int>(input => input.AddFlow("emit"))
                .AddOnInitialize(context =>
                {
                    initialized++;
                    return Task.CompletedTask;
                })
                .AddInitial(initial => initial.AddControlFlow("emit"))
                .AddAction("emit", async context =>
                {
                    context.Output(13);
                    await harness!.Executor.RequestCheckpointPauseAsync();
                }, action => action.AddFlow<int>("outside"));
            if (kind == "parallel")
                activity.AddParallelActivity<int>("main", Configure);
            else if (kind == "iterative")
                activity.AddIterativeActivity<int>("main", Configure);
            else
                activity.AddStructuredActivity("main", structure => Configure((IStructuredActivityBuilder)structure));
            activity.AddAction("outside", context =>
            {
                Assert.AreEqual(ExecutionMode.Streamed, context.ExecutionMode);
                Assert.IsNull(((ActionContext)context).Tokens);
                Assert.IsNull(harness!.Executor.NodeScope.ContinuationFrame);
                received += context.GetTokensOfType<int>().Single();
                return Task.CompletedTask;
            });
        });
        await using (harness)
        {
            using var provenance = harness.Root.BeginInvocationProvenance(true);
            await Assert.ThrowsExactlyAsync<ActivityCheckpointPauseException>(() => harness.ExecuteAsync());
            Assert.AreEqual(0, received);
            Assert.IsTrue(harness.Root.HasReadyFirings);
            if (rehydrate)
                await harness.RehydrateAsync();
            await harness.ResumeAsync();
            Assert.AreEqual(13, received);
            Assert.AreEqual(1, initialized);
            Assert.IsFalse(harness.Root.HasReadyFirings);
        }
    }

    [TestMethod]
    public async Task SiblingDestinationFrameIsRetainedWhenItsOwnLastFiringHasNoSuccessors()
    {
        var destinationStarted = Signal();
        var checkpointRequested = Signal();
        var releaseDestination = Signal();
        var initialized = 0;
        var finalized = 0;
        var received = 0;
        Harness? harness = null;
        harness = await Harness.CreateAsync(activity => activity
            .AddInitial(initial => initial.AddControlFlow("left").AddControlFlow("right"))
            .AddStructuredActivity("left", structure => structure
                .AddInitial(initial => initial.AddControlFlow("emit"))
                .AddAction("emit", async context =>
                {
                    await destinationStarted.Task.WaitAsync(Deadline);
                    context.Output(19);
                    await harness!.Executor.RequestCheckpointPauseAsync();
                    checkpointRequested.TrySetResult();
                }, action => action.AddFlow<int>("consume")))
            .AddStructuredActivity("right", structure => structure
                .AddOnInitialize(context =>
                {
                    initialized++;
                    return Task.CompletedTask;
                })
                .AddOnFinalize(context =>
                {
                    finalized++;
                    return Task.CompletedTask;
                })
                .AddInitial(initial => initial.AddControlFlow("hold"))
                .AddAction("hold", async context =>
                {
                    destinationStarted.TrySetResult();
                    await releaseDestination.Task.WaitAsync(Deadline);
                })
                .AddAction("consume", context =>
                {
                    received = context.GetTokensOfType<int>().Single();
                    return Task.CompletedTask;
                })));
        await using (harness)
        {
            var execution = harness.ExecuteAsync();
            await checkpointRequested.Task.WaitAsync(Deadline);
            await WaitForReadyAsync().WaitAsync(Deadline);
            releaseDestination.TrySetResult();
            await Assert.ThrowsExactlyAsync<ActivityCheckpointPauseException>(() => execution);
            Assert.AreEqual(0, received);
            Assert.AreEqual(0, finalized);
            await harness.RehydrateAsync();
            await harness.ResumeAsync();
            Assert.AreEqual(19, received);
            Assert.AreEqual(1, initialized);
            Assert.AreEqual(1, finalized);
            Assert.IsFalse(harness.Root.HasReadyFirings);
        }

        async Task WaitForReadyAsync()
        {
            while (!harness.Root.HasReadyFirings)
                await Task.Delay(1);
        }
    }

    [TestMethod]
    public async Task OutgoingMixedAcceptanceHandsOffFromDestinationRatherThanSourceFrame()
    {
        Func<Task>? hosted = null;
        var received = Array.Empty<int>();
        await using var harness = await Harness.CreateAsync(activity => activity
            .AddInputPin<int>(input => input.AddFlow("preStream"))
            .AddAction("preStream", context =>
            {
                context.PassAllTokensOn();
                return Task.CompletedTask;
            }, action => action.AddFlow<int>("outside"))
            .AddInitial(initial => initial.AddControlFlow("main"))
            .AddStructuredActivity("main", structure => structure
                .AddInitial(initial => initial.AddControlFlow("emit"))
                .AddAction("emit", context =>
                {
                    context.Output(13);
                    return Task.CompletedTask;
                }, action => action.AddFlow<int>("outside")))
            .AddAction("outside", context =>
            {
                Assert.AreEqual(ExecutionMode.Streamed, context.ExecutionMode);
                Assert.IsNull(((ActionContext)context).Tokens);
                received = context.GetTokensOfType<int>().Order().ToArray();
                return Task.CompletedTask;
            }));

        using (harness.Root.BeginInvocationProvenance(true, callRoot: true))
            await harness.InputAsync([new TokenHolder<int> { Payload = 99 }]);
        harness.Executor.StreamedWorkHost = continuation =>
        {
            hosted = continuation;
            return true;
        };
        using (harness.Root.BeginInvocationProvenance(false, callRoot: true))
            await harness.ExecuteAsync();
        Assert.IsNotNull(hosted);
        Assert.AreEqual(0, received.Length);
        Assert.IsFalse(harness.Root.HasReadyFirings);
        await harness.RunHostedAsync(hosted);
        CollectionAssert.AreEqual(new[] { 13, 99 }, received);
    }

    [TestMethod]
    [DataRow("parallel-input")]
    [DataRow("iterative-input")]
    [DataRow("reentrant-input")]
    [DataRow("parallel-descendant-input")]
    [DataRow("parallel-grandparent-output")]
    [DataRow("iterative-sibling-output")]
    [DataRow("unknown-target")]
    public void InvalidCrossingModelsAreRejected(string model)
    {
        using var pending = new PendingTasksScope();
        var services = new ServiceCollection().AddLogging();
        var exception = Assert.Throws<Exception>(() => services.AddStateflows(sf => sf.AddActivities(activities =>
            activities.AddActivity("invalid-crossing", activity =>
            {
                activity.AddInitial(initial => initial.AddControlFlow("produce"));
                activity.AddAction("produce", context => Task.CompletedTask, action => action
                    .AddControlFlow(model switch
                    {
                        "unknown-target" => "missing.consume",
                        "parallel-descendant-input" => "consume",
                        "iterative-sibling-output" => "main-consume",
                        _ => "consume"
                    }));
                if (model == "parallel-input")
                    activity.AddParallelActivity<int>("main", structure => structure.AddAction("consume", context => Task.CompletedTask));
                else if (model == "iterative-input")
                    activity.AddIterativeActivity<int>("main", structure => structure.AddAction("consume", context => Task.CompletedTask));
                else if (model == "parallel-descendant-input")
                    activity.AddParallelActivity<int>("main", structure => structure
                        .AddStructuredActivity("inner", inner => inner.AddAction("consume", context => Task.CompletedTask)));
                else if (model == "parallel-grandparent-output")
                    activity.AddStructuredActivity("main", structure => structure
                        .AddAction("consume", context => Task.CompletedTask, action => action.AddControlFlow("partition"))
                        .AddParallelActivity<int>("partition", partition => partition
                            .AddInitial(initial => initial.AddControlFlow("produce"))));
                else if (model == "iterative-sibling-output")
                    activity.AddStructuredActivity("main", structure => structure
                        .AddAction("main-consume", context => Task.CompletedTask, action => action.AddControlFlow("partition"))
                        .AddIterativeActivity<int>("partition", partition => partition
                            .AddInitial(initial => initial.AddControlFlow("other-consume"))));
                else
                    activity.AddStructuredActivity("main", structure =>
                    {
                        structure.AddAction(model == "iterative-sibling-output" ? "main-consume" : "consume", context => Task.CompletedTask);
                        if (model == "reentrant-input")
                            structure.SetOptions(NodeOptions.ActionDefault | NodeOptions.Reentrant);
                    });
                if (model == "iterative-sibling-output")
                    activity.AddStructuredActivity("other", structure => structure.AddAction("other-consume", context => Task.CompletedTask));
            }))));
        StringAssert.Contains(exception.Message, model == "unknown-target" ? "ACT-014" : "ACT-059");
    }

    private sealed class Harness(ServiceProvider provider, PendingTasksScope pending, Executor executor, RootContext root, IHost host) : IAsyncDisposable
    {
        private bool runtimeStarted;
        public Executor Executor { get; private set; } = executor;
        public RootContext Root { get; private set; } = root;

        public static async Task<Harness> CreateAsync(Action<IActivityBuilder> configure)
        {
            var pending = new PendingTasksScope();
            var host = new HostBuilder()
                .ConfigureServices((_, services) => services.AddLogging()
                    .AddStateflows(sf => sf.AddLegacyRuntime().AddActivities(activities =>
                        activities.AddActivity("crossing-flows", activity => configure(activity)))))
                .Build();
            var provider = (ServiceProvider)host.Services;
            provider.GetRequiredService<ITenantAccessor>().CurrentTenantId =
                provider.GetRequiredService<IStateflowsTenantProvider>().GetCurrentTenantId();
            var register = provider.GetRequiredService<ActivitiesRegister>();
            var executor = new Executor(register, register.Activities["crossing-flows.current"], provider);
            var root = new RootContext(new StateflowsContext(new ActivityId("crossing-flows", Guid.NewGuid().ToString("N"))));
            await executor.HydrateAsync(root);
            return new Harness(provider, pending, executor, root, host);
        }

        public async Task ExecuteAsync()
        {
            using var tasks = new PendingTasksScope();
            provider.GetRequiredService<ITenantAccessor>().CurrentTenantId =
                provider.GetRequiredService<IStateflowsTenantProvider>().GetCurrentTenantId();
            using var execution = Root.BeginEventExecution();
            await Executor.DoExecuteStructuredNodeAsync(Executor.Graph, Executor.NodeScope).WaitAsync(Deadline);
        }

        public async Task ResumeAsync()
        {
            using var tasks = new PendingTasksScope();
            provider.GetRequiredService<ITenantAccessor>().CurrentTenantId =
                provider.GetRequiredService<IStateflowsTenantProvider>().GetCurrentTenantId();
            await Executor.ResumeReadyFiringsAsync().WaitAsync(Deadline);
        }

        public async Task InputAsync(IEnumerable<TokenHolder> tokens)
        {
            using var tasks = new PendingTasksScope();
            provider.GetRequiredService<ITenantAccessor>().CurrentTenantId =
                provider.GetRequiredService<IStateflowsTenantProvider>().GetCurrentTenantId();
            using var execution = Root.BeginEventExecution();
            foreach (var inputPin in Executor.Graph.InputPins)
                Root.NodesToExecute.Add(inputPin);
            await Executor.DoExecuteStructuredNodeAsync(Executor.Graph, Executor.NodeScope, tokens).WaitAsync(Deadline);
        }

        public async Task RehydrateAsync()
        {
            using var tasks = new PendingTasksScope();
            await Executor.DisposeAsync();
            var register = provider.GetRequiredService<ActivitiesRegister>();
            Executor = new Executor(register, register.Activities["crossing-flows.current"], provider);
            Root = new RootContext(Root.Context);
            await Executor.HydrateAsync(Root);
        }

        private IActivityBehavior LocateBehavior()
        {
            provider.GetRequiredService<ITenantAccessor>().CurrentTenantId =
                provider.GetRequiredService<IStateflowsTenantProvider>().GetCurrentTenantId();
            var locator = provider.GetRequiredService<IActivityLocator>();
            Assert.IsTrue(locator.TryLocateActivity(Root.Id, out var behavior));
            return behavior;
        }

        public async Task InitializeBehaviorAsync()
        {
            await host.StartAsync().WaitAsync(Deadline);
            runtimeStarted = true;
            await LocateBehavior().SendAsync(new Initialize()).WaitAsync(Deadline);
        }

        public async Task SendInputAsync(int value)
            => await LocateBehavior().SendInputAsync(value).WaitAsync(Deadline);

        public async Task RunHostedAsync(Func<Task> continuation)
        {
            using var tasks = new PendingTasksScope();
            provider.GetRequiredService<ITenantAccessor>().CurrentTenantId =
                provider.GetRequiredService<IStateflowsTenantProvider>().GetCurrentTenantId();
            using var execution = Root.BeginEventExecution();
            using var provenance = Root.BeginInvocationProvenance(true, callRoot: true);
            await continuation().WaitAsync(Deadline);
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                if (runtimeStarted)
                    await host.StopAsync().WaitAsync(Deadline);
                await Executor.DisposeAsync();
                if (host is IAsyncDisposable asyncHost)
                    await asyncHost.DisposeAsync();
                else
                    host.Dispose();
            }
            finally
            {
                pending.Dispose();
            }
        }
    }
}
