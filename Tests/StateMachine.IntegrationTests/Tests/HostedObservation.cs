using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Activities;
using Stateflows.Common;
using Stateflows.Common.Utilities;
using StateMachine.IntegrationTests.Utils;

namespace StateMachine.IntegrationTests.Tests;

[TestClass]
public class HostedObservation : StateflowsTestClass
{
    private sealed record Ping;
    private sealed record Leave;
    private readonly ConcurrentQueue<(string Operation, string Id, Guid EventId)> deliveries = new();
    private readonly TaskCompletionSource entered = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource release = new(TaskCreationOptions.RunContinuationsAsynchronously);

    [TestInitialize] public override void Initialize() => base.Initialize();
    [TestCleanup] public override void Cleanup() { release.TrySetResult(); base.Cleanup(); }

    protected override void InitializeStateflows(IStateflowsBuilder builder) => builder.AddStateMachines(machines => machines
        .AddStateMachine("hosted-observation", machine => machine
            .AddObserver((_, _) => new Recorder(this))
            .AddInitialState("active", state => state
                .AddDoActivity(activity => activity.AddAcceptEventAction<Ping>(async _ =>
                {
                    entered.TrySetResult();
                    await release.Task;
                }))
                .AddTransition<Leave>("done"))
            .AddState("done"))
        .AddStateMachine("behavior-metadata", machine => machine
            .AddInitialState("source", state => state
                .AddOnEntryAction(_ => Task.CompletedTask)
                .AddTransition<Ping>("target", transition => transition
                    .AddGuardExpression(expression => expression.AddOrExpression(branch => branch
                        .AddGuard(_ => false)
                        .AddGuardAction(context => { context.Output(true); return Task.CompletedTask; })))
                    .AddEffectAction(_ => Task.CompletedTask)))
            .AddState("target")));

    [TestMethod]
    public async Task HostedScopesIdentifySchedulingWithoutWaitingForChildCompletion()
    {
        var metadata = new Metadata();
        await ServiceProvider.GetRequiredService<IStateMachinesRegister>()
            .VisitStateMachineAsync("hosted-observation", 1, metadata);
        Assert.IsTrue(StateMachineLocator.TryLocateStateMachine(new StateMachineId("hosted-observation", "test"), out var machine));
        await machine.SendAsync(new Initialize());
        var delivery = new Ping().ToEventHolder();
        var forwarded = await delivery.SendAsync(machine).WaitAsync(TimeSpan.FromSeconds(5));
        Assert.AreEqual(EventStatus.Forwarded, forwarded.Status);
        await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.IsFalse(release.Task.IsCompleted);
        release.TrySetResult();
        await machine.SendAsync(new Leave());
        CollectionAssert.AreEqual(new[] { "initialize", "forward", "finalize" },
            deliveries.Select(item => item.Operation).ToArray());
        Assert.IsTrue(deliveries.All(item => item.Id == metadata.HostedId));
        Assert.AreEqual(delivery.Id, deliveries.Single(item => item.Operation == "forward").EventId);
    }

    [TestMethod]
    public async Task AnonymousBehaviorHelpersHaveRelationshipsIncludingNestedGuards()
    {
        var metadata = new Metadata();
        await ServiceProvider.GetRequiredService<IStateMachinesRegister>()
            .VisitStateMachineAsync("behavior-metadata", 1, metadata);
        Assert.HasCount(2, metadata.Actions);
        Assert.HasCount(1, metadata.Guards);
        Assert.IsTrue(metadata.Guards.Single().Contains("/guard:0/0/1", StringComparison.Ordinal));
        Assert.AreEqual(0, metadata.Typed);
    }

    private sealed class Metadata : StateMachineVisitor
    {
        public string? HostedId;
        public List<string> Actions { get; } = [];
        public List<string> Guards { get; } = [];
        public int Typed;
        public override Task HostedBehaviorAddedAsync(string id, string name, int version, string vertexId, BehaviorClass behavior)
        { HostedId = id; return Task.CompletedTask; }
        public override Task ActionBehaviorAddedAsync(string id, string name, int version, BehaviorClass behavior)
        { Actions.Add(id); return Task.CompletedTask; }
        public override Task GuardBehaviorAddedAsync(string id, string name, int version, BehaviorClass behavior)
        { Guards.Add(id); return Task.CompletedTask; }
        public override Task ActionTypeAddedAsync<TImplementation>(string id, string name, int version)
        { Typed++; return Task.CompletedTask; }
        public override Task GuardTypeAddedAsync<TImplementation>(string id, string name, int version)
        { Typed++; return Task.CompletedTask; }
    }

    private sealed class Recorder(HostedObservation owner) : StateMachineObserver
    {
        private async Task Observe(string operation, IHostedBehaviorObservationContext context, Func<Task> next)
        {
            await next();
            owner.deliveries.Enqueue((operation, context.Identifier, context.EventId));
        }
        public override Task OnHostedBehaviorInitializeAsync(IHostedBehaviorObservationContext context, Func<Task> next)
            => Observe("initialize", context, next);
        public override Task OnHostedBehaviorEventForwardAsync(IHostedBehaviorObservationContext context, Func<Task> next)
            => Observe("forward", context, next);
        public override Task OnHostedBehaviorFinalizeAsync(IHostedBehaviorObservationContext context, Func<Task> next)
            => Observe("finalize", context, next);
    }
}
