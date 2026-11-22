using Microsoft.Extensions.DependencyInjection;
using StateMachine.IntegrationTests.Utils;
using Stateflows.Actions;
using Stateflows.Common;
using Stateflows.Common.Interfaces;

namespace Action.IntegrationTests.Tests;

[TestClass]
public sealed class ClientDispatch : StateflowsTestClass
{
    private readonly TaskCompletionSource processingStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource releaseProcessing = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource processingCompleted = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly List<string> interceptorTrace = [];
    private bool shortCircuit;

    [TestInitialize]
    public override void Initialize() => base.Initialize();

    [TestCleanup]
    public override void Cleanup()
    {
        releaseProcessing.TrySetResult();
        base.Cleanup();
    }

    protected override void InitializeStateflows(IStateflowsBuilder builder)
    {
        builder.ServiceCollection.AddSingleton<IStateflowsClientInterceptor>(new Probe(this));
        builder.AddActions(actions => actions.AddAction("dispatch-target", async _ =>
        {
            processingStarted.TrySetResult();
            await releaseProcessing.Task;
            processingCompleted.TrySetResult();
        }));
    }

    [TestMethod]
    public async Task DispatchAwaitsInterceptorsButNotEventProcessing()
    {
        Assert.IsTrue(ServiceProvider.GetRequiredService<IBehaviorLocator>()
            .TryLocateBehavior(new ActionId("dispatch-target", "instance"), out var behavior));

        var dispatchTask = behavior.DispatchAsync("event");
        try
        {
            await processingStarted.Task.WaitAsync(TimeSpan.FromSeconds(10));
            var result = await dispatchTask.WaitAsync(TimeSpan.FromSeconds(10));

            Assert.AreEqual(EventStatus.Dispatched, result.Status);
            Assert.IsFalse(processingCompleted.Task.IsCompleted);
            CollectionAssert.AreEqual(new[] { "before", "after" }, interceptorTrace);
        }
        finally
        {
            releaseProcessing.TrySetResult();
        }

        await processingCompleted.Task.WaitAsync(TimeSpan.FromSeconds(10));
    }

    [TestMethod]
    public async Task DispatchShortCircuitDoesNotStartProcessing()
    {
        shortCircuit = true;
        Assert.IsTrue(ServiceProvider.GetRequiredService<IBehaviorLocator>()
            .TryLocateBehavior(new ActionId("dispatch-target", "instance"), out var behavior));

        var result = await behavior.DispatchAsync("event");

        Assert.AreEqual(EventStatus.Undelivered, result.Status);
        Assert.IsFalse(processingStarted.Task.IsCompleted);
        CollectionAssert.AreEqual(new[] { "before" }, interceptorTrace);
    }

    private sealed class Probe(ClientDispatch owner) : IStateflowsClientInterceptor
    {
        public async Task DispatchEventAsync<TEvent>(
            BehaviorId targetBehaviorId,
            TEvent @event,
            IDictionary<string, EventHeader> headers,
            Func<Task> next)
        {
            owner.interceptorTrace.Add("before");
            if (owner.shortCircuit) return;
            await next();
            owner.interceptorTrace.Add("after");
        }
    }
}
