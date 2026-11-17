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
        var sourceReadyToFinish = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var finishSource = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var fieldReadDuringTransaction = -1;
        var projectionReadDuringTransaction = -1;
        using var host = Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) => services.AddStateflows(builder => builder
                .AddStateMachines(machines => machines.AddStateMachine("entity-transaction", machine => machine
                    .AddEntity<ICounter>(entity => entity
                        .AddProjection<CounterProjection>(template => new CounterProjection(template.Count)))
                    .AddInitialState("initial", state => state.AddTransition<Update>("done", transition =>
                        transition.AddEffect(async context =>
                        {
                            var updated = mutate
                                ? join
                                    ? await context.Behavior.TryMutateTransactionallyAsync<ICounter, Add>(new Add { Amount = 5 })
                                    : await context.Behavior.TryMutateAsync<ICounter, Add>(new Add { Amount = 5 })
                                : join
                                    ? await context.Behavior.TrySetFieldTransactionallyAsync<ICounter, int>(nameof(ICounter.Count), 5)
                                    : await context.Behavior.TrySetFieldAsync<ICounter, int>(nameof(ICounter.Count), 5);
                            if (!updated)
                                throw new InvalidOperationException("The entity update was not consumed.");

                            var field = join
                                ? await context.Behavior.TryGetFieldTransactionallyAsync<ICounter, int>(nameof(ICounter.Count))
                                : await context.Behavior.TryGetFieldAsync<ICounter, int>(nameof(ICounter.Count));
                            var projection = join
                                ? await context.Behavior.TryGetProjectionTransactionallyAsync<ICounter, CounterProjection>()
                                : await context.Behavior.TryGetProjectionAsync<ICounter, CounterProjection>();
                            if (!field.Success || !projection.Success)
                                throw new InvalidOperationException("The entity state could not be read during the source execution.");

                            fieldReadDuringTransaction = field.Field;
                            projectionReadDuringTransaction = projection.Projection.Count;
                            entityOperationCompleted = true;
                            sourceReadyToFinish.TrySetResult();
                            await finishSource.Task;
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

            var processing = source.SendAsync(new Update());
            await sourceReadyToFinish.Task.WaitAsync(TimeSpan.FromSeconds(15));
            Assert.IsTrue(entityOperationCompleted, "The entity update must complete before the source outcome is evaluated.");
            Assert.AreEqual(5, fieldReadDuringTransaction, "The source must read its own entity update.");
            Assert.AreEqual(5, projectionReadDuringTransaction, "The source must read its own refreshed projection.");

            finishSource.TrySetResult();
            var result = await processing.WaitAsync(TimeSpan.FromSeconds(15));
            Assert.AreEqual(fail ? EventStatus.Failed : EventStatus.Consumed, result.Status);

            var count = await entity.RequestAsync(new FieldStateRequest<int> { Name = nameof(ICounter.Count) });
            Assert.AreEqual(EventStatus.Consumed, count.Status);
            Assert.AreEqual(fail && join ? 0 : 5, count.Response!.Value);
            var projection = await entity.RequestAsync(new ProjectionRequest<CounterProjection>());
            Assert.AreEqual(EventStatus.Consumed, projection.Status);
            Assert.AreEqual(fail && join ? 0 : 5, projection.Response!.Count);
            var state = await source.RequestAsync(new StateMachineInfoRequest());
            Assert.AreEqual(fail ? "initial" : "done", state.Response.CurrentStates.Value);
        }
        finally
        {
            finishSource.TrySetResult();
            await host.StopAsync();
        }
    }

    public sealed class Update;

    public sealed class Add
    {
        public int Amount { get; set; }
    }

    public sealed record CounterProjection(int Count);

    public interface ICounter
    {
        [Field(FieldAccess.Get | FieldAccess.Set)]
        int Count { get; set; }

        [Mutation]
        void Apply(Add mutation) => Count += mutation.Amount;
    }
}
