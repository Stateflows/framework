using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Orleans.Storage;
using Stateflows;
using Stateflows.Activities;
using Stateflows.Common;
using Stateflows.Common.Context;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Classes;
using Stateflows.StateMachines;

namespace Orleans.IntegrationTests;

[TestClass]
public sealed class StagingTests
{
    [TestMethod]
    public async Task StreamedActivityRejectsStagedSending()
    {
        var observed = new TaskCompletionSource<Exception>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var host = CreateHost(services => services.AddStateflows(builder => builder
            .AddActivities(activities => activities.AddActivity("stream-staging", activity => activity
                .AddInputPin<int>(input => input.AddFlow("stage"))
                .AddAction("stage", c =>
                {
                    try
                    {
                        c.Behavior.StageSending(new Followup());
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
    [DataRow("commit-failure")]
    [DataRow("after-execute-failure")]
    public async Task StagedSelfSendsFollowSuccessfulCommit(string scenario)
    {
        var probe = new Probe { Scenario = scenario };
        var storage = new TestStorage();
        using var host = CreateHost(services =>
        {
            services.AddSingleton<IStateflowsExecutionInterceptor>(probe);
            services.AddStateflows(builder => builder.AddStateMachines(machines => machines
                .AddStateMachine("staging", machine => machine
                    .AddInitialState("initial", state => state
                        .AddTransition<Run>("done", transition => transition.AddEffect(async c =>
                        {
                            c.Behavior.StageSending(new Followup());
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
            if (scenario == "commit-failure")
                await TransactionalStorageTestHelpers.AssertTransactionCommitFailureAsync(
                    () => behavior.SendAsync(new Run()).WaitAsync(TimeSpan.FromSeconds(15)), "source commit failed");
            else
                Assert.AreEqual(scenario == "success" ? EventStatus.Consumed : EventStatus.Failed,
                    (await behavior.SendAsync(new Run()).WaitAsync(TimeSpan.FromSeconds(15))).Status);

            if (scenario == "success")
                Assert.IsTrue(await probe.Followup.Task.WaitAsync(TimeSpan.FromSeconds(10)),
                    "Self-send must execute after source values have committed.");
            else
            {
                Assert.IsFalse(probe.Followup.Task.IsCompleted);
                var status = await behavior.RequestAsync(new StateMachineInfoRequest());
                Assert.AreEqual("initial", status.Response.CurrentStates.Value, "Source transition must not commit.");
            }

            Assert.IsFalse(storage.RejectSourceWrite);
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
        private readonly ConcurrentDictionary<(string, GrainId), (BinaryData Data, string ETag)> states = new();
        private IGrainStorageSerializer serializer = null!;
        public bool RejectSourceWrite;

        public IGrainStorage Attach(IGrainStorageSerializer value)
        {
            serializer = value;
            return this;
        }

        public Task ReadStateAsync<T>(string stateName, GrainId id, IGrainState<T> state)
        {
            if (states.TryGetValue((stateName, id), out var stored))
            {
                state.State = serializer.Deserialize<T>(stored.Data);
                state.ETag = stored.ETag;
                state.RecordExists = true;
            }
            else
            {
                state.State = Activator.CreateInstance<T>();
                state.ETag = null;
                state.RecordExists = false;
            }
            return Task.CompletedTask;
        }

        public Task WriteStateAsync<T>(string stateName, GrainId id, IGrainState<T> state)
        {
            if (RejectSourceWrite && stateName == "Stateflows.BehaviorContext" &&
                TransactionalStorageTestHelpers.GetBehaviorContextStates(state.State).Any(context =>
                    Stateflows.Common.Utilities.StateflowsJsonConverter.DeserializeObject<StateflowsContext>(context.Payload)?.Id.Type == BehaviorType.StateMachine))
            {
                RejectSourceWrite = false;
                throw new InvalidOperationException("source commit failed");
            }
            var eTag = Guid.NewGuid().ToString("N");
            states[(stateName, id)] = (serializer.Serialize(state.State), eTag);
            state.ETag = eTag;
            state.RecordExists = true;
            return Task.CompletedTask;
        }

        public Task ClearStateAsync<T>(string stateName, GrainId id, IGrainState<T> state)
        {
            states.TryRemove((stateName, id), out _);
            state.ETag = null;
            state.RecordExists = false;
            return Task.CompletedTask;
        }
    }
}
