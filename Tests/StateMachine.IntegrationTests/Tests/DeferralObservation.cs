using Stateflows.Common;
using Stateflows.StateMachines;
using StateMachine.IntegrationTests.Utils;

namespace StateMachine.IntegrationTests.Tests;

[TestClass]
public class DeferralObservation : StateflowsTestClass
{
    private sealed record Work(int Value);
    private sealed record Move;
    private sealed record Release;
    private readonly List<int> guardValues = [];
    private readonly List<(Guid Id, string Name, bool Reevaluation, int Value)> evaluations = [];
    private readonly List<Guid> enqueued = [];
    private int consumed;

    [TestInitialize]
    public override void Initialize() => base.Initialize();

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();

    protected override void InitializeStateflows(IStateflowsBuilder builder)
        => builder.AddStateMachines(machines => machines
            .AddStateMachine("deferral-observation", machine => machine
                .AddObserver((_, _) => new Recorder(this))
                .AddInitialState("First", state => state
                    .AddDeferredEvent<Work>(rule => rule.AddGuard(context =>
                    {
                        guardValues.Add(context.Event.Value);
                        return true;
                    }))
                    .AddTransition<Move>("Second"))
                .AddState("Second", state => state
                    .AddDeferredEvent<Work>(rule => rule.AddGuard(context =>
                    {
                        guardValues.Add(context.Event.Value);
                        return true;
                    }))
                    .AddTransition<Release>("Ready"))
                .AddState("Ready", state => state
                    .AddInternalTransition<Work>(transition => transition
                        .AddEffect(_ => { consumed++; return Task.CompletedTask; })))));

    [TestMethod]
    public async Task RecheckingUsesCandidateAndDoesNotEnqueueItAgain()
    {
        Assert.IsTrue(StateMachineLocator.TryLocateStateMachine(
            new StateMachineId("deferral-observation", "test"), out var machine));
        Assert.AreEqual(EventStatus.Deferred, (await machine.SendAsync(new Work(42))).Status);
        Assert.AreEqual(EventStatus.Consumed, (await machine.SendAsync(new Move())).Status);
        Assert.AreEqual(EventStatus.Consumed, (await machine.SendAsync(new Release())).Status);

        Assert.AreEqual(1, consumed);
        Assert.HasCount(1, enqueued);
        Assert.IsTrue(evaluations.Any(evaluation => !evaluation.Reevaluation));
        Assert.IsTrue(evaluations.Any(evaluation => evaluation.Reevaluation));
        Assert.IsTrue(evaluations.All(evaluation => evaluation.Id == enqueued[0] &&
            evaluation.Name == Event<Work>.Name && evaluation.Value == 42));
        Assert.IsTrue(guardValues.Count >= 2 && guardValues.All(value => value == 42));
    }

    private sealed class Recorder(DeferralObservation owner) : StateMachineObserver
    {
        public override Task OnDeferralGuardAsync(IDeferralObservationContext context, Func<Task<bool>> next)
        {
            owner.evaluations.Add((context.EventId, context.EventName, context.IsReevaluation, ((Work)context.Event).Value));
            return next();
        }

        public override Task OnEventDeferredAsync(IDeferralObservationContext context, Func<Task> next)
        {
            owner.enqueued.Add(context.EventId);
            return next();
        }
    }
}
