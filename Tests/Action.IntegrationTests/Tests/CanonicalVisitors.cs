using Microsoft.Extensions.DependencyInjection;
using Stateflows.Actions;
using Stateflows.Common;
using Stateflows.Events;

namespace Action.IntegrationTests.Tests;

[TestClass]
public sealed class CanonicalVisitors
{
    private sealed class FailingVisitor(Exception failure) : ActionVisitor
    {
        public bool Ended;
        public override Task CustomEventAddedAsync<TEvent>(string id, string name, int version,
            IReadOnlyList<string> eventNames, BehaviorStatus[] supportedStatuses)
            => throw failure;
        public override Task ActionAddedAsync(string id, string name, int version)
        {
            Ended = true;
            return Task.CompletedTask;
        }
    }

    private sealed class Ping;
    private sealed class Implementation : IAction
    {
        public Task ExecuteAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class Recorder : ActionVisitor
    {
        public readonly List<string> Trace = [];
        public readonly List<(string Id, Type Type, string[] Names)> Bindings = [];
        public readonly List<int> Versions = [];
        public readonly List<(string Id, Type Type, string[] Names)> Produced = [];
        public readonly List<(string Id, Type Type, bool Input)> Tokens = [];
        public bool IsStateless;
        private string? definitionId;

        public override Task ActionAddingAsync(string id, string name, int version, string? behaviorClassType = null,
            BehaviorClass? ownerClass = null, BehaviorClass? parentClass = null, bool hasDefaultInstance = false)
        {
            Assert.IsFalse(string.IsNullOrEmpty(id));
            definitionId = id;
            Versions.Add(version);
            Trace.Add("begin");
            return Task.CompletedTask;
        }

        public override Task ActionTypeAddedAsync<TAction>(string id, string name, int version)
        {
            Assert.AreEqual(definitionId, id);
            Assert.AreEqual("begin", Trace[^1]);
            Trace.Add("type");
            return Task.CompletedTask;
        }

        public override Task CustomEventAddedAsync<TEvent>(string id, string name, int version,
            IReadOnlyList<string> eventNames, BehaviorStatus[] supportedStatuses)
        {
            Assert.IsFalse(string.IsNullOrEmpty(id));
            Assert.IsTrue(eventNames.Count > 0);
            Assert.AreEqual(eventNames.Count, eventNames.Distinct().Count());
            CollectionAssert.AreEqual(new[] { BehaviorStatus.Initialized }, supportedStatuses);
            Bindings.Add((id, typeof(TEvent), eventNames.ToArray()));
            Trace.Add("event");
            return Task.CompletedTask;
        }

        public override Task ActionAddedAsync(string id, string name, int version)
        {
            Assert.AreEqual(definitionId, id);
            Trace.Add("end");
            return Task.CompletedTask;
        }

        public override Task ProducedEventAddedAsync<TEvent>(string id, string name, int version, IReadOnlyList<string> eventNames)
        {
            Produced.Add((id, typeof(TEvent), eventNames.ToArray()));
            return Task.CompletedTask;
        }

        public override Task InputTokenAddedAsync<TToken>(string id, string name, int version)
        {
            Tokens.Add((id, typeof(TToken), true));
            return Task.CompletedTask;
        }

        public override Task OutputTokenAddedAsync<TToken>(string id, string name, int version)
        {
            Tokens.Add((id, typeof(TToken), false));
            return Task.CompletedTask;
        }

        public override Task ActionConfigurationAddedAsync(string id, string name, int version, bool isStateless)
        {
            Assert.AreEqual(definitionId, id);
            IsStateless = isStateless;
            return Task.CompletedTask;
        }
    }

    [TestMethod]
    public async Task SynchronousGenericCallbackFailureIsNotWrappedAndStopsVisitation()
    {
        var services = new ServiceCollection();
        services.AddStateflows(builder => builder.AddActions(actions => actions.AddAction("failing-visitor", 1,
            _ => Task.CompletedTask, action => action.AddConsumedEvent<Ping>())));
        using var provider = services.BuildServiceProvider();
        var failure = new ApplicationException("visitor failed");
        var visitor = new FailingVisitor(failure);
        var thrown = await Assert.ThrowsAsync<ApplicationException>(() =>
            provider.GetRequiredService<IActionsRegister>().VisitActionsAsync(visitor));
        Assert.AreSame(failure, thrown);
        Assert.IsFalse(visitor.Ended);
    }

    [TestMethod]
    public async Task TypedAndAnonymousDefinitionsVisitStableUnifiedBindingsAndEveryVersion()
    {
        var services = new ServiceCollection();
        services.AddStateflows(builder => builder
            .AddEvents(events => events.AddEvent("first", _ => { }).AddEvent("second", _ => { }))
            .AddActions(actions => actions
                .AddAction<Implementation>("canonical-action", 1, action => action
                    .AddConsumedEvent("first").AddConsumedEvent<Ping>().AddConsumedEvent("second").AddConsumedEvent("first"))
                .AddAction("canonical-action", 2, _ => Task.CompletedTask)));
        using var provider = services.BuildServiceProvider();
        var register = provider.GetRequiredService<IActionsRegister>();
        var first = new Recorder();
        var second = new Recorder();
        await register.VisitActionsAsync(first);
        await register.VisitActionsAsync(second);

        CollectionAssert.AreEqual(new[] { 1, 2 }, first.Versions);
        CollectionAssert.AreEqual(new[] { "begin", "type", "event", "event", "end", "begin", "end" }, first.Trace);
        CollectionAssert.AreEqual(first.Trace, second.Trace);
        CollectionAssert.AreEqual(first.Bindings.Select(binding => binding.Id).ToArray(), second.Bindings.Select(binding => binding.Id).ToArray());
        Assert.AreEqual(typeof(StructuralEvent), first.Bindings[0].Type);
        CollectionAssert.AreEqual(new[] { "first", "second" }, first.Bindings[0].Names);
        Assert.AreEqual(typeof(Ping), first.Bindings[1].Type);
        CollectionAssert.AreEqual(new[] { typeof(Ping).GetEventName() }, first.Bindings[1].Names);
        Assert.AreEqual(2, first.Bindings.Select(binding => binding.Id).Distinct().Count());
    }

    [TestMethod]
    public async Task DeclaredProductionTokenBoundariesAndConfigurationAreVisitedBeforeEnd()
    {
        var services = new ServiceCollection();
        services.AddStateflows(builder => builder.AddActions(actions => actions.AddAction("surfaces", 1,
            _ => Task.CompletedTask, action => action.AddConsumedToken<int>().AddProducedToken<string>()
                .AddProducedEvent<Ping>().SetIsStateless(true))));
        using var provider = services.BuildServiceProvider();
        var visitor = new Recorder();
        await provider.GetRequiredService<IActionsRegister>().VisitActionsAsync(visitor);
        Assert.IsTrue(visitor.IsStateless);
        CollectionAssert.AreEqual(new[] { typeof(int), typeof(string) }, visitor.Tokens.Select(token => token.Type).ToArray());
        CollectionAssert.AreEqual(new[] { true, false }, visitor.Tokens.Select(token => token.Input).ToArray());
        Assert.AreEqual(typeof(Ping), visitor.Produced.Single().Type);
        CollectionAssert.AreEqual(new[] { typeof(Ping).GetEventName() }, visitor.Produced.Single().Names);
        Assert.AreEqual("end", visitor.Trace[^1]);
        Assert.AreEqual(4, visitor.Bindings.Select(binding => binding.Id).Concat(visitor.Tokens.Select(token => token.Id))
            .Concat(visitor.Produced.Select(binding => binding.Id)).Distinct().Count());
    }
}
