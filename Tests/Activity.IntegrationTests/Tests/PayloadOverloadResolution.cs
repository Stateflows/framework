using Microsoft.Extensions.DependencyInjection;
using Stateflows.Activities.Registration.Builders;
using Stateflows.Common;
using Stateflows.Common.Registration.Builders;

namespace Activity.IntegrationTests.Tests;

/// <summary>Payload and CLR-typed initializer methods resolve independently.</summary>
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
        ((IActivityBuilder)list).AddPayloadInitializer<PayloadInitializer>(["b", "a"]);
        CollectionAssert.AreEqual(new[] { "a|b" }, list.Graph.Initializers.Keys.ToArray());

        var single = NewBuilder();
        ((IActivityBuilder)single).AddPayloadInitializer<PayloadInitializer>(["a"]);
        CollectionAssert.AreEqual(new[] { "a" }, single.Graph.Initializers.Keys.ToArray());
        Assert.AreEqual(1, single.Graph.PayloadInitializerNames.Count);
    }
}