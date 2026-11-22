using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using StateMachine.IntegrationTests.Utils;
using Stateflows;
using Stateflows.Actions;
using Stateflows.Common;
using Stateflows.Common.Context.Interfaces;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Utilities;
using Stateflows.Entities;

namespace Action.IntegrationTests.Tests;

[TestClass]
public class BehaviorOperationObservation : StateflowsTestClass
{
    private readonly Log log = new();
    private readonly BehaviorId target = new("fake", "target", "x");
    private IBehavior? captured;
    private int executions;
    private bool testDirectHandles;
    private int dispatchExecutions;
    private readonly TaskCompletionSource releaseDetached = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource detachedDone = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource bothStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private int parallelStarted;
    private Exception? observedFailure;
    public sealed record Question : IRequest<int>;

    public sealed class Log
    {
        public readonly ConcurrentQueue<string> Trace = new();
        public readonly ConcurrentQueue<BehaviorCallContext> Calls = new();
        public readonly ConcurrentQueue<EntityOperationContext> Entities = new();
        public readonly ConcurrentQueue<BehaviorCallContext> Dispatches = new();
        public readonly ConcurrentQueue<Guid> ReceivedIds = new();
        public string? Violation;
        public readonly TaskCompletionSource SendBlocked = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public readonly TaskCompletionSource ReleaseSend = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public readonly TaskCompletionSource DispatchStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    public sealed class Observer(Log log) : IBehaviorObserver
    {
        public async Task OnSendAsync(BehaviorCallContext context, Func<Task<SendResult>> next)
        {
            log.Calls.Enqueue(context);
            log.Trace.Enqueue("send:before");
            if (log.Violation == "missing") return;
            if (log.Violation == "delay" && Equals(context.Event.BoxedPayload, "delayed"))
            {
                log.SendBlocked.TrySetResult();
                await log.ReleaseSend.Task;
            }
            if (log.Violation == "swallow")
            {
                try { await next(); }
                catch (InvalidOperationException) { }
                return;
            }
            await next();
            if (log.Violation == "double") await next();
            if (log.Violation == "throw-after") throw new InvalidOperationException("observer failed after dispatch");
            log.Trace.Enqueue("send:after");
        }

        public async Task OnRequestAsync<T>(BehaviorCallContext context, Func<Task<RequestResult<T>>> next)
        {
            log.Calls.Enqueue(context);
            log.Trace.Enqueue("request:before");
            await next();
            log.Trace.Enqueue("request:after");
        }
        public async Task OnEntityOperationAsync<T>(EntityOperationContext context, Func<Task<T>> next)
        {
            log.Entities.Enqueue(context);
            log.Trace.Enqueue("entity:before");
            await next();
            log.Trace.Enqueue("entity:after");
        }

        public void OnDispatch(BehaviorCallContext context, System.Action next)
        {
            log.Dispatches.Enqueue(context);
            log.DispatchStarted.TrySetResult();
            if (log.Violation == "sync-missing") return;
            next();
            if (log.Violation == "sync-double") next();
        }
    }

    public sealed class ReceiptInterceptor(Log log) : IBehaviorInterceptor
    {
        public Task<EventStatus> ProcessEventAsync<T>(IEventContext<T> context, Func<Task<EventStatus>> next)
        {
            if (context.Behavior.Id.Name == "recipient")
                log.ReceivedIds.Enqueue(context.EventId);
            return next();
        }
        public Task NotificationPublishedAsync<T>(IBehaviorActionContext context, T notification, IDictionary<string, EventHeader> headers)
            => Task.CompletedTask;
        public Task RequestRespondedAsync<TRequest, TResponse>(IBehaviorActionContext context, TRequest request, TResponse response)
            where TRequest : IRequest<TResponse> => Task.CompletedTask;
    }
    public sealed class Provider : IBehaviorProvider
    {
        public bool IsLocal => true;
        public IEnumerable<BehaviorClass> BehaviorClasses => [];
        public event ActionAsync<IBehaviorProvider>? BehaviorClassesChanged;
        public bool TryProvideBehavior(BehaviorId id, out IBehavior behavior)
        {
            var found = id.Type == "fake" || id.Type == BehaviorType.Entity &&
                id.Name is "source.entity.owned" or "declared-mutation.entity.owned";
            behavior = found ? new Fake(id) : null!;
            return found;
        }
    }

    public sealed class Fake(BehaviorId id) : IBehavior
    {
        public BehaviorId Id => id;
        public Task<SendResult> SendAsync<T>(T value, IDictionary<string, EventHeader>? headers = null)
            => value is EventHolder holder && Equals(holder.BoxedPayload, "target-failure")
                ? Task.FromException<SendResult>(new InvalidOperationException("target failed"))
                : Task.FromResult(new SendResult(EventStatus.Consumed, new EventValidation(true)));
        public Task<SendResult> DispatchAsync<T>(T value, IDictionary<string, EventHeader>? headers = null)
            => Task.FromResult(new SendResult(EventStatus.Dispatched, new EventValidation(true)));
        public Task<RequestResult<T>> RequestAsync<T>(IRequest<T> request, IDictionary<string, EventHeader>? headers = null)
        {
            object? value = request switch
            {
                FieldStateRequest<int> => new FieldState<int> { Name = "count", Value = 7 },
                ProjectionRequest<int> => 9,
                _ => default(T)
            };
            return Task.FromResult(new RequestResult<T>(((T)value!).ToEventHolder(), EventStatus.Consumed));
        }
        public Task<IWatcher> WatchAsync<T>(Func<T, Task> handler, DateTime? since = null) => throw new NotSupportedException();
        public Task<IWatcher> WatchAsync(string[] names, Func<EventHolder, Task> handler, DateTime? since = null) => throw new NotSupportedException();
        public Task<IEnumerable<T>> GetNotificationsAsync<T>(DateTime? since = null) => throw new NotSupportedException();
        public Task<IEnumerable<EventHolder>> GetNotificationsAsync(string[] names, DateTime? since = null) => throw new NotSupportedException();
        public void Dispose() { }
    }

    [TestInitialize]
    public override void Initialize()
    {
        ServiceCollection.AddSingleton(log);
        ServiceCollection.AddSingleton<IBehaviorProvider, Provider>();
        ServiceCollection.AddScoped<IBehaviorInterceptor, ReceiptInterceptor>();
        base.Initialize();
    }

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();

    protected override void InitializeStateflows(IStateflowsBuilder builder)
    {
        builder.AddBehaviorObserver<Observer>();
        builder.AddActions(actions => actions.AddAction("source", async context =>
        {
            if (Interlocked.Increment(ref executions) != 1) return;
            Assert.IsTrue(context.TryLocateBehavior(target, out var behavior));
            captured = behavior;
            Assert.AreEqual(EventStatus.Consumed, (await behavior.SendAsync("hello")).Status);
            Assert.IsTrue(await context.Behavior.TryMutateAsync("owned", "mutation"));
            Assert.IsTrue(await context.Behavior.TrySetFieldAsync("owned", "count", 4));
            Assert.AreEqual((true, 7), await context.Behavior.TryGetFieldAsync<int>("owned", "count"));
            Assert.AreEqual((true, 9), await context.Behavior.TryGetProjectionAsync<int>("owned"));
            Assert.IsFalse(await context.Behavior.TryMutateAsync("missing", "mutation"));
            if (testDirectHandles)
            {
                Assert.AreEqual(EventStatus.Consumed, (await behavior.RequestAsync(new Question())).Status);
                var factory = context.Behavior.ServiceProvider.GetRequiredService<IBehaviorFactory>();
                using var raw = factory.CreateBehavior(new ActionId("recipient", "x"));
                await raw.SendAsync("raw");
                await raw.GetStatusAsync();
                Assert.IsTrue(context.TryLocateBehavior(new ActionId("recipient", "x"), out var located));
                await located.SendAsync("located");
                await located.GetStatusAsync();
            }
            await context.Behavior.DispatchAsync("fire-and-forget");
        }));
        builder.AddActions(actions => actions.AddAction("recipient", _ => Task.CompletedTask));
        builder.AddActions(actions => actions
            .AddAction("parallel", async context =>
            {
                Assert.IsTrue(context.TryLocateBehavior(target, out var behavior));
                if (Interlocked.Increment(ref parallelStarted) == 2) bothStarted.TrySetResult();
                await bothStarted.Task.WaitAsync(TimeSpan.FromSeconds(10));
                await behavior.SendAsync(context.Behavior.Id.Instance);
            })
            .AddAction("detached", context =>
            {
                Assert.IsTrue(context.TryLocateBehavior(target, out var behavior));
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await releaseDetached.Task;
                        await behavior.SendAsync("detached");
                        detachedDone.TrySetResult();
                    }
                    catch (Exception exception) { detachedDone.TrySetException(exception); }
                });
                return Task.CompletedTask;
            })
            .AddAction("observer-failure", async context =>
            {
                Assert.IsTrue(context.TryLocateBehavior(target, out var behavior));
                try { await behavior.SendAsync(log.Violation == "swallow" ? "target-failure" : "failure"); }
                catch (Exception exception) { observedFailure = exception; throw; }
            })
            .AddAction("missing-owner", async context =>
            {
                var operations = (IEntityOperations<IParentBehaviorContext>)context;
                Assert.IsFalse((await operations.TryGetFieldAsync<int>("missing", "count")).Success);
            })
            .AddAction("declared-mutation", async context =>
            {
                object mutation = "runtime-string";
                Assert.IsTrue(await context.Behavior.TryMutateAsync("owned", mutation));
            })
            .AddAction("dispatch", async context =>
            {
                if (Interlocked.Increment(ref dispatchExecutions) != 1) return;
                Assert.IsTrue(context.TryLocateBehavior(target, out var behavior));
                var delayed = behavior.SendAsync("delayed");
                await log.SendBlocked.Task.WaitAsync(TimeSpan.FromSeconds(10));
                await context.Behavior.DispatchAsync("fire-and-forget");
                await delayed;
            }));
    }

    [TestMethod]
    public async Task RawFactoryHandlesAndLocatedRequestsAreObserved()
    {
        testDirectHandles = true;
        Assert.AreEqual(EventStatus.Consumed, await ExecuteAsync());
        var calls = log.Calls.ToArray();
        var recipient = calls.Where(c => c.Target.Name == "recipient").ToArray();
        Assert.AreEqual(4, recipient.Length);
        CollectionAssert.AreEquivalent(recipient.Select(c => c.Event.Id).ToArray(), log.ReceivedIds.ToArray());
        Assert.AreEqual(1, calls.Count(c => c.Event.BoxedPayload is Question));
        Assert.IsTrue(calls.All(c => c.Source.Name == "source"));
    }

    private async Task<EventStatus> ExecuteAsync()
    {
        Assert.IsTrue(ActionLocator.TryLocateAction(new ActionId("source", "x"), out var action));
        return (await action.ExecuteAsync()).Status;
    }

    [TestMethod]
    public async Task LogicalOperationsCorrelateUnderlyingCallsAndDoNotLeakToExternalUse()
    {
        Assert.AreEqual(EventStatus.Consumed, await ExecuteAsync());
        var entities = log.Entities.ToArray();
        CollectionAssert.AreEqual(new[] { EntityOperationKind.Mutation, EntityOperationKind.FieldWrite,
            EntityOperationKind.FieldRead, EntityOperationKind.ProjectionRead, EntityOperationKind.Mutation },
            entities.Select(e => e.Kind).ToArray());
        var calls = log.Calls.ToArray();
        Assert.AreEqual(5, calls.Length);
        Assert.IsNull(calls[0].ParentOperationId);
        for (var i = 0; i < 4; i++)
            Assert.AreEqual(entities[i].OperationId, calls[i + 1].ParentOperationId);
        Assert.IsTrue(calls.All(c => c.Source.Name == "source"));
        CollectionAssert.AreEqual(new[]
        {
            $"entity:source.entity.owned/mutation:{Event.GetName(typeof(string))}",
            "entity:source.entity.owned/field:count",
            "entity:source.entity.owned/field:count",
            $"entity:source.entity.owned/projection:{Event.GetName(typeof(int))}",
            $"entity:source.entity.missing/mutation:{Event.GetName(typeof(string))}"
        }, entities.Select(entity => entity.DefinitionElementId).ToArray());
        Assert.AreEqual(1, log.Dispatches.Count);
        Assert.IsTrue(log.Dispatches.All(c => c.Dispatched == true));
        Assert.IsNotNull(captured);
        await captured.SendAsync("external");
        Assert.AreEqual(5, log.Calls.Count);
    }

    [TestMethod]
    [DataRow("missing")]
    [DataRow("double")]
    public async Task InvalidContinuationsAreNotReportedAsSuccess(string violation)
    {
        log.Violation = violation;
        Assert.AreEqual(EventStatus.Failed, await ExecuteAsync());
        Assert.IsTrue(log.Calls.Count >= 1);
        Assert.IsFalse(log.Trace.Contains("send:after"));
    }

    [TestMethod]
    [DataRow("sync-missing")]
    [DataRow("sync-double")]
    public async Task SynchronousContinuationsMustRunExactlyOnce(string violation)
    {
        log.Violation = violation;
        Assert.AreEqual(EventStatus.Failed, await ExecuteAsync());
    }

    [TestMethod]
    public async Task ConcurrentInvocationsKeepTheirOwnSource()
    {
        Assert.IsTrue(ActionLocator.TryLocateAction(new ActionId("parallel", "one"), out var first));
        Assert.IsTrue(ActionLocator.TryLocateAction(new ActionId("parallel", "two"), out var second));
        var results = await Task.WhenAll(first.ExecuteAsync(), second.ExecuteAsync()).WaitAsync(TimeSpan.FromSeconds(15));
        Assert.IsTrue(results.All(result => result.Status == EventStatus.Consumed));
        var calls = log.Calls.Where(call => call.Target == target).ToArray();
        Assert.AreEqual(2, calls.Length);
        Assert.IsTrue(calls.All(call => Equals(call.Event.BoxedPayload, call.Source.Instance)));
        CollectionAssert.AreEquivalent(new[] { "one", "two" }, calls.Select(call => call.Source.Instance).ToArray());
    }

    [TestMethod]
    public async Task DetachedWorkCannotReuseCompletedSource()
    {
        Assert.IsTrue(ActionLocator.TryLocateAction(new ActionId("detached", "one"), out var action));
        Assert.AreEqual(EventStatus.Consumed, (await action.ExecuteAsync()).Status);
        releaseDetached.TrySetResult();
        await detachedDone.Task.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.IsFalse(log.Calls.Any(call => Equals(call.Event.BoxedPayload, "detached")));
    }

    [TestMethod]
    public async Task AwaitedObserverFailurePropagatesToCaller()
    {
        log.Violation = "throw-after";
        Assert.IsTrue(ActionLocator.TryLocateAction(new ActionId("observer-failure", "one"), out var action));
        Assert.AreEqual(EventStatus.Failed, (await action.ExecuteAsync()).Status);
        Assert.IsInstanceOfType<InvalidOperationException>(observedFailure);
        Assert.AreEqual("observer failed after dispatch", observedFailure.Message);
    }

    [TestMethod]
    public async Task SwallowedTargetFailureIsRethrownToCaller()
    {
        log.Violation = "swallow";
        Assert.IsTrue(ActionLocator.TryLocateAction(new ActionId("observer-failure", "one"), out var action));
        Assert.AreEqual(EventStatus.Failed, (await action.ExecuteAsync()).Status);
        Assert.IsInstanceOfType<InvalidOperationException>(observedFailure);
        Assert.AreEqual("target failed", observedFailure.Message);
    }

    [TestMethod]
    public async Task MissingOwnerHasNoDefinitionIdentity()
    {
        Assert.IsTrue(ActionLocator.TryLocateAction(new ActionId("missing-owner", "one"), out var action));
        Assert.AreEqual(EventStatus.Consumed, (await action.ExecuteAsync()).Status);
        var operation = log.Entities.Single();
        Assert.IsNull(operation.Owner);
        Assert.IsNull(operation.Target);
        Assert.IsNull(operation.DefinitionElementId);
    }

    [TestMethod]
    public async Task MutationDefinitionUsesDeclaredTypeRatherThanRuntimePayloadType()
    {
        Assert.IsTrue(ActionLocator.TryLocateAction(new ActionId("declared-mutation", "one"), out var action));
        Assert.AreEqual(EventStatus.Consumed, (await action.ExecuteAsync()).Status);
        Assert.AreEqual($"entity:declared-mutation.entity.owned/mutation:{Event.GetName(typeof(object))}",
            log.Entities.Single().DefinitionElementId);
    }

    [TestMethod]
    public async Task DelayedAsyncSendObserverDoesNotBlockSynchronousDispatch()
    {
        log.Violation = "delay";
        Assert.IsTrue(ActionLocator.TryLocateAction(new ActionId("dispatch", "one"), out var action));
        var execution = action.ExecuteAsync();
        try
        {
            await log.SendBlocked.Task.WaitAsync(TimeSpan.FromSeconds(10));
            await log.DispatchStarted.Task.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.IsFalse(execution.IsCompleted);
            Assert.IsTrue(log.Dispatches.Any(call => Equals(call.Event.BoxedPayload, "fire-and-forget")));
            Assert.IsFalse(log.Calls.Any(call => Equals(call.Event.BoxedPayload, "fire-and-forget")));
        }
        finally { log.ReleaseSend.TrySetResult(); }
        Assert.AreEqual(EventStatus.Consumed, (await execution.WaitAsync(TimeSpan.FromSeconds(10))).Status);
    }
}
