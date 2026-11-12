using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Activities;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Engine;
using Stateflows.Activities.Models;
using Stateflows.Activities.Registration;
using Stateflows.Activities.Json;
using Stateflows.Common;
using Stateflows.Common.Context;
using Stateflows.Common.Engine;
using Stateflows.Common.Json;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Utilities;

namespace Activity.IntegrationTests.Tests;

[TestClass]
public sealed class InvocationIsolation
{
    private static readonly TimeSpan Deadline = TimeSpan.FromSeconds(5);

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task InvocationModeAndOptionalTriggerRemainStableAfterTheirAsyncScopeCloses(bool streamed)
    {
        using var pending = new PendingTasksScope();
        await using var provider = new ServiceCollection().BuildServiceProvider();
        await using var scope = new NodeScope(provider, new Node(), null!, Guid.NewGuid());
        var root = NewRoot();
        var holder = new Trigger(42).ToEventHolder(new Dictionary<string, EventHeader>
        {
            ["origin"] = new Origin { Value = 42 }
        });
        var token = new TokenHolder<int> { Payload = 42, IsStreamed = streamed };
        ActionContext context;
        using (root.BeginEventExecution())
        using (root.BeginInvocationProvenance(streamed, callRoot: true))
        {
            root.SetEvent(holder);
            context = new ActionContext(root, scope, new Node(), [token]);
            root.ClearEvent();
        }

        await Task.Yield();
        token.IsStreamed = !streamed;
        Assert.AreEqual(streamed ? ExecutionMode.Streamed : ExecutionMode.Transactional, context.ExecutionMode);
        Assert.AreEqual(ExecutionMode.Transactional, root.Context.ExecutionMode);
        if (streamed)
        {
            Assert.IsNull(context.ExecutionTrigger);
            Assert.IsNull(context.ExecutionTriggerId);
            Assert.IsNull(context.Headers);
        }
        else
        {
            Assert.AreSame(holder.BoxedPayload, context.ExecutionTrigger);
            Assert.AreEqual(holder.Id, context.ExecutionTriggerId);
            Assert.AreSame(holder.Headers, context.Headers);
        }
    }

    [TestMethod]
    public async Task ConcurrentEventScopesKeepTheirOwnTriggerAndHeadersAcrossAwaits()
    {
        var root = NewRoot();
        var first = new Trigger(1).ToEventHolder(new Dictionary<string, EventHeader>
        {
            ["origin"] = new Origin { Value = 1 }
        });
        var second = new Trigger(2).ToEventHolder(new Dictionary<string, EventHeader>
        {
            ["origin"] = new Origin { Value = 2 }
        });
        var ready = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        async Task ObserveAsync(EventHolder holder, bool signal)
        {
            root.SetEvent(holder);
            try
            {
                if (signal)
                    ready.SetResult();
                await release.Task.WaitAsync(Deadline);
                Assert.AreSame(holder, root.EventHolder);
                Assert.AreSame(holder, root.ExecutionTriggerHolder);
                Assert.AreEqual(((Trigger)holder.BoxedPayload).Value,
                    ((Origin)root.ExecutionTriggerHolder.Headers["origin"]).Value);
            }
            finally
            {
                root.ClearEvent();
            }
        }

        var one = ObserveAsync(first, true);
        await ready.Task.WaitAsync(Deadline);
        var two = ObserveAsync(second, false);
        Assert.IsNull(root.EventHolder, "A child async operation must not replace its caller's event.");
        release.SetResult();
        await Task.WhenAll(one, two).WaitAsync(Deadline);
        Assert.IsNull(root.EventHolder);
    }

    [TestMethod]
    public void NestedEventScopesRestoreTheOuterEventAndRetainTheExecutionTrigger()
    {
        var root = NewRoot();
        var outer = new Trigger(1).ToEventHolder();
        var inner = new Trigger(2).ToEventHolder();
        root.SetEvent(outer);
        root.SetEvent(inner);
        Assert.AreSame(inner, root.EventHolder);
        Assert.AreSame(outer, root.ExecutionTriggerHolder);
        root.ClearEvent();
        Assert.AreSame(outer, root.EventHolder);
        root.ClearEvent();
        Assert.IsNull(root.EventHolder);
        Assert.Throws<InvalidOperationException>(root.ClearEvent);
    }

    [TestMethod]
    public async Task EveryChildScopeIsDisposedExactlyOnceAndContextsAreUnregistered()
    {
        using var pending = new PendingTasksScope();
        await using var provider = new ServiceCollection().AddScoped<ScopedService>().BuildServiceProvider();
        var root = NewRoot();
        var scope = new NodeScope(provider, new Node(), null!, Guid.NewGuid());
        var first = scope.CreateChildScope();
        var second = scope.CreateChildScope();
        var one = first.ServiceProvider.GetRequiredService<ScopedService>();
        var two = second.ServiceProvider.GetRequiredService<ScopedService>();
        using var firstContext = new ActionContext(root, first, new Node(), []);
        using var secondContext = new ActionContext(root, second, new Node(), []);

        Assert.AreNotSame(one, two);
        Assert.AreEqual(2, scope.ChildScopes.Length);
        await scope.DisposeAsync();
        await scope.DisposeAsync();
        Assert.AreEqual(1, one.Disposals);
        Assert.AreEqual(1, two.Disposals);
        lock (Stateflows.Common.Context.Classes.BaseContext.Instances)
            Assert.IsFalse(Stateflows.Common.Context.Classes.BaseContext.Instances.ContainsKey(root.Id.BehaviorId));
    }

    [TestMethod]
    public async Task DisposingAContextDoesNotDisposeItsInvocationsCancellationSource()
    {
        using var pending = new PendingTasksScope();
        await using var provider = new ServiceCollection().BuildServiceProvider();
        await using var scope = new NodeScope(provider, new Node(), null!, Guid.NewGuid());
        var context = new ActionContext(NewRoot(), scope, new Node(), []);
        context.Dispose();
        scope.CancellationTokenSource.Cancel();
        Assert.IsTrue(scope.CancellationToken.IsCancellationRequested);
        Assert.IsTrue(context.CancellationToken.IsCancellationRequested);
    }

    [TestMethod]
    public async Task ChildCancellationDoesNotCancelOtherInvocations()
    {
        using var pending = new PendingTasksScope();
        await using var provider = new ServiceCollection().BuildServiceProvider();
        await using var scope = new NodeScope(provider, new Node(), null!, Guid.NewGuid());
        var first = scope.CreateChildScope();
        var second = scope.CreateChildScope();
        first.CancellationTokenSource.Cancel();
        Assert.IsTrue(first.CancellationToken.IsCancellationRequested);
        Assert.IsFalse(second.CancellationToken.IsCancellationRequested);
        Assert.IsFalse(scope.CancellationToken.IsCancellationRequested);
        scope.CancellationTokenSource.Cancel();
        Assert.IsTrue(second.CancellationToken.IsCancellationRequested);
    }

    [TestMethod]
    public async Task DisposalWaitsForPendingWorkBeforeClosingScopes()
    {
        using var pending = new PendingTasksScope();
        await using var provider = new ServiceCollection().AddScoped<ScopedService>().BuildServiceProvider();
        var scope = new NodeScope(provider, new Node(), null!, Guid.NewGuid());
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        scope.AddPendingTask(release.Task);
        var firstDisposal = scope.DisposeAsync().AsTask();
        var secondDisposal = scope.DisposeAsync().AsTask();
        Assert.AreSame(firstDisposal, secondDisposal);
        Assert.IsFalse(firstDisposal.IsCompleted);
        var service = scope.CreateChildScope().ServiceProvider.GetRequiredService<ScopedService>();
        Assert.AreEqual(0, service.Disposals);
        release.SetResult();
        await firstDisposal.WaitAsync(Deadline);
        Assert.AreEqual(1, service.Disposals);
        Assert.Throws<ObjectDisposedException>(() => scope.CreateChildScope());
    }

    [TestMethod]
    public async Task OneDisposalFailureDoesNotLeakSiblingOrParentScopes()
    {
        using var pending = new PendingTasksScope();
        await using var provider = new ServiceCollection()
            .AddScoped<ScopedService>()
            .AddScoped<FailingService>()
            .BuildServiceProvider();
        var scope = new NodeScope(provider, new Node(), null!, Guid.NewGuid());
        var parent = scope.ServiceProvider.GetRequiredService<ScopedService>();
        var failed = scope.CreateChildScope().ServiceProvider.GetRequiredService<FailingService>();
        var sibling = scope.CreateChildScope().ServiceProvider.GetRequiredService<ScopedService>();

        var failure = await Assert.ThrowsAsync<AggregateException>(() => scope.DisposeAsync().AsTask());
        Assert.IsTrue(failure.Flatten().InnerExceptions.Any(exception => exception.Message == "Expected disposal failure."));
        Assert.AreEqual(1, failed.Disposals);
        Assert.AreEqual(1, sibling.Disposals);
        Assert.AreEqual(1, parent.Disposals);
        await Assert.ThrowsAsync<AggregateException>(() => scope.DisposeAsync().AsTask());
        Assert.AreEqual(1, failed.Disposals);
    }

    [TestMethod]
    public async Task ContextRetainsItsTriggerAfterTheEventScopeCloses()
    {
        using var pending = new PendingTasksScope();
        await using var provider = new ServiceCollection().BuildServiceProvider();
        await using var scope = new NodeScope(provider, new Node(), null!, Guid.NewGuid());
        var root = NewRoot();
        var holder = new Trigger(17).ToEventHolder();
        root.SetEvent(holder);
        var context = new ActionContext(root, scope, new Node(), []);
        root.ClearEvent();
        Assert.AreEqual(holder.Id, context.ExecutionTriggerId);
        Assert.AreSame(holder.BoxedPayload, context.ExecutionTrigger);
        Assert.AreSame(holder.Headers, context.Headers);
    }

    [TestMethod]
    [DataRow(false, 16)]
    [DataRow(true, 1)]
    [DataRow(true, 2)]
    [DataRow(true, 16)]
    public async Task NodeInvocationsQueueByDefaultAndOverlapOnlyWhenReentrant(bool reentrant, int concurrency)
    {
        using var pending = new PendingTasksScope();
        var probe = new Probe();
        await using var provider = new ServiceCollection()
            .AddSingleton(probe)
            .AddScoped<ScopedService>()
            .AddLogging()
            .AddStateflows(sf => sf.AddLegacyRuntime().AddActivities(activities => activities
                .AddActivity("invocations", activity => activity
                    .AddInputPin<int>(input => input.AddFlow("work"))
                    .AddAction("work", async context =>
                    {
                        var action = (ActionContext)context;
                        var value = context.GetTokensOfType<int>().Single();
                        var service = action.NodeScope.ServiceProvider.GetRequiredService<ScopedService>();
                        probe.ActionServices[value] = service;
                        probe.Triggers[value] = ((Trigger)action.ExecutionTrigger).Value;
                        probe.Entered[value].TrySetResult();
                        await probe.Release[value].Task.WaitAsync(Deadline);
                        CollectionAssert.AreEqual(new[] { value }, context.GetTokensOfType<int>().ToArray());
                        Assert.AreEqual(value, ((Trigger)action.ExecutionTrigger).Value);
                        context.PassAllTokensOn();
                    }, node =>
                    {
                        if (reentrant)
                            node.Reentrant();
                        node.AddFlow<int>("sink", flow => flow
                            .AddGuard<ScopeGuard>()
                            .AddTransformation<int, ScopeTransformation>());
                    })
                    .AddAction("sink", _ => Task.CompletedTask))))
            .BuildServiceProvider();
        var register = provider.GetRequiredService<ActivitiesRegister>();
        provider.GetRequiredService<ITenantAccessor>().CurrentTenantId =
            provider.GetRequiredService<IStateflowsTenantProvider>().GetCurrentTenantId();
        var graph = register.Activities["invocations.current"];
        await using var executor = new Executor(register, graph, provider, concurrency);
        var root = NewRoot();
        await executor.HydrateAsync(root);
        var node = graph.AllNamedNodes["work"];
        root.NodesToExecute.Add(node);

        async Task FireAsync(int value)
        {
            root.SetEvent(new Trigger(value).ToEventHolder());
            try
            {
                await executor.DoHandleNodeAsync(node, null!, executor.NodeScope, [value.ToTokenHolder()]);
            }
            finally
            {
                root.ClearEvent();
            }
        }

        var first = FireAsync(1);
        await Task.WhenAny(first, probe.Entered[1].Task).WaitAsync(Deadline);
        if (first.IsCompleted)
            await first;
        await probe.Entered[1].Task.WaitAsync(Deadline);
        var second = FireAsync(2);
        try
        {
            if (reentrant && concurrency > 1)
                await probe.Entered[2].Task.WaitAsync(Deadline);
            else
                Assert.IsFalse(probe.Entered[2].Task.IsCompleted);
        }
        finally
        {
            probe.Release[1].TrySetResult();
            probe.Release[2].TrySetResult();
            await Task.WhenAll(first, second).WaitAsync(Deadline);
        }

        Assert.AreEqual(1, probe.Triggers[1]);
        Assert.AreEqual(2, probe.Triggers[2]);
        Assert.AreNotSame(probe.ActionServices[1], probe.ActionServices[2]);
        Assert.AreSame(probe.ActionServices[1], probe.GuardServices[1]);
        Assert.AreSame(probe.ActionServices[2], probe.GuardServices[2]);
        Assert.AreSame(probe.ActionServices[1], probe.TransformationServices[1]);
        Assert.AreSame(probe.ActionServices[2], probe.TransformationServices[2]);
        await executor.DehydrateAsync();
        Assert.AreEqual(1, probe.ActionServices[1].Disposals);
        Assert.AreEqual(1, probe.ActionServices[2].Disposals);
    }

    [TestMethod]
    public void JsonRegistrationRetainsTheReentrantActionOption()
    {
        const string json = """
            {
              "formatVersion": "1.0",
              "name": "json-reentrant",
              "nodes": [
                { "kind": "inputPin", "tokenType": "int", "flows": [
                  { "kind": "object", "tokenType": "int", "target": "work" }
                ] },
                {
                  "kind": "action", "name": "work",
                  "implementation": { "clrType": "worker" },
                  "options": ["implicitJoin", "implicitFork", "reentrant"]
                }
              ]
            }
            """;
        using var provider = new ServiceCollection().AddLogging()
            .AddStateflows(sf => sf.AddLegacyRuntime().AddActivities(activities => activities.AddActivityJson(
                json, new JsonOptions().AddType<int>("int").AddType<JsonWorker>("worker"))))
            .BuildServiceProvider();
        var graph = provider.GetRequiredService<ActivitiesRegister>().Activities["json-reentrant.current"];
        Assert.AreEqual(NodeOptions.ActionDefault | NodeOptions.Reentrant, graph.AllNamedNodes["work"].Options);
    }

    [TestMethod]
    public void StructuredNodesRejectActionReentrancy()
    {
        Assert.Throws<Stateflows.Activities.Exceptions.ActivityDefinitionException>(() =>
            new ServiceCollection().AddStateflows(sf => sf.AddActivities(activities => activities
                .AddActivity("reentrant-structure", activity => activity
                    .AddInitial(initial => initial.AddControlFlow("body"))
                    .AddStructuredActivity("body", body => body.Reentrant())))));
    }

    [TestMethod]
    public async Task AFlowCycleDoesNotWaitForItsOwnInvocationGate()
    {
        using var pending = new PendingTasksScope();
        var executions = new List<int>();
        await using var provider = new ServiceCollection().AddLogging()
            .AddStateflows(sf => sf.AddLegacyRuntime().AddActivities(activities => activities
                .AddActivity("cycle", activity => activity
                    .AddInputPin<int>(input => input.AddFlow("work"))
                    .AddAction("work", context =>
                    {
                        var value = context.GetTokensOfType<int>().Single();
                        executions.Add(value);
                        if (value < 3)
                            context.Output(value + 1);
                        return Task.CompletedTask;
                    }, node => node.NoImplicitJoin().AddFlow<int>("work")))))
            .BuildServiceProvider();
        provider.GetRequiredService<ITenantAccessor>().CurrentTenantId =
            provider.GetRequiredService<IStateflowsTenantProvider>().GetCurrentTenantId();
        var register = provider.GetRequiredService<ActivitiesRegister>();
        var graph = register.Activities["cycle.current"];
        await using var executor = new Executor(register, graph, provider);
        var root = NewRoot();
        await executor.HydrateAsync(root);
        var node = graph.AllNamedNodes["work"];
        root.NodesToExecute.Add(node);
        root.SetEvent(new Trigger(1).ToEventHolder());
        try
        {
            await executor.DoHandleNodeAsync(node, null!, executor.NodeScope, [1.ToTokenHolder()]).WaitAsync(Deadline);
        }
        finally
        {
            root.ClearEvent();
        }
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, executions);
    }

    [TestMethod]
    public async Task ParallelPartitionsKeepDistinctActionScopesAndDisposeAllOfThem()
    {
        var probe = new Probe();
        await using var provider = new ServiceCollection()
            .AddSingleton(probe)
            .AddScoped<ScopedService>()
            .AddLogging()
            .AddStateflows(sf => sf.AddLegacyRuntime().AddActivities(activities => activities
                .AddActivity("parallel-scopes", activity => activity
                    .AddInitial(initial => initial.AddControlFlow("produce"))
                    .AddAction("produce", context =>
                    {
                        context.OutputRange(Enumerable.Range(1, 12));
                        return Task.CompletedTask;
                    }, node => node.AddFlow<int>("parallel"))
                    .AddParallelActivity<int>("parallel", body => body
                        .AddInputPin<int>(input => input.AddFlow("work"))
                        .AddAction("work", context =>
                        {
                            var action = (ActionContext)context;
                            var value = context.GetTokensOfType<int>().Single();
                            probe.ActionServices[value] = action.NodeScope.ServiceProvider.GetRequiredService<ScopedService>();
                            context.PassAllTokensOn();
                            return Task.CompletedTask;
                        }, node => node.AddFlow<int>("sink", flow => flow
                            .AddGuard<ScopeGuard>()
                            .AddTransformation<int, ScopeTransformation>()))
                        .AddAction("sink", _ => Task.CompletedTask), 1))))
            .BuildServiceProvider();
        provider.GetRequiredService<ITenantAccessor>().CurrentTenantId =
            provider.GetRequiredService<IStateflowsTenantProvider>().GetCurrentTenantId();
        var id = new BehaviorId(BehaviorType.Activity, "parallel-scopes", Guid.NewGuid().ToString("N"));
        var processor = provider.GetServices<IEventProcessor>().Single(processor =>
            processor.BehaviorTypes.Contains(BehaviorType.Activity));
        var errors = new List<Exception>();
        ResponseHolder.SetResponses(new Dictionary<object, EventHolder>());
        EventStatus result;
        try
        {
            result = await processor.ProcessEventAsync(id, new Initialize().ToEventHolder(), errors);
        }
        finally
        {
            ResponseHolder.ClearResponses();
        }

        Assert.AreEqual(EventStatus.Initialized, result);
        Assert.AreEqual(0, errors.Count);
        Assert.AreEqual(12, probe.ActionServices.Count);
        Assert.AreEqual(12, probe.ActionServices.Values.Distinct().Count());
        foreach (var (value, service) in probe.ActionServices)
        {
            Assert.AreSame(service, probe.GuardServices[value]);
            Assert.AreSame(service, probe.TransformationServices[value]);
            Assert.AreEqual(1, service.Disposals);
        }
        lock (Stateflows.Common.Context.Classes.BaseContext.Instances)
            Assert.IsFalse(Stateflows.Common.Context.Classes.BaseContext.Instances.ContainsKey(id));
    }

    private static RootContext NewRoot()
        => new(new StateflowsContext(new BehaviorId(BehaviorType.Activity, "invocations", Guid.NewGuid().ToString("N"))));

    private sealed record Trigger(int Value);
    private sealed class Origin : EventHeader
    {
        public int Value { get; init; }
    }

    public sealed class ScopedService : IDisposable
    {
        public int Disposals { get; private set; }
        public void Dispose() => Disposals++;
    }

    public sealed class FailingService : IDisposable
    {
        public int Disposals { get; private set; }
        public void Dispose()
        {
            Disposals++;
            throw new InvalidOperationException("Expected disposal failure.");
        }
    }

    public sealed class Probe
    {
        public Dictionary<int, TaskCompletionSource> Entered { get; } = new()
        {
            [1] = new(TaskCreationOptions.RunContinuationsAsynchronously),
            [2] = new(TaskCreationOptions.RunContinuationsAsynchronously)
        };
        public Dictionary<int, TaskCompletionSource> Release { get; } = new()
        {
            [1] = new(TaskCreationOptions.RunContinuationsAsynchronously),
            [2] = new(TaskCreationOptions.RunContinuationsAsynchronously)
        };
        public ConcurrentDictionary<int, ScopedService> ActionServices { get; } = new();
        public ConcurrentDictionary<int, ScopedService> GuardServices { get; } = new();
        public ConcurrentDictionary<int, ScopedService> TransformationServices { get; } = new();
        public ConcurrentDictionary<int, int> Triggers { get; } = new();
    }

    public sealed class ScopeGuard(Probe probe, ScopedService service) : IFlowGuard<int>
    {
        public Task<bool> GuardAsync(int token)
        {
            probe.GuardServices[token] = service;
            return Task.FromResult(true);
        }
    }

    public sealed class ScopeTransformation(Probe probe, ScopedService service) : IFlowTransformation<int, int>
    {
        public Task<int> TransformAsync(int token)
        {
            probe.TransformationServices[token] = service;
            return Task.FromResult(token);
        }
    }

    public sealed class JsonWorker(IInputToken<int> input) : IActionNode
    {
        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _ = input.Token;
            return Task.CompletedTask;
        }
    }
}
