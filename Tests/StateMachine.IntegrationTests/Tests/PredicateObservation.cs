using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.StateMachines;
using Stateflows.StateMachines.Context.Interfaces;
using StateMachine.IntegrationTests.Utils;

namespace StateMachine.IntegrationTests.Tests;

[TestClass]
public sealed class PredicateObservation : StateflowsTestClass
{
    private sealed record Start;
    private sealed record Pending;
    private sealed record Advance;
    private sealed record Evaluation(string Id, string Owner, GuardKind Kind, object Event, Guid EventId,
        string? Scope, bool? IsReevaluation, bool Result);

    private readonly List<Evaluation> evaluations = [];
    private readonly List<string> invoked = [];
    private int initializationAttempts;
    private int deferralAttempts;
    private int entries;

    [TestInitialize]
    public override void Initialize() => base.Initialize();

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();

    protected override void InitializeStateflows(IStateflowsBuilder builder)
        => builder.AddStateMachines(machines => machines
            .AddStateMachine("predicate-event-initializers", machine => machine
                .AddInitializer<Start>(_ => { invoked.Add("event-0"); return Task.FromResult(true); })
                .AddInitializer<Start>(_ => { invoked.Add("event-1"); return Task.FromResult(false); })
                .AddInitializer<Start>(_ => { invoked.Add("event-2"); return Task.FromResult(true); })
                .AddObserver((_, _) => new Recorder(this))
                .AddInitialState("source", state => state.AddOnEntry(_ => { entries++; return Task.CompletedTask; })))
            .AddStateMachine("predicate-default-initializers", machine => machine
                .AddDefaultInitializer(_ => { invoked.Add("default-0"); return Task.FromResult(true); })
                .AddDefaultInitializer(_ => { invoked.Add("default-1"); return Task.FromResult(false); })
                .AddDefaultInitializer(_ => { invoked.Add("default-2"); return Task.FromResult(true); })
                .AddObserver((_, _) => new Recorder(this))
                .AddInitialState("source", state => state.AddOnEntry(_ => { entries++; return Task.CompletedTask; })))
            .AddStateMachine("predicate-all-scopes", machine => machine
                .AddInitializer<Start>(_ => Task.FromResult(true))
                .AddObserver((_, _) => new Recorder(this))
                .AddInitialState("source", state => state
                    .AddDeferredEvent<Pending>(deferral => deferral.AddGuard(_ => true))
                    .AddTransition<Advance>("source", transition => transition.AddGuard(_ => true)))));

    private IStateMachineBehavior Locate(string name)
    {
        Assert.IsTrue(StateMachineLocator.TryLocateStateMachine(new StateMachineId(name, "test"), out var machine));
        return machine;
    }

    [TestMethod]
    public void PredicateContractKeepsWholeOperationCallbacksSeparate()
    {
        Assert.IsNull(typeof(IStateMachineObserver).GetMethod("OnGuardAsync"));
        var predicate = typeof(IStateMachineObserver).GetMethod(nameof(IStateMachineObserver.OnPredicateAsync));
        Assert.IsNotNull(predicate);
        Assert.AreEqual(typeof(IStateMachinePredicateObservationContext), predicate.GetParameters()[0].ParameterType);
        Assert.IsNotNull(typeof(IStateMachineObserver).GetMethod(nameof(IStateMachineObserver.OnStateMachineInitializeAsync)));
        Assert.IsNotNull(typeof(IStateMachineObserver).GetMethod(nameof(IStateMachineObserver.OnTransitionGuardAsync)));
        Assert.IsNotNull(typeof(IStateMachineObserver).GetMethod(nameof(IStateMachineObserver.OnDeferralGuardAsync)));
    }

    [TestMethod]
    [DataRow("predicate-event-initializers", "event")]
    [DataRow("predicate-default-initializers", "default")]
    public async Task InitializersUsePredicateHookInsideWholeInitializationAndStopOnFalse(string name, string prefix)
    {
        var metadata = new Metadata();
        await ServiceProvider.GetRequiredService<IStateMachinesRegister>().VisitStateMachineAsync(name, 1, metadata);
        var machine = Locate(name);
        if (prefix == "event")
            await machine.SendAsync(new Start());
        else
            await machine.SendAsync(new Initialize());

        CollectionAssert.AreEqual(new[] { $"{prefix}-0", $"{prefix}-1" }, invoked);
        Assert.AreEqual(1, initializationAttempts);
        Assert.AreEqual(0, entries);
        Assert.AreEqual(2, evaluations.Count);
        CollectionAssert.AreEqual(new[] { true, false }, evaluations.Select(evaluation => evaluation.Result).ToArray());
        Assert.IsTrue(evaluations.All(evaluation => evaluation.Scope == "initialize" && evaluation.Kind == GuardKind.Predicate));
        Assert.IsTrue(evaluations.All(evaluation => metadata.Predicates.TryGetValue(evaluation.Id, out var owner)
            && owner == evaluation.Owner));
        Assert.AreEqual(1, evaluations.Select(evaluation => evaluation.EventId).Distinct().Count());
    }

    [TestMethod]
    public async Task SamePredicateHookCoversInitializationTransitionsAndDeferrals()
    {
        var metadata = new Metadata();
        await ServiceProvider.GetRequiredService<IStateMachinesRegister>()
            .VisitStateMachineAsync("predicate-all-scopes", 1, metadata);
        var machine = Locate("predicate-all-scopes");
        Assert.AreEqual(EventStatus.Initialized, (await machine.SendAsync(new Start())).Status);
        Assert.AreEqual(EventStatus.Deferred, (await machine.SendAsync(new Pending())).Status);
        var deferralEvaluationsBeforeTransition = evaluations.Count(evaluation => evaluation.Scope == "deferral");
        Assert.AreEqual(EventStatus.Consumed, (await machine.SendAsync(new Advance())).Status);

        Assert.AreEqual(1, initializationAttempts);
        Assert.AreEqual(1, evaluations.Count(evaluation => evaluation.Scope == "initialize" && evaluation.Event is Start));
        Assert.AreEqual(1, evaluations.Count(evaluation => evaluation.Scope == "transition" && evaluation.Event is Advance));
        var deferrals = evaluations.Where(evaluation => evaluation.Scope == "deferral").ToArray();
        Assert.IsTrue(deferrals.Length > deferralEvaluationsBeforeTransition,
            "The self-transition must reevaluate the queued candidate.");
        Assert.AreEqual(deferralAttempts, deferrals.Length,
            "Each whole deferral evaluation must invoke its single predicate exactly once.");
        Assert.AreEqual(1, deferrals.Count(evaluation => evaluation.IsReevaluation == false));
        Assert.IsTrue(deferrals.Skip(1).All(evaluation => evaluation.IsReevaluation == true));
        Assert.IsTrue(deferrals.All(evaluation => evaluation.Event is Pending));
        Assert.AreEqual(1, deferrals.Select(evaluation => evaluation.Id).Distinct().Count());
        Assert.AreEqual(1, deferrals.Select(evaluation => evaluation.EventId).Distinct().Count());
        Assert.IsTrue(evaluations.All(evaluation => evaluation.Result
            && metadata.Predicates.TryGetValue(evaluation.Id, out var owner) && owner == evaluation.Owner));
    }

    private sealed class Metadata : StateMachineVisitor
    {
        public Dictionary<string, string> Predicates { get; } = [];

        public override Task GuardAddedAsync(string predicateId, string stateMachineName, int stateMachineVersion,
            string ownerElementId, string? parentGuardId, int guardIndex, GuardKind guardKind)
        {
            Predicates.Add(predicateId, ownerElementId);
            return Task.CompletedTask;
        }
    }

    private sealed class Recorder(PredicateObservation owner) : StateMachineObserver
    {
        private readonly AsyncLocal<string?> scope = new();
        private readonly AsyncLocal<bool?> reevaluation = new();

        private async Task ObserveAsync(string operation, Func<Task<bool>> next)
        {
            var previous = scope.Value;
            scope.Value = operation;
            try { await next(); }
            finally { scope.Value = previous; }
        }

        public override Task OnStateMachineInitializeAsync(IStateMachineInitializationContext initializationContext,
            bool implicitInitialization, Func<Task<bool>> next)
        {
            owner.initializationAttempts++;
            return ObserveAsync("initialize", next);
        }

        public override Task OnTransitionGuardAsync<TEvent>(ITransitionContext<TEvent> transitionContext, Func<Task<bool>> next)
            => ObserveAsync("transition", next);

        public override async Task OnDeferralGuardAsync(IDeferralObservationContext deferralContext, Func<Task<bool>> next)
        {
            owner.deferralAttempts++;
            var previous = reevaluation.Value;
            reevaluation.Value = deferralContext.IsReevaluation;
            try { await ObserveAsync("deferral", next); }
            finally { reevaluation.Value = previous; }
        }

        public override async Task OnPredicateAsync(IStateMachinePredicateObservationContext predicateContext, Func<Task<bool>> next)
        {
            var result = await next();
            owner.evaluations.Add(new(predicateContext.Identifier, predicateContext.OwnerId, predicateContext.Kind,
                predicateContext.Event, predicateContext.EventId, scope.Value, reevaluation.Value, result));
        }
    }
}
