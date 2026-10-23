using Microsoft.Extensions.DependencyInjection;
using StateMachine.IntegrationTests.Utils;
using Stateflows.Actions;
using Stateflows.Activities;
using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.Common.Context.Interfaces;
using Stateflows.Common.Engine;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Utilities;
using Stateflows.Entities;
using Stateflows.Entities.Attributes;

namespace StateMachine.IntegrationTests.Tests;

[TestClass]
public class PendingPublications : StateflowsTestClass
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);
    private readonly TaskCompletionSource published = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource release = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource nestedStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource releaseNested = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly Exception publicationFailure = new ApplicationException("publication failure");
    private readonly Exception processingFailure = new ApplicationException("processing failure");
    private bool specialized;
    private bool failPublication;
    private bool failProcessing;
    private bool cascade;
    private int completed;

    public record Notification;
    public record NestedNotification;

    public interface ITemplate
    {
        [Field]
        int Value { get; set; }
    }

    [TestInitialize]
    public override void Initialize()
    {
        ServiceCollection.AddScoped<Probe>();
        base.Initialize();
    }

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();

    protected override void InitializeStateflows(IStateflowsBuilder builder)
    {
        builder.AddInterceptor(provider => new Publisher(this, provider.GetRequiredService<Probe>()));
        builder.AddActions(actions => actions.AddAction("publishing", context =>
        {
            PublishSpecialized(context.Behavior);
            return Task.CompletedTask;
        }));
        builder.AddActivities(activities => activities.AddActivity("publishing", activity => activity
            .AddInitial(initial => initial.AddControlFlow("work"))
            .AddAction("work", context =>
            {
                PublishSpecialized(context.Behavior);
                return Task.CompletedTask;
            }, node => node.AddControlFlow<FinalNode>())
            .AddFinal()));
        builder.AddStateMachines(machines => machines.AddStateMachine("publishing", machine => machine
            .AddInitialState("ready", state => state.AddOnEntry(context =>
            {
                PublishSpecialized(context.Behavior);
                return Task.CompletedTask;
            }))));
        builder.AddEntities(entities => entities.AddEntity<ITemplate>("publishing", entity =>
            entity.AddDefaultInitializer(context => context.Entity.Value = 42)));
    }

    private void PublishSpecialized(IBehaviorContext context)
    {
        if (!specialized)
            return;

        context.Publish(new Notification());
        published.TrySetResult();
    }

    private async Task<(EventStatus Status, List<Exception> Exceptions)> ExecuteAsync(string type)
    {
        ServiceProvider.GetRequiredService<ITenantAccessor>().CurrentTenantId =
            ServiceProvider.GetRequiredService<IStateflowsTenantProvider>().GetCurrentTenantId();
        var processor = ServiceProvider.GetServices<IEventProcessor>()
            .Single(processor => processor.BehaviorTypes.Contains(type));
        var exceptions = new List<Exception>();
        var status = await processor.ProcessEventAsync(new BehaviorId(type, "publishing", "x"),
            new Initialize().ToEventHolder(), exceptions);
        return (status, exceptions);
    }

    [TestMethod]
    [DataRow(BehaviorType.Action, false)]
    [DataRow(BehaviorType.Activity, false)]
    [DataRow(BehaviorType.StateMachine, false)]
    [DataRow(BehaviorType.Entity, false)]
    [DataRow(BehaviorType.Action, true)]
    [DataRow(BehaviorType.Activity, true)]
    [DataRow(BehaviorType.StateMachine, true)]
    public async Task PublishReturnsWithoutBlocking_AndExecutionDrainsNestedPublications(string type, bool useSpecialized)
    {
        specialized = useSpecialized;
        cascade = true;
        var execution = Task.Run(() => ExecuteAsync(type));
        try
        {
            await published.Task.WaitAsync(Timeout);
            Assert.IsFalse(execution.IsCompleted, "Execution must wait for the publication interceptor.");
            Assert.AreEqual(0, completed);
            release.SetResult();
            await nestedStarted.Task.WaitAsync(Timeout);
            Assert.IsFalse(execution.IsCompleted, "Publications added during draining must also be awaited.");
        }
        finally
        {
            release.TrySetResult();
            releaseNested.TrySetResult();
        }

        var (status, exceptions) = await execution.WaitAsync(Timeout);
        Assert.AreNotEqual(EventStatus.Failed, status);
        Assert.AreEqual(0, exceptions.Count);
        Assert.AreEqual(2, completed);
    }

    [TestMethod]
    [DataRow(BehaviorType.Action, false)]
    [DataRow(BehaviorType.Activity, false)]
    [DataRow(BehaviorType.StateMachine, false)]
    [DataRow(BehaviorType.Entity, false)]
    [DataRow(BehaviorType.Action, true)]
    [DataRow(BehaviorType.Activity, true)]
    [DataRow(BehaviorType.StateMachine, true)]
    public async Task PublicationFailureFailsExecution_AndDoesNotPersist(string type, bool useSpecialized)
    {
        specialized = useSpecialized;
        failPublication = true;
        release.SetResult();
        var (status, exceptions) = await ExecuteAsync(type).WaitAsync(Timeout);
        Assert.AreEqual(EventStatus.Failed, status);
        Assert.AreEqual(1, exceptions.Count(exception => ReferenceEquals(exception, publicationFailure)));
        ServiceProvider.GetRequiredService<ITenantAccessor>().CurrentTenantId =
            ServiceProvider.GetRequiredService<IStateflowsTenantProvider>().GetCurrentTenantId();
        var stored = await ServiceProvider.GetRequiredService<IStateflowsStorage>()
            .HydrateAsync(new BehaviorId(type, "publishing", "x"));
        Assert.IsTrue(stored.Status is BehaviorStatus.Unknown or BehaviorStatus.NotInitialized);
    }

    [TestMethod]
    [DataRow(BehaviorType.Action)]
    [DataRow(BehaviorType.Activity)]
    [DataRow(BehaviorType.StateMachine)]
    [DataRow(BehaviorType.Entity)]
    public async Task CleanupPreservesProcessingAndPublicationFailures(string type)
    {
        failPublication = true;
        failProcessing = true;
        var execution = Task.Run(() => ExecuteAsync(type));
        try
        {
            await published.Task.WaitAsync(Timeout);
            Assert.IsFalse(execution.IsCompleted);
        }
        finally
        {
            release.TrySetResult();
        }

        var (status, exceptions) = await execution.WaitAsync(Timeout);
        Assert.AreEqual(EventStatus.Failed, status);
        Assert.AreSame(processingFailure, exceptions[0]);
        Assert.AreEqual(1, exceptions.Count(exception => ReferenceEquals(exception, publicationFailure)));
    }

    [TestMethod]
    public async Task ConcurrentDrainsObserveInFlightTasks_AndRecordCancellationOnce()
    {
        using var scope = new PendingTasksScope();
        var task = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        scope.Tasks.Add(task.Task);
        var first = scope.Tasks.DrainAsync();
        var second = scope.Tasks.DrainAsync();
        Assert.IsFalse(first.IsCompleted);
        Assert.IsFalse(second.IsCompleted);
        task.SetCanceled();
        await Task.WhenAll(first, second).WaitAsync(Timeout);
        await scope.Tasks.DrainAsync();
        Assert.AreEqual(1, scope.Tasks.Exceptions.Length);
        Assert.IsInstanceOfType<TaskCanceledException>(scope.Tasks.Exceptions[0]);
    }

    [TestMethod]
    public async Task DrainingContinuesAfterFailure_AndReportsFailuresFromLaterBatches()
    {
        using var scope = new PendingTasksScope();
        async Task EnqueueAsync()
        {
            await release.Task;
            scope.Tasks.Add(Task.FromException(processingFailure));
        }
        scope.Tasks.Add(Task.FromException(publicationFailure));
        scope.Tasks.Add(EnqueueAsync());
        var drain = scope.Tasks.DrainAsync();
        Assert.IsFalse(drain.IsCompleted);
        release.SetResult();
        await drain.WaitAsync(Timeout);
        CollectionAssert.AreEquivalent(new[] { publicationFailure, processingFailure }, scope.Tasks.Exceptions);
    }

    [TestMethod]
    public async Task NestedExecutionScopesRestoreTheParent_AndDoNotWaitForItsTasks()
    {
        Assert.IsNull(PendingTasksScope.Current);
        using (var outer = new PendingTasksScope())
        {
            var task = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            outer.Tasks.Add(task.Task);
            using (var inner = new PendingTasksScope())
            {
                Assert.AreSame(inner.Tasks, PendingTasksScope.RequireCurrent());
                await inner.Tasks.DrainAsync().WaitAsync(Timeout);
            }
            Assert.AreSame(outer.Tasks, PendingTasksScope.RequireCurrent());
            task.SetResult();
            await outer.Tasks.DrainAsync().WaitAsync(Timeout);
        }
        Assert.IsNull(PendingTasksScope.Current);
    }

    [TestMethod]
    public async Task ServiceScopeDrainsPendingWorkBeforeDisposingDependencies_EvenOnFailure()
    {
        using var execution = new PendingTasksScope();
        var scope = new StateflowsServiceScope(ServiceProvider.CreateAsyncScope());
        var probe = scope.ServiceProvider.GetRequiredService<Probe>();
        async Task PublishAsync()
        {
            await release.Task;
            Assert.IsFalse(probe.Disposed);
            throw publicationFailure;
        }
        execution.Tasks.Add(PublishAsync());
        var disposal = scope.DisposeAsync().AsTask();
        Assert.IsFalse(disposal.IsCompleted);
        Assert.IsFalse(probe.Disposed);
        release.SetResult();
        await disposal.WaitAsync(Timeout);
        Assert.IsTrue(probe.Disposed);
        Assert.AreSame(publicationFailure, execution.Tasks.Exceptions.Single());
    }

    private sealed class Probe : IDisposable
    {
        public bool Disposed { get; private set; }
        public void Dispose() => Disposed = true;
    }

    private sealed class Publisher(PendingPublications owner, Probe probe) : BehaviorInterceptor
    {
        public override async Task<EventStatus> ProcessEventAsync<TEvent>(IEventContext<TEvent> context, Func<Task<EventStatus>> next)
        {
            if (!owner.specialized)
            {
                context.Behavior.Publish(new Notification());
                owner.published.TrySetResult();
            }
            if (owner.failProcessing)
                throw owner.processingFailure;
            return await next();
        }

        public override async Task NotificationPublishedAsync<TNotification>(
            IBehaviorActionContext context, TNotification notification, IDictionary<string, EventHeader> headers)
        {
            if (notification is Notification)
            {
                await owner.release.Task;
                Assert.IsFalse(probe.Disposed, "Publication must complete before its service scope is disposed.");
                if (owner.failPublication)
                    throw owner.publicationFailure;
                if (owner.cascade)
                    context.Behavior.Publish(new NestedNotification());
            }
            else if (notification is NestedNotification)
            {
                owner.nestedStarted.TrySetResult();
                await owner.releaseNested.Task;
                Assert.IsFalse(probe.Disposed);
            }
            else
            {
                return;
            }
            Interlocked.Increment(ref owner.completed);
        }
    }
}
