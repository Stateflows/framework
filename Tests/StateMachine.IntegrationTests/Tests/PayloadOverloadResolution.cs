using Stateflows.Common;
using Stateflows.StateMachines;
using Stateflows.StateMachines.Registration.Interfaces;
using StateMachine.IntegrationTests.Tests.DefinitionRules;

namespace StateMachine.IntegrationTests.Tests;

/// <summary>T0.18 / T2.17: the named class overloads sit beside the typed ones without changing their resolution.</summary>
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
        ((IStateMachineBuilder)list.Builder).AddInitializer<PayloadInitializer>(["a", "b"]);
        Assert.AreEqual(1, list.Graph.PayloadInitializerDeclarations.Count);
        Assert.AreEqual(0, list.Graph.InitializerDeclarations.Count);

        var sugar = new DefinitionModel();
        ((IStateMachineBuilder)sugar.Builder).AddInitializer<PayloadInitializer>("a");
        Assert.AreEqual(1, sugar.Graph.PayloadInitializerDeclarations.Count);
        Assert.AreEqual(0, sugar.Graph.InitializerDeclarations.Count);
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
                .AddDeferredEvent<PayloadGuard>(["a", "b"])
                .AddDeferredEvent<PayloadGuard>("c")
                .AddDeferredEvent("d")
                .AddDeferredEvent(["e"], b => b.AddGuard(_ => true)));
        CollectionAssert.AreEquivalent(new[] { Event<Ping>.Name }, model.Graph.Vertices["typed"].Deferrals.Keys.ToArray());
        CollectionAssert.AreEquivalent(new[] { Event<Ping>.Name }, model.Graph.Vertices["typedClass"].Deferrals.Keys.ToArray());
        CollectionAssert.AreEquivalent(new[] { "a", "b", "c", "d", "e" }, model.Graph.Vertices["named"].Deferrals.Keys.ToArray());
    }
}