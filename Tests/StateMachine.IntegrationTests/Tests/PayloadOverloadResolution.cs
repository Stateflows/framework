using Stateflows.Common;
using Stateflows.StateMachines;
using Stateflows.StateMachines.Registration.Interfaces;
using StateMachine.IntegrationTests.Tests.DefinitionRules;

namespace StateMachine.IntegrationTests.Tests;

/// <summary>Payload and CLR-typed builder methods resolve independently.</summary>
[TestClass]
public class PayloadOverloadResolution
{
    public sealed class Ping;

    public sealed class PayloadInitializer : IInitializer<Payload>
    {
        public Task<bool> OnInitializeAsync(Payload initializationEvent) => Task.FromResult(true);
    }

    public sealed class PingInitializer : IInitializer<Ping>
    {
        public Task<bool> OnInitializeAsync(Ping initializationEvent) => Task.FromResult(true);
    }

    public sealed class PayloadGuard : IDeferralGuard<Payload>
    {
        public Task<bool> GuardAsync(Payload @event) => Task.FromResult(true);
    }

    public sealed class PingGuard : IDeferralGuard<Ping>
    {
        public Task<bool> GuardAsync(Ping @event) => Task.FromResult(true);
    }

    [TestMethod]
    public void TypedInitializerOverloadsResolveUnchanged()
    {
        var lambda = new DefinitionModel();
        ((IStateMachineBuilder)lambda.Builder).AddInitializer<Ping>(async _ => true);
        var @class = new DefinitionModel();
        ((IStateMachineBuilder)@class.Builder).AddInitializer<Ping, PingInitializer>();
        foreach (var model in new[] { lambda, @class })
        {
            Assert.IsTrue(model.Graph.InitializerDeclarations.ContainsKey(typeof(Ping)));
            Assert.AreEqual(0, model.Graph.PayloadInitializerDeclarations.Count);
        }
    }

    [TestMethod]
    public void NamedInitializerClassOverloadsResolveToNamedMembers()
    {
        var list = new DefinitionModel();
        ((IStateMachineBuilder)list.Builder).AddPayloadInitializer<PayloadInitializer>(["a", "b"]);
        Assert.AreEqual(1, list.Graph.PayloadInitializerDeclarations.Count);
        Assert.AreEqual(0, list.Graph.InitializerDeclarations.Count);

        var single = new DefinitionModel();
        ((IStateMachineBuilder)single.Builder).AddPayloadInitializer<PayloadInitializer>(["a"]);
        Assert.AreEqual(1, single.Graph.PayloadInitializerDeclarations.Count);
        Assert.AreEqual(0, single.Graph.InitializerDeclarations.Count);
    }

    [TestMethod]
    public void DeferralOverloadsResolveToExpectedMembers()
    {
        var model = new DefinitionModel(initialize: false);
        IStateMachineBuilder builder = model.Builder;
        builder
            .AddInitialState("typed", b => b.AddDeferredEvent<Ping>())
            .AddState("typedClass", b => b.AddDeferredEvent<Ping, PingGuard>())
            .AddState("named", b => b
                .AddDeferredPayloadEvent<PayloadGuard>(["a", "b"])
                .AddDeferredPayloadEvent<PayloadGuard>(["c"])
                .AddDeferredPayloadEvent(["d"])
                .AddDeferredPayloadEvent(["e"], b => b.AddGuard(_ => true)));
        CollectionAssert.AreEquivalent(new[] { Event<Ping>.Name }, model.Graph.Vertices["typed"].Deferrals.Keys.ToArray());
        CollectionAssert.AreEquivalent(new[] { Event<Ping>.Name }, model.Graph.Vertices["typedClass"].Deferrals.Keys.ToArray());
        CollectionAssert.AreEquivalent(new[] { "a", "b", "c", "d", "e" }, model.Graph.Vertices["named"].Deferrals.Keys.ToArray());
    }

    public class TypedTargetsMachine : IStateMachine
    {
        public static void Build(IStateMachineBuilder builder) => builder
            .AddInitialState("waiting", b => b
                .AddPayloadTransition<PayloadTransitions.Done>(["a"])
                .AddElsePayloadTransition<PayloadTransitions.Fallback>(["a"]))
            .AddState<PayloadTransitions.Done>()
            .AddState<PayloadTransitions.Fallback>();
    }

    [TestMethod]
    public void PayloadTransitionTargetAndImplementationOverloadsKeepExplicitNames()
    {
        var model = new DefinitionModel(initialize: false);
        ((IStateMachineBuilder)model.Builder)
            .AddInitialState("waiting", b => b
                .AddPayloadTransition<PayloadTransitions.Done>(["inline"])
                .AddPayloadTransition<PayloadTransitions.RegisteredTransition>(["named"], "done")
                .AddPayloadTransition<PayloadTransitions.RegisteredTransition, PayloadTransitions.Done>(["typed"])
                .AddElsePayloadTransition<PayloadTransitions.Fallback>(["inline"])
                .AddElsePayloadTransition<PayloadTransitions.RecordEffect>(["named"], "fallback")
                .AddElsePayloadTransition<PayloadTransitions.RecordEffect, PayloadTransitions.Fallback>(["typed"]));

        var edges = model.Graph.AllEdges;
        Assert.AreEqual(6, edges.Count);
        Assert.IsTrue(edges.All(edge => edge.TriggerType == typeof(Payload) && edge.PayloadNames!.Count == 1));
        foreach (var edge in edges.Where(edge => edge.ImplementationType == typeof(PayloadTransitions.RegisteredTransition)))
        {
            Assert.AreEqual(1, edge.Guards.Actions.Count);
            Assert.AreEqual(2, edge.Effects.Actions.Count);
        }
        Assert.AreEqual(State<PayloadTransitions.Done>.Name, edges.Single(edge => edge.PayloadNames![0] == "typed" && !edge.IsElse).TargetName);
        Assert.AreEqual(State<PayloadTransitions.Fallback>.Name, edges.Single(edge => edge.PayloadNames![0] == "typed" && edge.IsElse).TargetName);
    }

    [TestMethod]
    public void PayloadTransitionSelectorsResolveTypedTargetsWithoutReplacingImplementations()
    {
        var model = new DefinitionModel(initialize: false);
        ((IStateMachineBuilder)model.Builder)
            .UseStateMachine<TypedTargetsMachine>(b => b.UseState("waiting", b => b
                .UsePayloadTransition<PayloadTransitions.Done>(["a"], b => b.AddEffect(_ => Task.CompletedTask))
                .UseElsePayloadTransition<PayloadTransitions.Fallback>(["a"], b => b.AddEffect(_ => Task.CompletedTask))));

        Assert.AreEqual(2, model.Graph.AllEdges.Count);
        Assert.IsTrue(model.Graph.AllEdges.All(edge => edge.ImplementationType == null && edge.Effects.Actions.Count == 1));
    }
}