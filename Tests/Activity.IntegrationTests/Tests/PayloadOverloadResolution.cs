using Microsoft.Extensions.DependencyInjection;
using Stateflows.Activities.Registration.Builders;
using Stateflows.Common;
using Stateflows.Common.Registration.Builders;

namespace Activity.IntegrationTests.Tests;

/// <summary>T0.18 / T2.17: the named initializer class overloads sit beside the typed ones without changing their resolution.</summary>
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

    private static ActivityBuilder NewBuilder()
        => new("definition", 1, null!, new StateflowsBuilder(new ServiceCollection()), null, null);

    [TestMethod]
    public void TypedInitializerOverloadsResolveUnchanged()
    {
        var lambda = NewBuilder();
        ((IActivityBuilder)lambda).AddInitializer<Ping>(async _ => true);
        var @class = NewBuilder();
        ((IActivityBuilder)@class).AddInitializer<Ping, PingInitializer>();
        foreach (var builder in new[] { lambda, @class })
        {
            CollectionAssert.AreEqual(new[] { Event<Ping>.Name }, builder.Graph.Initializers.Keys.ToArray());
            Assert.AreEqual(0, builder.Graph.PayloadInitializerNames.Count);
        }
    }

    [TestMethod]
    public void NamedInitializerClassOverloadsResolveToNamedMembers()
    {
        var list = NewBuilder();
        ((IActivityBuilder)list).AddInitializer<PayloadInitializer>(["b", "a"]);
        CollectionAssert.AreEqual(new[] { "a|b" }, list.Graph.Initializers.Keys.ToArray());

        var sugar = NewBuilder();
        ((IActivityBuilder)sugar).AddInitializer<PayloadInitializer>("a");
        CollectionAssert.AreEqual(new[] { "a" }, sugar.Graph.Initializers.Keys.ToArray());
        Assert.AreEqual(1, sugar.Graph.PayloadInitializerNames.Count);
    }
}