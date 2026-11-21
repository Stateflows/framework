using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Stateflows;
using Stateflows.Common;
using Stateflows.Common.Interfaces;
using Stateflows.Entities;
using Stateflows.Entities.Attributes;
using Stateflows.Entities.Enums;
using Stateflows.StateMachines;

namespace Orleans.IntegrationTests;

public sealed partial class BehaviorNotificationTests
{
    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task RelatedEntityCommitIsIndependentOfSourceOutcome(bool failSource)
    {
        var ports = GetFreeTcpPorts(2);
        var cluster = Guid.NewGuid().ToString("N");
        using var host = Host.CreateDefaultBuilder()
            .ConfigureServices(services => services.AddStateflows(builder => builder
                .AddStateMachines(machines => machines.AddStateMachine("entity-operations", machine => machine
                    .AddEntity<IIndependentCounter>()
                    .AddInitialState("initial", state => state.AddTransition<UpdateIndependentCounter>("done",
                        transition => transition.AddEffect(async context =>
                        {
                            if (!await context.Behavior.TrySetFieldAsync<IIndependentCounter, int>(
                                    nameof(IIndependentCounter.Count), 5))
                                throw new InvalidOperationException("Entity update failed.");
                            if (failSource)
                                throw new InvalidOperationException("Source failed after the entity update.");
                        })))
                    .AddState("done")))))
            .ConfigureLogging(logging => logging.SetMinimumLevel(LogLevel.Error))
            .UseOrleans(silo => silo
                .UseLocalhostClustering(ports[0], ports[1], clusterId: cluster, serviceId: cluster)
                .AddMemoryGrainStorageAsDefault()
                .UseInMemoryReminderService()
                .AddStateflowsRuntime())
            .Build();

        await host.StartAsync();
        try
        {
            var factory = host.Services.GetRequiredService<IBehaviorFactory>();
            using var source = factory.CreateBehavior(new StateMachineId("entity-operations", "one"));
            using var entity = factory.CreateBehavior(new EntityId(
                $"entity-operations.entity.{Entity<IIndependentCounter>.Name}", "one"));
            Assert.AreEqual(EventStatus.Initialized, (await source.SendAsync(new Initialize())).Status);
            Assert.AreEqual(failSource ? EventStatus.Failed : EventStatus.Consumed,
                (await source.SendAsync(new UpdateIndependentCounter())).Status);
            var count = await entity.RequestAsync(new FieldStateRequest<int> { Name = nameof(IIndependentCounter.Count) });
            Assert.AreEqual(5, count.Response!.Value);
            Assert.AreEqual(failSource ? "initial" : "done",
                (await source.RequestAsync(new StateMachineInfoRequest())).Response.CurrentStates.Value);
        }
        finally
        {
            await host.StopAsync();
        }
    }

    public interface IIndependentCounter
    {
        [Field(FieldAccess.Get | FieldAccess.Set)]
        int Count { get; set; }
    }

    public sealed class UpdateIndependentCounter;
}
