using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Stateflows.Activities;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Engine;
using Stateflows.Activities.Registration;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Common;
using Stateflows.Common.Context;
using Stateflows.Common.Engine;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Utilities;
using Stateflows.Activities.Streams;

namespace Activity.IntegrationTests.Tests;

[TestClass]
public sealed class Interleaving
{
    private static readonly TimeSpan Deadline = TimeSpan.FromSeconds(10);
    private static TaskCompletionSource Signal() => new(TaskCreationOptions.RunContinuationsAsynchronously);

    [TestMethod]
    public async Task StructuredArrivalsUseOneInitializationAndInitialControl()
    {
        var first = Signal();
        var second = Signal();
        var release = Signal();
        var initialized = 0;
        var initial = 0;
        var finalized = 0;
        var outputs = new ConcurrentBag<int>();
        await using var harness = await Harness.CreateAsync(activity => activity
            .AddInputPin<int>(input => input.AddFlow("work"))
            .AddStructuredActivity("work", structure => structure.Interleave()
                .AddOnInitialize(_ => { initialized++; return Task.CompletedTask; })
                .AddOnFinalize(_ => { finalized++; return Task.CompletedTask; })
                .AddInitial(input => input.AddControlFlow("initial"))
                .AddAction("initial", _ => { initial++; return Task.CompletedTask; })
                .AddInputPin<int>(input => input.AddFlow("consume"))
                .AddAction("consume", async context =>
                {
                    var value = context.GetTokensOfType<int>().Single();
                    if (value == 1) { first.TrySetResult(); await release.Task.WaitAsync(Deadline); }
                    else second.TrySetResult();
                    context.Output(value);
                }, action => action.Reentrant().AddFlow<int>("result"))
                .AddOutputPin<int>("result")
                .AddFlow<int>("sink"))
            .AddAction("sink", context =>
            {
                foreach (var value in context.GetTokensOfType<int>()) outputs.Add(value);
                return Task.CompletedTask;
            }));

        var invocation = harness.InputAsync(1);
        await first.Task.WaitAsync(Deadline);
        await harness.InputAsync(2);
        await second.Task.WaitAsync(Deadline);
        Assert.AreEqual(1, initialized);
        Assert.AreEqual(1, initial);
        Assert.AreEqual(0, finalized);
        release.TrySetResult();
        await invocation;
        Assert.AreEqual(1, finalized);
        CollectionAssert.AreEquivalent(new[] { 1, 2 }, outputs.ToArray());
    }

    [TestMethod]
    [DataRow(true, -1)]
    [DataRow(true, 2)]
    [DataRow(false, -1)]
    public async Task PartitionsChunkEachArrivalAndNeverInheritCommonInput(bool parallel, int limit)
    {
        var entered = Signal();
        var release = Signal();
        var arrived = Signal();
        var records = new ConcurrentQueue<(int[] Values, string[] Common)>();
        var initialized = 0;
        var finalized = 0;
        var outputs = new List<int>();
        await using var harness = await Harness.CreateAsync(activity =>
        {
            activity.AddInputPin<object>(input => input.AddFlow("work"));
            void Build(IStructuredActivityBuilder structure) => structure.Interleave()
                .AddOnInitialize(_ => { initialized++; return Task.CompletedTask; })
                .AddOnFinalize(_ => { finalized++; return Task.CompletedTask; })
                .AddInputPin<object>(input => input.AddFlow("consume"))
                .AddAction("consume", async context =>
                {
                    var values = context.GetTokensOfType<int>().ToArray();
                    records.Enqueue((values, context.GetTokensOfType<string>().ToArray()));
                    if (values.Contains(1)) { entered.TrySetResult(); await release.Task.WaitAsync(Deadline); }
                    else arrived.TrySetResult();
                    foreach (var value in values) context.Output(value);
                }, action => action.AddFlow<int>("result"))
                .AddOutputPin<int>("result").AddFlow<int>("sink");
            if (parallel)
                activity.AddParallelActivity<int>("work", Build, chunkSize: 2, maxConcurrency: limit);
            else
                activity.AddIterativeActivity<int>("work", Build, chunkSize: 2);
            activity.AddAction("sink", context =>
            {
                outputs.AddRange(context.GetTokensOfType<int>());
                return Task.CompletedTask;
            });
        });

        var invocation = harness.InputAsync(1, 2, 3, "first");
        await entered.Task.WaitAsync(Deadline);
        await harness.InputAsync(4, 5, 6);
        if (parallel) await arrived.Task.WaitAsync(Deadline);
        else Assert.AreEqual(1, records.Count, "New chunks must wait behind the active iterative partition.");
        release.TrySetResult();
        await invocation;
        Assert.AreEqual(1, initialized);
        Assert.AreEqual(1, finalized);
        CollectionAssert.AreEquivalent(new[] { 1, 2, 3, 4, 5, 6 }, outputs);
        var chunks = records.ToArray();
        CollectionAssert.AreEquivalent(new[] { "1,2", "3", "4,5", "6" },
            chunks.Select(record => string.Join(",", record.Values)).ToArray());
        foreach (var record in chunks)
            CollectionAssert.AreEqual(record.Values[0] <= 3 ? new[] { "first" } : Array.Empty<string>(), record.Common);
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task ParallelLimitQueuesUntilSlotOpens(bool interleave)
    {
        var started = new ConcurrentQueue<int>();
        var entries = Enumerable.Range(0, 5).Select(_ => Signal()).ToArray();
        var releases = Enumerable.Range(0, 5).Select(_ => Signal()).ToArray();
        var current = 0;
        var maximum = 0;
        await using var harness = await Harness.CreateAsync(activity => activity
            .AddInputPin<int>(input => input.AddFlow("work"))
            .AddParallelActivity<int>("work", structure =>
            {
                if (interleave) structure.Interleave();
                structure.AddInputPin<int>(input => input.AddFlow("consume"))
                    .AddAction("consume", async context =>
                    {
                        var value = context.GetTokensOfType<int>().Single();
                        var active = Interlocked.Increment(ref current);
                        int previous;
                        do { previous = maximum; } while (active > previous && Interlocked.CompareExchange(ref maximum, active, previous) != previous);
                        started.Enqueue(value);
                        entries[value].TrySetResult();
                        await releases[value].Task.WaitAsync(Deadline);
                        Interlocked.Decrement(ref current);
                    });
            }, chunkSize: 1, maxConcurrency: 2));
        var invocation = harness.InputAsync(0, 1, 2);
        await Task.WhenAll(entries[0].Task, entries[1].Task).WaitAsync(Deadline);
        if (interleave) await harness.InputAsync(3, 4);
        Assert.AreEqual(2, started.Count);
        releases[0].TrySetResult();
        await entries[2].Task.WaitAsync(Deadline);
        Assert.AreEqual(3, started.Count);
        for (var index = 1; index < releases.Length; index++) releases[index].TrySetResult();
        await invocation;
        Assert.AreEqual(2, maximum);
        Assert.AreEqual(interleave ? 5 : 3, started.Count);
    }

    [TestMethod]
    public async Task FinalizationAndOutputRoutingFenceNextInvocation()
    {
        var finalizing = Signal();
        var finalizeRelease = Signal();
        var routing = Signal();
        var routeRelease = Signal();
        var initialized = 0;
        var finalized = 0;
        await using var harness = await Harness.CreateAsync(activity => activity
            .AddInputPin<int>(input => input.AddFlow("work"))
            .AddParallelActivity<int>("work", structure => structure.Interleave()
                .AddOnInitialize(_ => { initialized++; return Task.CompletedTask; })
                .AddOnFinalize(async _ =>
                {
                    finalized++;
                    if (finalized == 1) { finalizing.TrySetResult(); await finalizeRelease.Task.WaitAsync(Deadline); }
                })
                .AddInputPin<int>(input => input.AddFlow("result"))
                .AddOutputPin<int>("result")
                .AddFlow<int>("sink", flow => flow.AddGuard(async context =>
                {
                    if (context.Token == 1) { routing.TrySetResult(); await routeRelease.Task.WaitAsync(Deadline); }
                    return true;
                })))
            .AddAction("sink", _ => Task.CompletedTask));
        var first = harness.InputAsync(1);
        await finalizing.Task.WaitAsync(Deadline);
        var second = harness.InputAsync(2);
        Assert.AreEqual(1, initialized);
        finalizeRelease.TrySetResult();
        await routing.Task.WaitAsync(Deadline);
        Assert.AreEqual(1, initialized);
        routeRelease.TrySetResult();
        await Task.WhenAll(first, second);
        Assert.AreEqual(2, initialized);
        Assert.AreEqual(2, finalized);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(-2)]
    public void InvalidParallelLimitFailsRegistration(int limit)
    {
        var exception = Assert.Throws<Exception>(() =>
            new ServiceCollection().AddStateflows(sf => sf.AddActivities(activities =>
                activities.AddActivity("invalid", activity => activity.AddInitial(initial => initial.AddControlFlow("work")).AddParallelActivity<int>(
                    "work", _ => { }, maxConcurrency: limit)))));
        StringAssert.Contains(exception.Message, "ACT-060");
    }

    [TestMethod]
    [DataRow("structured")]
    [DataRow("parallel")]
    [DataRow("iterative")]
    public void ReentrantAndInterleaveAreMutuallyExclusive(string kind)
    {
        var exception = Assert.Throws<Exception>(() =>
            new ServiceCollection().AddStateflows(sf => sf.AddActivities(activities =>
                activities.AddActivity("invalid", activity =>
                {
                    activity.AddInitial(initial => initial.AddControlFlow("work"));
                    if (kind == "structured") activity.AddStructuredActivity("work", scope => scope.Interleave().Reentrant());
                    else if (kind == "parallel") activity.AddParallelActivity<int>("work", scope => scope.Reentrant().Interleave());
                    else activity.AddIterativeActivity<int>("work", scope => scope.Interleave().Reentrant());
                }))));
        StringAssert.Contains(exception.Message, "ACT-061");
    }

    [TestMethod]
    public void InterleaveIsNotAnActionBuilderMethod()
    {
        foreach (var builder in new[] { typeof(IActionBuilder), typeof(IActivityBuilder), typeof(IInitialBuilder) })
            Assert.IsFalse(builder.GetInterfaces().Append(builder).SelectMany(type => type.GetMethods())
                .Any(method => method.Name == "Interleave"), builder.Name);
        var exception = Assert.Throws<Exception>(() =>
            new ServiceCollection().AddStateflows(sf => sf.AddActivities(activities =>
                activities.AddActivity("invalid", activity => activity.AddInitial(initial => initial.AddControlFlow("work")).AddAction("work", _ => Task.CompletedTask,
                    action => action.SetOptions(NodeOptions.ActionDefault | NodeOptions.Interleave))))));
        StringAssert.Contains(exception.Message, "ACT-023");
    }

    [TestMethod]
    [DataRow("structured")]
    [DataRow("parallel")]
    [DataRow("iterative")]
    public async Task CheckpointRetainsAdmittedArrivalsWithoutReinitializing(string kind)
    {
        var entered = Signal();
        var release = Signal();
        var initialized = 0;
        var finalized = 0;
        var records = new ConcurrentBag<string>();
        var modes = new ConcurrentDictionary<int, ExecutionMode>();
        Harness? harness = null;
        harness = await Harness.CreateAsync(activity =>
        {
            activity.AddInputPin<object>(input => input.AddFlow("work"));
            void Build(IStructuredActivityBuilder structure) => structure.Interleave()
                .AddOnInitialize(_ => { initialized++; return Task.CompletedTask; })
                .AddOnFinalize(_ => { finalized++; return Task.CompletedTask; })
                .AddInputPin<object>(input => input.AddFlow("first"))
                .AddAction("first", async context =>
                {
                    if (context.GetTokensOfType<int>().Contains(1))
                    {
                        entered.TrySetResult();
                        await release.Task.WaitAsync(Deadline);
                        await harness!.Executor.RequestCheckpointPauseAsync();
                    }
                    context.PassAllTokensOn();
                }, action => action.AddFlow<object>("second"))
                .AddAction("second", context =>
                {
                    foreach (var value in context.GetTokensOfType<int>())
                        modes[value] = context.ExecutionMode;
                    records.Add(string.Join(",", context.GetTokensOfType<int>()) + ":" +
                        string.Join(",", context.GetTokensOfType<string>()));
                    return Task.CompletedTask;
                });
            if (kind == "structured") activity.AddStructuredActivity("work", structure => Build((IStructuredActivityBuilder)structure));
            else if (kind == "parallel") activity.AddParallelActivity<int>("work", Build, maxConcurrency: 1);
            else activity.AddIterativeActivity<int>("work", Build);
        });
        await using (harness)
        {
            var invocation = harness.InputAsync(1, "old");
            await entered.Task.WaitAsync(Deadline);
            await harness.InputAsync(2.ToTokenHolder().CopyForExecution(true), "new".ToTokenHolder().CopyForExecution(true));
            release.TrySetResult();
            await Assert.ThrowsExactlyAsync<ActivityCheckpointPauseException>(() => invocation);
            Assert.AreEqual(0, finalized);
            await harness.RehydrateAsync();
            await harness.ResumeAsync();
            Assert.AreEqual(1, initialized);
            Assert.AreEqual(1, finalized);
            if (kind == "structured")
            {
                CollectionAssert.AreEquivalent(new[] { "1", "2" }, records.SelectMany(record => record.Split(':')[0].Split(',')).ToArray());
                CollectionAssert.AreEquivalent(new[] { "old", "new" }, records.SelectMany(record => record.Split(':')[1].Split(',')).ToArray());
            }
            else
            {
                CollectionAssert.AreEquivalent(new[] { "1:old", "2:new" }, records.ToArray());
                Assert.AreEqual(ExecutionMode.Transactional, modes[1]);
                Assert.AreEqual(ExecutionMode.Streamed, modes[2]);
            }
            Assert.IsFalse(harness.Root.HasReadyFirings);
        }
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public async Task DrainingDiscardsUnsatisfiedWeightedTokens(bool parallel)
    {
        var finalized = 0;
        var consumed = 0;
        await using var harness = await Harness.CreateAsync(activity =>
        {
            activity.AddInputPin<int>(input => input.AddFlow("work"));
            void Build(IStructuredActivityBuilder structure) => structure.Interleave()
                .AddOnFinalize(_ => { finalized++; return Task.CompletedTask; })
                .AddInputPin<int>(input => input.AddFlow("consume", flow => flow.SetWeight(2)))
                .AddAction("consume", _ => { consumed++; return Task.CompletedTask; });
            if (parallel) activity.AddParallelActivity<int>("work", Build);
            else activity.AddIterativeActivity<int>("work", Build);
        });
        await harness.InputAsync(1);
        await harness.InputAsync(2);
        Assert.AreEqual(0, consumed);
        Assert.AreEqual(2, finalized);
        Assert.IsFalse(harness.Root.Streams.Values.SelectMany(streams => streams.Values)
            .Any(stream => stream.Tokens.Count > 0));
    }

    public sealed class Wake;

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task StructuredEventWaitRetainsInvocationIdentityAcrossArrivals(bool rehydrate)
    {
        var initialized = 0;
        var initial = 0;
        var finalized = 0;
        var outputs = new List<int>();
        await using var harness = await Harness.CreateAsync(activity => activity
            .AddInputPin<int>(input => input.AddFlow("work"))
            .AddStructuredActivity("work", structure => structure.Interleave()
                .AddOnInitialize(_ => { initialized++; return Task.CompletedTask; })
                .AddOnFinalize(_ => { finalized++; return Task.CompletedTask; })
                .AddInitial(input => input.AddControlFlow("initial"))
                .AddAction("initial", _ => { initial++; return Task.CompletedTask; })
                .AddAcceptEventAction<Wake>("wait", _ => Task.CompletedTask)
                .AddInputPin<int>(input => input.AddFlow("consume"))
                .AddAction("consume", context =>
                {
                    context.PassAllTokensOn();
                    return Task.CompletedTask;
                }, action => action.AddFlow<int>("result").AddControlFlow(FinalNode.Name, flow =>
                    flow.AddGuard(context => Task.FromResult(outputs.Count > 0))))
                .AddOutputPin<int>("result").AddFinal()
                .AddFlow<int>("sink"))
            .AddAction("sink", context =>
            {
                outputs.AddRange(context.GetTokensOfType<int>());
                return Task.CompletedTask;
            }));
        await harness.InputAsync(1);
        Assert.AreEqual(0, finalized);
        if (rehydrate) await harness.RehydrateAsync();
        await harness.InputAsync(2);
        Assert.AreEqual(1, initialized);
        Assert.AreEqual(1, initial);
        Assert.AreEqual(1, finalized);
        CollectionAssert.AreEqual(new[] { 1, 2 }, outputs);
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task CancellationDropsQueuedPartitionsAndFencesLateOutputs(bool interleave)
    {
        var entered = Signal();
        var release = Signal();
        var executed = 0;
        var routed = 0;
        await using var harness = await Harness.CreateAsync(activity => activity
            .AddInputPin<int>(input => input.AddFlow("work"))
            .AddParallelActivity<int>("work", structure =>
            {
                if (interleave) structure.Interleave();
                structure.AddInputPin<int>(input => input.AddFlow("consume"))
                    .AddAction("consume", async context =>
                    {
                        Interlocked.Increment(ref executed);
                        entered.TrySetResult();
                        await release.Task.WaitAsync(Deadline);
                        context.Output(10);
                    }, action => action.AddFlow<int>("result"))
                    .AddOutputPin<int>("result").AddFlow<int>("sink");
            }, maxConcurrency: 1)
            .AddAction("sink", context => { routed++; return Task.CompletedTask; }));
        var invocation = harness.InputAsync(1, 2, 3);
        await entered.Task.WaitAsync(Deadline);
        harness.Executor.NodeScope.CancellationTokenSource.Cancel();
        release.TrySetResult();
        await invocation;
        Assert.AreEqual(1, executed);
        Assert.AreEqual(0, routed);
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public async Task NestedPartitionParentsNeverMixChildInvocations(bool parallel)
    {
        var entered = new[] { Signal(), Signal() };
        var release = Signal();
        var initialized = 0;
        var values = new ConcurrentBag<int>();
        await using var harness = await Harness.CreateAsync(activity => activity
            .AddInputPin<int>(input => input.AddFlow("parent"))
            .AddParallelActivity<int>("parent", parent =>
            {
                parent.AddInputPin<int>(input => input.AddFlow("child"));
                void Child(IStructuredActivityBuilder child) => child.Interleave()
                    .AddOnInitialize(_ => { Interlocked.Increment(ref initialized); return Task.CompletedTask; })
                    .AddInputPin<int>(input => input.AddFlow("consume"))
                    .AddAction("consume", async context =>
                    {
                        var value = context.GetTokensOfType<int>().Single();
                        entered[value - 1].TrySetResult();
                        await release.Task.WaitAsync(Deadline);
                        values.Add(value);
                    });
                if (parallel) parent.AddParallelActivity<int>("child", Child);
                else parent.AddIterativeActivity<int>("child", Child);
            }));
        var invocation = harness.InputAsync(1, 2);
        await Task.WhenAll(entered.Select(signal => signal.Task)).WaitAsync(Deadline);
        Assert.AreEqual(2, initialized);
        release.TrySetResult();
        await invocation;
        CollectionAssert.AreEquivalent(new[] { 1, 2 }, values.ToArray());
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public async Task InterleavedPartitionsCanEscapeToImmediateParentBeforeFinalization(bool parallel)
    {
        var escaped = Signal();
        var finalized = 0;
        var received = 0;
        await using var harness = await Harness.CreateAsync(activity =>
        {
            activity.AddInputPin<int>(input => input.AddFlow("work"));
            void Build(IStructuredActivityBuilder structure) => structure.Interleave()
                .AddOnFinalize(_ => { finalized++; return Task.CompletedTask; })
                .AddInputPin<int>(input => input.AddFlow("emit"))
                .AddAction("emit", context =>
                {
                    context.PassAllTokensOn();
                    return Task.CompletedTask;
                }, action => action.AddFlow<int>("outside").AddControlFlow("hold"))
                .AddAction("hold", async _ => await escaped.Task.WaitAsync(Deadline));
            if (parallel) activity.AddParallelActivity<int>("work", Build);
            else activity.AddIterativeActivity<int>("work", Build);
            activity.AddAction("outside", context =>
            {
                Assert.AreEqual(0, finalized);
                Interlocked.Add(ref received, context.GetTokensOfType<int>().Sum());
                escaped.TrySetResult();
                return Task.CompletedTask;
            });
        });
        await harness.InputAsync(7);
        Assert.AreEqual(7, received);
        Assert.AreEqual(1, finalized);
    }

    [TestMethod]
    public async Task IncomingCrossingAndInterleavedInputShareActiveStructuredScope()
    {
        var first = Signal();
        var crossed = Signal();
        var release = Signal();
        var initialized = 0;
        var values = new ConcurrentBag<int>();
        await using var harness = await Harness.CreateAsync(activity => activity
            .AddInputPin<int>(input => input
                .AddFlow("work", flow => flow.AddGuard(context => Task.FromResult(context.Token != 3)))
                .AddFlow("crossing-consume", flow => flow.AddGuard(context => Task.FromResult(context.Token == 3))))
            .AddStructuredActivity("work", structure => structure.Interleave()
                .AddOnInitialize(_ => { initialized++; return Task.CompletedTask; })
                .AddInputPin<int>(input => input.AddFlow("consume"))
                .AddAction("consume", async context =>
                {
                    var value = context.GetTokensOfType<int>().Single();
                    values.Add(value);
                    if (value == 1) { first.TrySetResult(); await release.Task.WaitAsync(Deadline); }
                }, action => action.Reentrant())
                .AddAction("crossing-consume", context =>
                {
                    values.Add(context.GetTokensOfType<int>().Single());
                    crossed.TrySetResult();
                    return Task.CompletedTask;
                })));
        var invocation = harness.InputAsync(1);
        await first.Task.WaitAsync(Deadline);
        await harness.InputAsync(2);
        await harness.InputAsync(3);
        await crossed.Task.WaitAsync(Deadline);
        release.TrySetResult();
        await invocation;
        Assert.AreEqual(1, initialized);
        CollectionAssert.AreEquivalent(new[] { 1, 2, 3 }, values.ToArray());
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public async Task FailuresDoNotFinalizeOrLeakIntoAFreshInvocation(bool parallel)
    {
        var finalized = 0;
        var initialized = 0;
        await using var harness = await Harness.CreateAsync(activity =>
        {
            activity.AddInputPin<int>(input => input.AddFlow("work"));
            void Build(IStructuredActivityBuilder structure) => structure.Interleave()
                .AddOnInitialize(_ => { initialized++; return Task.CompletedTask; })
                .AddOnFinalize(_ => { finalized++; return Task.CompletedTask; })
                .AddInputPin<int>(input => input.AddFlow("consume"))
                .AddAction("consume", context =>
                {
                    if (context.GetTokensOfType<int>().Contains(1)) throw new InvalidOperationException("expected");
                    return Task.CompletedTask;
                });
            if (parallel) activity.AddParallelActivity<int>("work", Build);
            else activity.AddIterativeActivity<int>("work", Build);
        });
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => harness.InputAsync(1));
        Assert.AreEqual(0, finalized);
        await harness.InputAsync(2);
        Assert.AreEqual(2, initialized);
        Assert.AreEqual(1, finalized);
    }

    [TestMethod]
    public async Task IterativeFinalKeepsItsExistingEarlyTerminationBehavior()
    {
        var values = new List<int>();
        var finalized = 0;
        await using var harness = await Harness.CreateAsync(activity => activity
            .AddInputPin<int>(input => input.AddFlow("work"))
            .AddIterativeActivity<int>("work", structure => structure.Interleave()
                .AddOnFinalize(_ => { finalized++; return Task.CompletedTask; })
                .AddInputPin<int>(input => input.AddFlow("consume"))
                .AddAction("consume", context =>
                {
                    values.Add(context.GetTokensOfType<int>().Single());
                    return Task.CompletedTask;
                }, action => action.AddControlFlow(FinalNode.Name))
                .AddFinal()));
        await harness.InputAsync(1, 2, 3);
        CollectionAssert.AreEqual(new[] { 1 }, values);
        Assert.AreEqual(1, finalized);
    }

    [TestMethod]
    public async Task OrdinaryBoundedParallelCheckpointResumesQueuedPartitions()
    {
        var initialized = 0;
        var finalized = 0;
        var values = new List<int>();
        Harness? harness = null;
        harness = await Harness.CreateAsync(activity => activity
            .AddInputPin<int>(input => input.AddFlow("work"))
            .AddParallelActivity<int>("work", structure => structure
                .AddOnInitialize(_ => { initialized++; return Task.CompletedTask; })
                .AddOnFinalize(_ => { finalized++; return Task.CompletedTask; })
                .AddInputPin<int>(input => input.AddFlow("first"))
                .AddAction("first", async context =>
                {
                    if (context.GetTokensOfType<int>().Single() == 1)
                        await harness!.Executor.RequestCheckpointPauseAsync();
                    context.PassAllTokensOn();
                }, action => action.AddFlow<int>("second"))
                .AddAction("second", context =>
                {
                    values.Add(context.GetTokensOfType<int>().Single());
                    return Task.CompletedTask;
                }), maxConcurrency: 1));
        await using (harness)
        {
            await Assert.ThrowsExactlyAsync<ActivityCheckpointPauseException>(() => harness.InputAsync(1, 2, 3));
            Assert.AreEqual(0, values.Count);
            await harness.RehydrateAsync();
            await harness.ResumeAsync();
            Assert.AreEqual(1, initialized);
            Assert.AreEqual(1, finalized);
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, values);
        }
    }

    [TestMethod]
    public async Task CheckpointDuringInitializationRetainsUnstartedStructuredNetwork()
        {
            var initializing = Signal();
            var release = Signal();
            var initialized = 0;
            var initial = 0;
            var received = 0;
            await using var harness = await Harness.CreateAsync(activity => activity
                .AddInputPin<int>(input => input.AddFlow("work"))
                .AddStructuredActivity("work", work => work.Interleave()
                    .AddOnInitialize(async _ =>
                    {
                        initialized++;
                        initializing.TrySetResult();
                        await release.Task.WaitAsync(Deadline);
                    })
                    .AddInitial(input => input.AddControlFlow("initial"))
                    .AddAction("initial", _ => { initial++; return Task.CompletedTask; })
                    .AddInputPin<int>(input => input.AddFlow("consume"))
                    .AddAction("consume", context =>
                    {
                        received += context.GetTokensOfType<int>().Sum();
                        return Task.CompletedTask;
                    })));
            var invocation = harness.InputAsync(7);
            await initializing.Task.WaitAsync(Deadline);
            await harness.InputAsync(2);
            await harness.Executor.RequestCheckpointPauseAsync();
            release.TrySetResult();
            await Assert.ThrowsExactlyAsync<ActivityCheckpointPauseException>(() => invocation);
            Assert.AreEqual(0, initial);
            Assert.AreEqual(0, received);
            await harness.RehydrateAsync();
            await harness.ResumeAsync();
            Assert.AreEqual(1, initialized);
            Assert.AreEqual(1, initial);
            Assert.AreEqual(9, received);
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task NestedIdleInterleavingKeepsItsLifecycleUnderOrdinaryParent(bool rehydrate)
    {
        var initialized = 0;
        var initial = 0;
        var values = new List<int>();
        await using var harness = await Harness.CreateAsync(activity => activity
            .AddInputPin<int>(input => input.AddFlow("outer"))
            .AddStructuredActivity("outer", outer => outer
                .AddInputPin<int>(input => input.AddFlow("work"))
                .AddStructuredActivity("work", work => work.Interleave()
                    .AddOnInitialize(_ => { initialized++; return Task.CompletedTask; })
                    .AddInitial(input => input.AddControlFlow("initial"))
                    .AddAction("initial", _ => { initial++; return Task.CompletedTask; })
                    .AddAcceptEventAction<Wake>("wait", _ => Task.CompletedTask)
                    .AddInputPin<int>(input => input.AddFlow("consume"))
                    .AddAction("consume", context =>
                    {
                        values.AddRange(context.GetTokensOfType<int>());
                        return Task.CompletedTask;
                    }))));
        await harness.InputAsync(1);
        if (rehydrate) await harness.RehydrateAsync();
        await harness.InputAsync(2);
        Assert.AreEqual(1, initialized);
        Assert.AreEqual(1, initial);
        CollectionAssert.AreEqual(new[] { 1, 2 }, values);
    }

    public sealed class TypedParallel(IInputToken<int> value) : IStructuredActivityNode;

    [TestMethod]
    [DataRow(true, false)]
    [DataRow(false, false)]
    [DataRow(true, true)]
    [DataRow(false, true)]
    public async Task CompletedPartitionsAreReleasedWhileInvocationRemainsActive(bool parallel, bool checkpoint)
    {
        const int count = 65;
        var entered = Enumerable.Range(0, count).Select(_ => Signal()).ToArray();
        var release = Enumerable.Range(0, count).Select(_ => Signal()).ToArray();
        var threads = new Guid[count];
        var outputs = new List<int>();
        var initialized = 0;
        var finalized = 0;
        NodeScope? owner = null;
        ActivityContinuationFrame? frame = null;
        Harness? harness = null;
        harness = await Harness.CreateAsync(activity =>
        {
            activity.AddInputPin<int>(pin => pin.AddFlow("work"));
            void Build(IStructuredActivityBuilder structure) => structure.Interleave()
                .AddOnInitialize(_ => { initialized++; return Task.CompletedTask; })
                .AddOnFinalize(_ => { finalized++; return Task.CompletedTask; })
                .AddInputPin<int>(pin => pin.AddFlow("consume"))
                .AddAction("consume", async context =>
                {
                    var value = context.GetTokensOfType<int>().Single();
                    var scope = ((ActionContext)context).NodeScope;
                    while (scope.ContinuationFrame == null)
                        scope = scope.BaseNodeScope;
                    owner = scope.BaseNodeScope;
                    frame = scope.ContinuationFrame;
                    threads[value] = scope.ThreadId;
                    entered[value].TrySetResult();
                    await release[value].Task.WaitAsync(Deadline);
                    if (checkpoint && value == count - 1)
                        await harness!.Executor.RequestCheckpointPauseAsync();
                    context.Output(value);
                }, action => action.AddFlow<int, OutputPin<int>>())
                .AddOutputPin<int>()
                .AddFlow<int>("sink");
            if (parallel)
                activity.AddParallelActivity<int>("work", Build, maxConcurrency: 1);
            else
                activity.AddIterativeActivity<int>("work", Build);
            activity.AddAction("sink", context =>
            {
                outputs.AddRange(context.GetTokensOfType<int>());
                return Task.CompletedTask;
            });
        });
        await using (harness)
        {
            var invocation = harness.InputAsync(0);
            await entered[0].Task.WaitAsync(Deadline);
            for (var value = 1; value < count; value++)
            {
                await harness.InputAsync(value);
                release[value - 1].TrySetResult();
                await entered[value].Task.WaitAsync(Deadline);
                Assert.AreEqual(1, frame!.Partitions.Count, "Only the running partition should remain.");
                Assert.IsFalse(frame.Partitions[0].Completed);
                Assert.AreEqual(value, frame.AccumulatedOutput.Count(token => token.PayloadType == typeof(int)), "Boundary output must be retained.");
                Assert.AreEqual(1, owner!.ChildScopes.Length, "Disposed partition scopes must be released.");
                Assert.AreEqual(0, outputs.Count, "Boundary output must not be routed before finalization.");
            }
            release[^1].TrySetResult();
            if (checkpoint)
            {
                await Assert.ThrowsExactlyAsync<ActivityCheckpointPauseException>(() => invocation);
                Assert.AreEqual(1, frame!.Partitions.Count, "The suspended partition must survive cleanup.");
                Assert.IsTrue(frame.Partitions[0].Started);
                Assert.IsFalse(frame.Partitions[0].Completed);
                await harness.RehydrateAsync();
                await harness.ResumeAsync();
            }
            else
            {
                await invocation;
                Assert.AreEqual(0, frame!.Partitions.Count);
                Assert.AreEqual(0, owner!.ChildScopes.Length);
            }
            Assert.AreEqual(1, initialized);
            Assert.AreEqual(1, finalized);
            CollectionAssert.AreEqual(Enumerable.Range(0, count).ToArray(), outputs);
            if (!parallel)
                Assert.AreEqual(1, threads.Distinct().Count(), "Iterative arrivals must retain one execution thread.");
        }
    }

    [TestMethod]
    public async Task TypedParallelOverloadsKeepConcurrencyAndInterleaveOptions()
    {
        await using var harness = await Harness.CreateAsync(activity => activity
            .AddInputPin<int>(input => input.AddFlow("typed").AddFlow("parent"))
            .AddParallelActivity<int, TypedParallel>("typed", structure => structure.Interleave()
                    .AddInputPin<int>(_ => { }),
                chunkSize: 2, maxConcurrency: 3)
            .AddParallelActivity<int>("parent", parent => parent
                .AddInputPin<int>(input => input.AddFlow("child"))
                .AddParallelActivity<int, TypedParallel>("child", structure => structure.Interleave()
                    .AddInputPin<int>(_ => { }),
                    chunkSize: 4, maxConcurrency: 5)));
        var typed = harness.Executor.Graph.AllNamedNodes["typed"];
        var child = harness.Executor.Graph.AllNamedNodes["child"];
        Assert.AreEqual(2, typed.ChunkSize);
        Assert.AreEqual(3, typed.MaxConcurrency);
        Assert.IsTrue(typed.Options.HasFlag(NodeOptions.Interleave));
        Assert.AreEqual(4, child.ChunkSize);
        Assert.AreEqual(5, child.MaxConcurrency);
        Assert.IsTrue(child.Options.HasFlag(NodeOptions.Interleave));
    }

    private sealed class Harness(ServiceProvider provider, PendingTasksScope pending, Executor executor, RootContext root, IHost host) : IAsyncDisposable
    {
        public Executor Executor { get; private set; } = executor;
        public RootContext Root { get; private set; } = root;
        public static async Task<Harness> CreateAsync(Action<IActivityBuilder> configure)
        {
            var pending = new PendingTasksScope();
            var host = new HostBuilder().ConfigureServices((_, services) => services.AddLogging()
                .AddStateflows(sf => sf.AddLegacyRuntime().AddActivities(activities =>
                    activities.AddActivity("interleaving", activity => configure(activity))))).Build();
            var provider = (ServiceProvider)host.Services;
            provider.GetRequiredService<ITenantAccessor>().CurrentTenantId =
                provider.GetRequiredService<IStateflowsTenantProvider>().GetCurrentTenantId();
            var register = provider.GetRequiredService<ActivitiesRegister>();
            var executor = new Executor(register, register.Activities["interleaving.current"], provider);
            var root = new RootContext(new StateflowsContext(new ActivityId("interleaving", Guid.NewGuid().ToString("N"))));
            await executor.HydrateAsync(root);
            return new Harness(provider, pending, executor, root, host);
        }
        public async Task InputAsync(params object[] values)
        {
            using var tasks = new PendingTasksScope();
            provider.GetRequiredService<ITenantAccessor>().CurrentTenantId =
                provider.GetRequiredService<IStateflowsTenantProvider>().GetCurrentTenantId();
            using var execution = Root.BeginEventExecution();
            foreach (var inputPin in Executor.Graph.InputPins)
                Root.NodesToExecute.Add(inputPin);
            await Executor.DoExecuteStructuredNodeAsync(Executor.Graph, Executor.NodeScope,
                values.Select(value => value switch
                {
                    int number => number.ToTokenHolder(),
                    string text => (TokenHolder)text.ToTokenHolder(),
                    TokenHolder token => token,
                    _ => throw new InvalidOperationException()
                }).ToArray()).WaitAsync(Deadline);
        }
        public async Task RehydrateAsync()
        {
            using var tasks = new PendingTasksScope();
            await Executor.DisposeAsync();
            var register = provider.GetRequiredService<ActivitiesRegister>();
            Executor = new Executor(register, register.Activities["interleaving.current"], provider);
            Root = new RootContext(StateflowsJsonConverter.Clone(Root.Context));
            await Executor.HydrateAsync(Root);
        }
        public async Task ResumeAsync()
        {
            using var tasks = new PendingTasksScope();
            provider.GetRequiredService<ITenantAccessor>().CurrentTenantId =
                provider.GetRequiredService<IStateflowsTenantProvider>().GetCurrentTenantId();
            await Executor.ResumeReadyFiringsAsync().WaitAsync(Deadline);
        }
        public async ValueTask DisposeAsync()
        {
            await Executor.DisposeAsync();
            host.Dispose();
            pending.Dispose();
        }
    }
}
