using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.StateMachines;
using Stateflows.StateMachines.Context.Interfaces;
using StateMachine.IntegrationTests.Utils;

namespace StateMachine.IntegrationTests.Tests;

[TestClass]
public class CanonicalObservation : StateflowsTestClass
{
    private sealed record Advance(bool Allowed);
    private readonly List<string> trace = [];
    private readonly List<(string Id, string Owner, GuardKind Kind, bool Result)> guards = [];
    private readonly List<string> actions = [];
    private readonly List<(string Id, Type Type, string[] Route, int Index)> segments = [];
    private int fallbackCalls;

    [TestInitialize] public override void Initialize() => base.Initialize();
    [TestCleanup] public override void Cleanup() => base.Cleanup();

    protected override void InitializeStateflows(IStateflowsBuilder builder) => builder.AddStateMachines(machines => machines
        .AddStateMachine("canonical-observation", machine => machine
            .AddObserver((_, _) => new Recorder(this))
            .AddInitialState("source", state => state
                .AddOnExit(_ => { trace.Add("exit"); return Task.CompletedTask; })
                .AddTransition<Advance>("first", transition => transition
                    .AddGuardExpression(expression => expression
                        .AddNegatedGuard(_ => false)
                        .AddOrExpression(branch => branch
                            .AddGuard(context => context.Event.Allowed)
                            .AddGuard(_ => { fallbackCalls++; return false; }))
                        .AddGuard(context => context.Event.Allowed))
                    .AddEffect(_ => { trace.Add("source-effect"); return Task.CompletedTask; })))
            .AddJunction("first", junction => junction.AddTransition("second", transition => transition
                .AddGuard(_ => true)
                .AddEffect(_ => { trace.Add("first-effect"); return Task.CompletedTask; })))
            .AddJunction("second", junction => junction.AddTransition("target", transition => transition
                .AddGuard(_ => true)
                .AddEffect(_ => { trace.Add("second-effect"); return Task.CompletedTask; })))
            .AddState("target", state => state.AddOnEntry(_ => { trace.Add("entry"); return Task.CompletedTask; }))
            .AddState("source/Exit:0")
            .AddState("machine:canonical-observation")
            .AddState("source/hosted")));

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public async Task RuntimeRegistrationsMatchVisitedIdsAndRespectShortCircuiting(bool allowed)
    {
        var visitor = new Metadata();
        await ServiceProvider.GetRequiredService<IStateMachinesRegister>()
            .VisitStateMachineAsync("canonical-observation", 1, visitor);
        var repeated = new Metadata();
        await ServiceProvider.GetRequiredService<IStateMachinesRegister>()
            .VisitStateMachineAsync("canonical-observation", 1, repeated);
        CollectionAssert.AreEqual(visitor.Ids.ToArray(), repeated.Ids.ToArray());
        Assert.AreEqual(visitor.Ids.Count, visitor.Ids.Distinct().Count());

        Assert.IsTrue(StateMachineLocator.TryLocateStateMachine(new StateMachineId("canonical-observation", "test"), out var machine));
        await machine.SendAsync(new Initialize());
        guards.Clear(); actions.Clear(); segments.Clear(); trace.Clear();
        var result = await machine.SendAsync(new Advance(allowed));
        Assert.AreEqual(allowed ? EventStatus.Consumed : EventStatus.NotConsumed, result.Status);
        Assert.IsTrue(guards.All(guard => visitor.Ids.Contains(guard.Id)));
        Assert.IsTrue(actions.All(visitor.Ids.Contains));
        Assert.AreEqual(allowed ? 0 : 1, fallbackCalls);
        Assert.IsTrue(guards.Any(guard => guard.Kind == GuardKind.Not && guard.Result));
        Assert.IsTrue(guards.Any(guard => guard.Kind == GuardKind.Predicate && !guard.Result));
        Assert.IsTrue(segments.All(segment => segment.Id == segment.Route[segment.Index]));
        Assert.IsTrue(segments.All(segment => segment.Route.Length == 3));
        if (allowed)
        {
            CollectionAssert.AreEqual(new[] { "exit", "source-effect", "first-effect", "second-effect", "entry" },
                trace.Where(item => !item.StartsWith("guard:")).ToArray());
            Assert.IsTrue(trace.TakeWhile(item => item != "exit").All(item => item.StartsWith("guard:")));
            Assert.AreEqual(3, segments.Select(segment => segment.Id).Distinct().Count());
            Assert.IsTrue(segments.Where(segment => segment.Index > 0).All(segment => segment.Type == typeof(Completion)));
            Assert.AreEqual("target", (await machine.GetStatusAsync()).Response.CurrentStates.Value);
        }
        else
        {
            Assert.IsEmpty(actions);
            Assert.IsTrue(segments.All(segment => segment.Index == 0));
            Assert.AreEqual("source", (await machine.GetStatusAsync()).Response.CurrentStates.Value);
        }
    }

    private sealed class Metadata : StateMachineVisitor
    {
        public List<string> Ids { get; } = [];
        public override Task StateMachineAddingAsync(string id, string name, int version, BehaviorClass? owner,
            BehaviorClass? parent, bool hasDefaultInstance)
        { Ids.Add(id); return Task.CompletedTask; }
        public override Task VertexAddedAsync(string id, string name, int version, string vertexName, VertexType kind,
            string? parentVertexId, string? regionId, int? regionIndex)
        { Ids.Add(id); return Task.CompletedTask; }
        public override Task TransitionAddedAsync<TEvent>(string id, string name, int version, string sourceId,
            string? targetId, bool isElse, bool isLocal, bool polymorphic, IReadOnlyList<string> triggerNames)
        { Ids.Add(id); return Task.CompletedTask; }
        public override Task ActionAddedAsync(string id, string name, int version, string owner, StateMachineOperation operation, int index)
        { Ids.Add(id); return Task.CompletedTask; }
        public override Task GuardAddedAsync(string id, string name, int version, string owner, string? parent, int index, GuardKind kind)
        { Ids.Add(id); return Task.CompletedTask; }
    }

    private sealed class Recorder(CanonicalObservation owner) : StateMachineObserver
    {
        public override async Task OnActionAsync(IStateMachineActionObservationContext context, Func<Task> next)
        { owner.actions.Add(context.Identifier); await next(); }
        public override async Task OnPredicateAsync(IStateMachinePredicateObservationContext context, Func<Task<bool>> next)
        {
            owner.trace.Add("guard:" + context.Identifier);
            var result = await next();
            owner.guards.Add((context.Identifier, context.OwnerId, context.Kind, result));
        }
        public override async Task OnTransitionGuardAsync<TEvent>(ITransitionContext<TEvent> context, Func<Task<bool>> next)
        {
            owner.segments.Add((context.Identifier, typeof(TEvent), context.RouteTransitionIds.ToArray(), context.RouteIndex));
            await next();
        }
    }
}
