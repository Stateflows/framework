using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Stateflows;
using Stateflows.Common;
using Stateflows.Common.Interfaces;
using Stateflows.Entities;
using Stateflows.Entities.Attributes;
using Stateflows.StateMachines;

namespace Orleans.IntegrationTests;

[TestClass]
public sealed class EntityTransactionTests
{
    [TestMethod]
    [DataRow(false, false, false)]
    [DataRow(false, true, false)]
    [DataRow(true, false, false)]
    [DataRow(true, true, false)]
    [DataRow(false, false, true)]
    [DataRow(false, true, true)]
    [DataRow(true, false, true)]
    [DataRow(true, true, true)]
    public async Task EntityUpdatesFollowSourceOutcome(bool mutate, bool fail, bool join)
    {
        var listeners = new[] { new TcpListener(IPAddress.Loopback, 0), new TcpListener(IPAddress.Loopback, 0) };
        int[] ports;
        try
        {
            foreach (var listener in listeners)
                listener.Start();
            ports = listeners.Select(listener => ((IPEndPoint)listener.LocalEndpoint).Port).ToArray();
        }
        finally
        {
            foreach (var listener in listeners)
                listener.Stop();
        }

        var cluster = Guid.NewGuid().ToString("N");
        var entityOperationCompleted = false;
        using var host = Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) => services.AddStateflows(builder => builder
                .AddStateMachines(machines => machines.AddStateMachine("entity-transaction", machine => machine
                    .AddEntity<ICounter>()
                    .AddInitialState("initial", state => state.AddTransition<Update>("done", transition =>
                        transition.AddEffect(async context =>
                        {
                            var headers = join
                                ? new Dictionary<string, EventHeader> { [nameof(JoinTransaction)] = new JoinTransaction() }
                                : null;
                            var updated = mutate
                                ? await context.Behavior.TryMutateAsync<ICounter, Add>(new Add { Amount = 5 }, headers)
                                : await context.Behavior.TrySetFieldAsync<ICounter, int>(nameof(ICounter.Count), 5, headers);
                            if (!updated)
                                throw new InvalidOperationException("The entity update was not consumed.");
                            entityOperationCompleted = true;
                            if (fail)
                                throw new InvalidOperationException("The source execution failed.");
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
            using var source = factory.CreateBehavior(new StateMachineId("entity-transaction", "one"));
            using var entity = factory.CreateBehavior(
                new EntityId($"entity-transaction.entity.{Entity<ICounter>.Name}", "one"));
            Assert.AreEqual(EventStatus.Initialized, (await source.SendAsync(new Initialize())).Status);
            Assert.AreEqual(EventStatus.Consumed, (await entity.SendAsync(
                new FieldState<int> { Name = nameof(ICounter.Count), Value = 0 })).Status);

            var result = await source.SendAsync(new Update()).WaitAsync(TimeSpan.FromSeconds(15));
            Assert.IsTrue(entityOperationCompleted, "The entity update must complete before the source outcome is evaluated.");
            Assert.AreEqual(fail ? EventStatus.Failed : EventStatus.Consumed, result.Status);

            var count = await entity.RequestAsync(new FieldStateRequest<int> { Name = nameof(ICounter.Count) });
            Assert.AreEqual(EventStatus.Consumed, count.Status);
            Assert.AreEqual(fail && join ? 0 : 5, count.Response!.Value);
            var state = await source.RequestAsync(new StateMachineInfoRequest());
            Assert.AreEqual(fail ? "initial" : "done", state.Response.CurrentStates.Value);
        }
        finally
        {
            await host.StopAsync();
        }
    }

    public sealed class Update;

    public sealed class Add
    {
        public int Amount { get; set; }
    }

    public interface ICounter
    {
        [Field(FieldAccess.Get | FieldAccess.Set)]
        int Count { get; set; }

        [Mutation]
        void Apply(Add mutation) => Count += mutation.Amount;
    }
}
