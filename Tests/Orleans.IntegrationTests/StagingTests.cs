using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Orleans.Storage;
using Stateflows;
using Stateflows.Actions;
using Stateflows.Activities;
using Stateflows.Common;
using Stateflows.Common.Context;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Classes;
using Stateflows.Entities;
using Stateflows.Entities.Attributes;
using Stateflows.StateMachines;

namespace Orleans.IntegrationTests;

[TestClass]
public sealed class StagingTests
{
    [TestMethod]
    [DataRow("mutation")]
    [DataRow("field")]
    [DataRow("sending")]
    public async Task StreamedActivityRejectsStaging(string operation)
    {
        var observed = new TaskCompletionSource<Exception>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var host = CreateHost(services => services.AddStateflows(builder => builder
            .AddActivities(activities => activities.AddActivity("stream-staging", activity => activity
                .AddInputPin<int>(input => input.AddFlow("stage"))
                .AddAction("stage", c =>
                {
                    try
                    {
                        switch (operation)
                        {
                            case "mutation":
                                c.Behavior.StageMutation<ICounter, Add>(new Add { Amount = 1 });
                                break;
                            case "field":
                                c.Behavior.StageFieldSetting<ICounter, int>(nameof(ICounter.Count), 1);
                                break;
                            default:
                                c.Behavior.StageSending(new Followup());
                                break;
                        }
                        observed.TrySetException(new AssertFailedException("Streamed staging was allowed."));
                    }
                    catch (InvalidOperationException exception)
                    {
                        observed.TrySetResult(exception);
                    }
                    return Task.CompletedTask;
                })))));
        await host.StartAsync();
        try
        {
            using var behavior = host.Services.GetRequiredService<IBehaviorFactory>()
                .CreateBehavior(new ActivityId("stream-staging", "one"));
            Assert.AreEqual(EventStatus.Accepted,
                (await behavior.SendAsync(new TokensInput().Add(1), new Dictionary<string, EventHeader>
                {
                    [nameof(Stateflows.Common.Stream)] = new Stateflows.Common.Stream()
                })).Status);
            var failure = await observed.Task.WaitAsync(TimeSpan.FromSeconds(10));
            StringAssert.Contains(failure.Message, "streamed execution");
        }
        finally
        {
            await host.StopAsync();
        }
    }

    [TestMethod]
    public void StagingWithoutTransactionFailsExplicitly()
        => Assert.Throws<InvalidOperationException>(() => StagingBag.RequireCurrent());

    [TestMethod]
    [DataRow("success")]
    [DataRow("handler-failure")]
    [DataRow("entity-failure")]
    [DataRow("not-consumed")]
    [DataRow("commit-failure")]
    [DataRow("after-execute-failure")]
    public async Task StagedEntityOperationsGateCommitAndSelfSendsFollowCommit(string scenario)
    {
        var probe = new Probe { Scenario = scenario };
        var storage = new TestStorage();
        using var host = CreateHost(services =>
        {
            services.AddSingleton<IStateflowsExecutionInterceptor>(probe);
            services.AddStateflows(builder => builder.AddStateMachines(machines => machines
                .AddStateMachine("staging", machine => machine
                    .AddEntity<ICounter>()
                    .AddInitialState("initial", state => state
                        .AddTransition<Run>("done", transition => transition.AddEffect(async c =>
                        {
                            c.Behavior.StageSending(new Followup());
                            c.Behavior.StageFieldSetting<ICounter, int>(nameof(ICounter.Count), 10);
                            var mutation = new Add { Amount = 2 };
                            c.Behavior.StageMutation<ICounter, Add>(mutation);
                            mutation.Amount = 900;
                            if (scenario == "entity-failure")
                                c.Behavior.StageMutation<ICounter, Add>(new Add { Amount = -1 });
                            if (scenario == "not-consumed")
                                c.Behavior.StageMutation<ICounter, Unknown>(new Unknown());
                            c.Behavior.StageMutation<ICounter, Add>(new Add { Amount = 3 });
                            var before = await c.Behavior.TryGetFieldAsync<ICounter, int>(nameof(ICounter.Count));
                            Assert.AreEqual(0, before.Field, "Staging must not send during handler execution.");
                            await c.Behavior.Values.SetAsync("committed", true);
                            if (scenario == "handler-failure")
                                throw new InvalidOperationException("handler failed");
                        })))
                    .AddState("done", state => state
                        .AddInternalTransition<Followup>(transition => transition.AddEffect(async c =>
                        {
                            var committed = await c.Behavior.Values.GetOrDefaultAsync("committed", false);
                            probe.Followup.TrySetResult(committed);
                        }))))));
        }, storage);
        await host.StartAsync();
        try
        {
            using var behavior = host.Services.GetRequiredService<IBehaviorFactory>()
                .CreateBehavior(new StateMachineId("staging", "one"));
            Assert.AreEqual(EventStatus.Initialized, (await behavior.SendAsync(new Initialize())).Status);
            storage.RejectSourceWrite = scenario == "commit-failure";
            var result = await behavior.SendAsync(new Run()).WaitAsync(TimeSpan.FromSeconds(15));
            Assert.AreEqual(scenario == "success" ? EventStatus.Consumed : EventStatus.Failed, result.Status);
            if (scenario == "success")
                Assert.IsTrue(await probe.Followup.Task.WaitAsync(TimeSpan.FromSeconds(10)),
                    "Self-send must execute after source values have committed.");
            else
            {
                Assert.IsFalse(probe.Followup.Task.IsCompleted);
                var status = await behavior.RequestAsync(new StateMachineInfoRequest());
                Assert.AreEqual("initial", status.Response.CurrentStates.Value, "Source transition must not commit.");
            }

            var entityId = new EntityId($"staging.entity.{Entity<ICounter>.Name}", "one");
            using var entity = host.Services.GetRequiredService<IBehaviorFactory>().CreateBehavior(entityId);
            var count = await entity.RequestAsync(new FieldStateRequest<int> { Name = nameof(ICounter.Count) });
            var expected = scenario is "handler-failure" or "after-execute-failure" ? 0
                : scenario is "entity-failure" or "not-consumed" ? 12 : 15;
            Assert.AreEqual(expected, count.Response!.Value,
                "Earlier entity commits survive later failures; later staged operations must not execute.");
            Assert.IsFalse(storage.RejectSourceWrite);
        }
        finally
        {
            await host.StopAsync();
        }
    }

    [TestMethod]
    [DataRow(BehaviorType.StateMachine)]
    [DataRow(BehaviorType.Action)]
    [DataRow(BehaviorType.Activity)]
    [DataRow(BehaviorType.Entity)]
    public async Task EveryBehaviorTypeSupportsStagedEntityOperations(string type)
    {
        async Task StageAsync(IBehaviorContext context)
        {
            context.StageFieldSetting<ICounter, int>(nameof(ICounter.Count), 4);
            context.StageMutation<ICounter, Add>(new Add { Amount = 3 });
            await Task.Yield();
        }

        using var host = CreateHost(services => services.AddStateflows(builder => builder
            .AddStateMachines(machines => machines.AddStateMachine("source", machine => machine
                .AddInitialState("initial", state => state.AddOnEntry(c => StageAsync(c.Behavior)))))
            .AddActions(actions => actions.AddAction("source", c => StageAsync(c.Behavior)))
            .AddActivities(activities => activities.AddActivity("source", activity => activity
                .AddInitial(initial => initial.AddControlFlow("stage"))
                .AddAction("stage", c => StageAsync(c.Behavior))))
            .AddEntities(entities => entities
                .AddEntity<ICounter>($"source.entity.{Entity<ICounter>.Name}")
                .AddEntity<ICounter>("source", entity => entity
                    .AddObserver((_, _) => new StagingObserver(StageAsync))))));
        await host.StartAsync();
        try
        {
            using var source = host.Services.GetRequiredService<IBehaviorFactory>()
                .CreateBehavior(new BehaviorId(type, "source", "one"));
            var result = type == BehaviorType.Entity
                ? await source.SendAsync(new Add { Amount = 1 })
                : type == BehaviorType.Action
                ? await source.SendAsync(new Run())
                : await source.SendAsync(new Initialize());
            Assert.AreNotEqual(EventStatus.Failed, result.Status);
            using var target = host.Services.GetRequiredService<IBehaviorFactory>()
                .CreateBehavior(new EntityId($"source.entity.{Entity<ICounter>.Name}", "one"));
            var count = await target.RequestAsync(new FieldStateRequest<int> { Name = nameof(ICounter.Count) });
            Assert.AreEqual(7, count.Response!.Value);
        }
        finally
        {
            await host.StopAsync();
        }
    }

    private static IHost CreateHost(System.Action<IServiceCollection> configure, TestStorage? storage = null)
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
        return Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) => configure(services))
            .ConfigureLogging(logging => logging.SetMinimumLevel(LogLevel.Error))
            .UseOrleans(silo =>
            {
                silo.UseLocalhostClustering(ports[0], ports[1], clusterId: cluster, serviceId: cluster)
                    .UseInMemoryReminderService()
                    .AddStateflowsRuntime();
                if (storage == null)
                    silo.AddMemoryGrainStorageAsDefault();
                else
                    silo.ConfigureServices(services => services.AddKeyedSingleton<IGrainStorage>("Default",
                        (provider, _) => storage.Attach(provider.GetRequiredService<IGrainStorageSerializer>())));
            })
            .Build();
    }

    public sealed class Run;
    public sealed class Followup;
    public sealed class Unknown;
    public sealed class Add
    {
        public int Amount { get; set; }
    }

    public interface ICounter
    {
        [Field(FieldAccess.Get | FieldAccess.Set)]
        int Count { get; set; }

        [Mutation]
        void Apply(Add mutation)
        {
            if (mutation.Amount < 0)
                throw new InvalidOperationException("entity failed");
            Count += mutation.Amount;
        }
    }

    private sealed class StagingObserver(Func<IBehaviorContext, Task> stage) : IEntityObserver
    {
        public async Task OnMutationAsync<TTemplate, TEvent>(IMutationObservationContext<TTemplate, TEvent> context, Func<Task> next)
        {
            await next();
            await stage(context.Behavior);
        }

        public Task OnComputedFieldAsync<TTemplate, TField>(IComputedFieldObservationContext<TTemplate> context, Func<Task<TField>> next)
            => next();

        public Task OnProjectionAsync<TTemplate, TProjection>(IProjectionObservationContext<TTemplate> context, Func<Task<TProjection>> next)
            => next();
    }

    private sealed class Probe : IStateflowsExecutionInterceptor
    {
        public string Scenario { get; init; } = "";
        public TaskCompletionSource<bool> Followup { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public bool BeforeExecute(BehaviorId id, EventHolder holder) => true;
        public void AfterExecute(BehaviorId id, EventHolder holder)
        {
            if (Scenario == "after-execute-failure" && holder.BoxedPayload is Run)
                throw new InvalidOperationException("after-execute failed");
        }
    }

    private sealed class TestStorage : IGrainStorage
    {
        private readonly ConcurrentDictionary<(string, GrainId), BinaryData> states = new();
        private IGrainStorageSerializer serializer = null!;
        public bool RejectSourceWrite;

        public IGrainStorage Attach(IGrainStorageSerializer value)
        {
            serializer = value;
            return this;
        }

        public Task ReadStateAsync<T>(string stateName, GrainId id, IGrainState<T> state)
        {
            state.RecordExists = states.TryGetValue((stateName, id), out var data);
            state.State = data == null ? Activator.CreateInstance<T>() : serializer.Deserialize<T>(data);
            return Task.CompletedTask;
        }

        public Task WriteStateAsync<T>(string stateName, GrainId id, IGrainState<T> state)
        {
            if (RejectSourceWrite && state.State is BehaviorContextState context &&
                Stateflows.Common.Utilities.StateflowsJsonConverter.DeserializeObject<StateflowsContext>(context.Payload).Id.Type == BehaviorType.StateMachine)
            {
                RejectSourceWrite = false;
                throw new InvalidOperationException("source commit failed");
            }
            states[(stateName, id)] = serializer.Serialize(state.State);
            state.RecordExists = true;
            return Task.CompletedTask;
        }

        public Task ClearStateAsync<T>(string stateName, GrainId id, IGrainState<T> state)
        {
            states.TryRemove((stateName, id), out _);
            state.RecordExists = false;
            return Task.CompletedTask;
        }
    }
}
