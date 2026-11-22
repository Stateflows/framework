using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using StateMachine.IntegrationTests.Utils;
using Stateflows.Common;

namespace Action.IntegrationTests.Tests;

[TestClass]
public sealed class TenantInterception : StateflowsTestClass
{
    private readonly ConcurrentQueue<string> trace = new();

    [TestInitialize]
    public override void Initialize() => base.Initialize();

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();

    protected override void InitializeStateflows(IStateflowsBuilder builder)
    {
        builder.ServiceCollection.AddSingleton<IStateflowsTenantInterceptor>(new Probe(this, "first"));
        builder.ServiceCollection.AddSingleton<IStateflowsTenantInterceptor>(new Probe(this, "second"));
    }

    [TestMethod]
    public async Task TenantInterceptorsWrapExecutionInRegistrationOrder()
    {
        trace.Clear();
        await ServiceProvider.GetRequiredService<IStateflowsTenantExecutor>()
            .ExecuteByTenantAsync("tenant", async () =>
            {
                trace.Enqueue("action");
                await Task.Yield();
            });

        CollectionAssert.AreEqual(
            new[] { "first:before", "second:before", "action", "second:after", "first:after" },
            trace.ToArray());
    }

    private sealed class Probe(TenantInterception owner, string name) : IStateflowsTenantInterceptor
    {
        public async Task ExecuteAsync(string tenantId, Func<Task> next)
        {
            owner.trace.Enqueue($"{name}:before");
            await next();
            owner.trace.Enqueue($"{name}:after");
        }
    }
}
