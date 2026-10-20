using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Orleans.Storage;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Stateflows;
using Stateflows.Actions;
using Stateflows.Activities;
using Stateflows.Common;
using Stateflows.Common.Context;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Registration.Interfaces;
using Stateflows.Common.Utilities;
using Stateflows.Entities.Attributes;
using Stateflows.Entities;
using Stateflows.StateMachines;

namespace Orleans.IntegrationTests;

[TestClass]
public sealed class FailedExecutionTests
{
    [TestCleanup]
    public void Cleanup() => ResponseHolder.ClearResponses();

    [TestMethod]
    [DataRow(BehaviorType.StateMachine)]
    [DataRow(BehaviorType.Activity)]
    [DataRow(BehaviorType.Action)]
    [DataRow(BehaviorType.Entity)]
    public async Task FailedInitializationLeavesNoStoredContext(string type)
    {
        var storage = new ReferenceStorage();
        await using var provider = CreateProvider(storage, new FailureProbe { Fail = true });
        var processor = provider.GetServices<IEventProcessor>().Single(p => p.BehaviorTypes.Contains(type));
        var errors = new List<Exception>();
        ResponseHolder.SetResponses(new Dictionary<object, EventHolder>());

        var result = await processor.ProcessEventAsync(new BehaviorId(type, "rollback", "one"),
            new Initialize().ToEventHolder(), errors);

        Assert.AreEqual(EventStatus.Failed, result);
        Assert.IsTrue(errors.Count > 0);
        Assert.IsNull(storage.State);
        Assert.AreEqual(0, storage.Writes);
    }

    [TestMethod]
    [DataRow(BehaviorType.StateMachine)]
    [DataRow(BehaviorType.Activity)]
    [DataRow(BehaviorType.Action)]
    [DataRow(BehaviorType.Entity)]
    public async Task FailedHydrationIsReportedWithoutDehydration(string type)
    {
        var storage = new ReferenceStorage { RejectReads = true };
        await using var provider = CreateProvider(storage, new FailureProbe());
        var processor = provider.GetServices<IEventProcessor>().Single(p => p.BehaviorTypes.Contains(type));
        var errors = new List<Exception>();

        var result = await processor.ProcessEventAsync(new BehaviorId(type, "rollback", "one"),
            new Initialize().ToEventHolder(), errors);

        Assert.AreEqual(EventStatus.Failed, result);
        Assert.AreEqual(1, errors.Count);
        Assert.AreEqual(0, storage.Writes);
    }

    [TestMethod]
    [DataRow(BehaviorType.StateMachine)]
    [DataRow(BehaviorType.Activity)]
    [DataRow(BehaviorType.Action)]
    [DataRow(BehaviorType.Entity)]
    public Task OrleansRejectedWriteDoesNotLeakStateIntoNextExecutionOrReactivation(string type)
        => VerifyOrleansFailureAsync(type, FailureMode.Write);

    [TestMethod]
    [DataRow(BehaviorType.StateMachine)]
    [DataRow(BehaviorType.Activity)]
    [DataRow(BehaviorType.Action)]
    [DataRow(BehaviorType.Entity)]
    public Task OrleansExecutionFailureDoesNotLeakStateIntoNextExecutionOrReactivation(string type)
        => VerifyOrleansFailureAsync(type, FailureMode.Execution);

    [TestMethod]
    [DataRow(BehaviorType.StateMachine)]
    [DataRow(BehaviorType.Activity)]
    [DataRow(BehaviorType.Action)]
    public Task OrleansRejectedLogicalDeleteDoesNotDiscardCommittedState(string type)
        => VerifyOrleansFailureAsync(type, FailureMode.Clear);

    [TestMethod]
    [DataRow(BehaviorType.StateMachine)]
    [DataRow(BehaviorType.Activity)]
    [DataRow(BehaviorType.Action)]
    [DataRow(BehaviorType.Entity)]
    public Task OrleansAfterExecuteFailureDoesNotCommitState(string type)
        => VerifyOrleansFailureAsync(type, FailureMode.AfterExecute);

    private static async Task VerifyOrleansFailureAsync(string type, FailureMode mode)
    {
        var storage = new FaultStorage();
        var probe = new FailureProbe();
        var ports = GetPorts();
        var cluster = Guid.NewGuid().ToString("N");
        using var host = Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
            {
                services.AddStateflows(sf => RegisterBehaviors(sf, probe));
            })
            .ConfigureLogging(l => l.SetMinimumLevel(LogLevel.Error))
            .UseOrleans(silo => silo
                .UseLocalhostClustering(ports[0], ports[1], clusterId: cluster, serviceId: cluster)
                .ConfigureServices(s => s
                    .AddKeyedSingleton<IGrainStorage>("Default",
                        (provider, _) => storage.Attach(provider.GetRequiredService<IGrainStorageSerializer>()))
                    .AddSingleton<IStateflowsExecutionInterceptor>(probe))
                .UseInMemoryReminderService()
                .AddStateflowsRuntime()
            )
            .Build();
        try
        {
            await host.StartAsync();
            var id = new BehaviorId(type, "rollback", "one");
            using var behavior = host.Services.GetRequiredService<IBehaviorFactory>().CreateBehavior(id);
            Assert.AreNotEqual(EventStatus.Failed, (await behavior.SendAsync(new Initialize())).Status);
            var before = storage.ContextSnapshot!;
            // Initialization has a receipt, so logical deletion must write a metadata-only tombstone atomically.
            storage.RejectNextWrite = mode is FailureMode.Write or FailureMode.Clear;
            probe.Value = "changed";
            probe.Fail = mode == FailureMode.Execution;
            probe.FailAfterExecute = mode == FailureMode.AfterExecute;

            var result = mode == FailureMode.Clear
                ? await behavior.SendAsync(new Reset())
                : type == BehaviorType.Entity && mode != FailureMode.Execution
                ? await behavior.SendAsync(new FieldState<List<string>> { Name = "Items", Value = new List<string> { "changed" } })
                : await behavior.SendAsync(new Mutate());

            Assert.AreEqual(EventStatus.Failed, result.Status);
            Assert.IsFalse(storage.RejectNextWrite, "The execution must reach the rejected storage write.");
            Assert.IsFalse(storage.RejectNextClear, "The execution must reach the rejected storage clear.");
            Assert.AreEqual(before, storage.ContextSnapshot);

            probe.Fail = false;
            probe.FailAfterExecute = false;
            Assert.AreNotEqual(EventStatus.Failed, (await behavior.SendAsync(new BehaviorInfoRequest())).Status);
            AssertRestoredContext(before, storage.ContextSnapshot!);
            if (mode is FailureMode.Write or FailureMode.Clear)
                Assert.IsTrue(storage.ContextReads >= 2, "State must have been reloaded from Orleans storage.");
            else
                Assert.AreEqual(1, storage.ContextReads, "Execution failure must also be isolated in the same activation.");
            var reads = storage.ContextReads;
            var management = host.Services.GetRequiredService<IGrainFactory>().GetGrain<IManagementGrain>(0);
            await management.ForceActivationCollection(TimeSpan.Zero);
            Assert.AreNotEqual(EventStatus.Failed, (await behavior.SendAsync(new BehaviorInfoRequest())).Status);
            Assert.IsTrue(storage.ContextReads > reads);
            AssertRestoredContext(before, storage.ContextSnapshot!);
        }
        finally
        {
            await host.StopAsync();
        }
    }

    private static void AssertRestoredContext(string before, string after)
    {
        var expected = StateflowsJsonConverter.DeserializeObject<StateflowsContext>(before);
        var actual = StateflowsJsonConverter.DeserializeObject<StateflowsContext>(after);
        actual.LastExecutedAt = expected.LastExecutedAt;
        foreach (var receipt in expected.EventReceipts)
        {
            Assert.IsTrue(actual.EventReceipts.TryGetValue(receipt.Key, out var expiry));
            Assert.AreEqual(receipt.Value, expiry, "A later request must not alter existing receipts.");
        }
        actual.EventReceipts = expected.EventReceipts;
        Assert.AreEqual(before, StateflowsJsonConverter.SerializePolymorphicObject(actual),
            "A subsequent successful execution must not persist any failed mutation.");
    }

    private enum FailureMode { Execution, Write, Clear, AfterExecute }

    [TestMethod]
    [DataRow(BehaviorType.StateMachine)]
    [DataRow(BehaviorType.Activity)]
    [DataRow(BehaviorType.Action)]
    [DataRow(BehaviorType.Entity)]
    public async Task FailedExecutionDoesNotMutateHydratedStateOrWriteIt(string type)
    {
        var probe = new FailureProbe();
        var storage = new ReferenceStorage();
        await using var provider = CreateProvider(storage, probe);
        var processor = provider.GetServices<IEventProcessor>().Single(p => p.BehaviorTypes.Contains(type));
        var id = new BehaviorId(type, "rollback", "one");
        var errors = new List<Exception>();
        ResponseHolder.SetResponses(new Dictionary<object, EventHolder>());
        Assert.AreNotEqual(EventStatus.Failed, await processor.ProcessEventAsync(id, new Initialize().ToEventHolder(), errors),
            string.Join(Environment.NewLine, errors));
        var committed = storage.State!;
        var before = StateflowsJsonConverter.SerializePolymorphicObject(committed);
        storage.ResetCounts();
        probe.Fail = true;

        var result = await processor.ProcessEventAsync(id, new Mutate().ToEventHolder(), errors);

        Assert.AreEqual(EventStatus.Failed, result);
        Assert.AreSame(committed, storage.State);
        Assert.AreEqual(before, StateflowsJsonConverter.SerializePolymorphicObject(storage.State),
            "Failed execution must not change even nested values in the hydrated object.");
        Assert.AreEqual(0, storage.Writes);
        Assert.AreEqual(type == BehaviorType.Action ? 2 : 1, storage.Reads);

        probe.Fail = false;
        Assert.AreNotEqual(EventStatus.Failed,
            await processor.ProcessEventAsync(id, new BehaviorInfoRequest().ToEventHolder(), new List<Exception>()));
        Assert.IsFalse(storage.State!.GlobalValues.Values.Any(v => v.Contains("failed")));
        if (type == BehaviorType.Entity)
            CollectionAssert.AreEqual(new[] { "initial" }, (List<string>)storage.State.Values["$field:Items"]);
    }

    [TestMethod]
    [DataRow(BehaviorType.StateMachine)]
    [DataRow(BehaviorType.Activity)]
    [DataRow(BehaviorType.Action)]
    [DataRow(BehaviorType.Entity)]
    public async Task SuccessfulExecutionCommitsExactlyOnce(string type)
    {
        var storage = new ReferenceStorage();
        await using var provider = CreateProvider(storage, new FailureProbe());
        var processor = provider.GetServices<IEventProcessor>().Single(p => p.BehaviorTypes.Contains(type));
        var errors = new List<Exception>();
        ResponseHolder.SetResponses(new Dictionary<object, EventHolder>());

        var status = await processor.ProcessEventAsync(new BehaviorId(type, "rollback", "one"),
            new Initialize().ToEventHolder(), errors);

        Assert.AreNotEqual(EventStatus.Failed, status, string.Join(Environment.NewLine, errors));
        Assert.AreEqual(0, errors.Count);
        Assert.AreEqual(type == BehaviorType.Action ? 2 : 1, storage.Reads);
        Assert.AreEqual(1, storage.Writes);
        Assert.AreEqual(1, storage.State!.Version);
    }

    [TestMethod]
    public async Task HandledActivityExceptionStillRollsBack()
    {
        var storage = new ReferenceStorage();
        var probe = new FailureProbe { Fail = true };
        await using var provider = CreateProvider(storage, probe);
        var processor = provider.GetServices<IEventProcessor>().Single(p => p.BehaviorTypes.Contains(BehaviorType.Activity));
        var errors = new List<Exception>();
        ResponseHolder.SetResponses(new Dictionary<object, EventHolder>());

        var result = await processor.ProcessEventAsync(new BehaviorId(BehaviorType.Activity, "handled", "one"),
            new Initialize().ToEventHolder(), errors);

        Assert.IsTrue(probe.Handled, string.Join(Environment.NewLine, errors));
        Assert.AreEqual(EventStatus.Failed, result);
        Assert.IsTrue(errors.Count > 0);
        Assert.IsNull(storage.State);
        Assert.AreEqual(0, storage.Writes);
    }

    [TestMethod]
    public async Task HandledStateMachineExceptionStillRollsBack()
    {
        var storage = new ReferenceStorage();
        var probe = new FailureProbe();
        await using var provider = CreateProvider(storage, probe);
        var processor = provider.GetServices<IEventProcessor>().Single(p => p.BehaviorTypes.Contains(BehaviorType.StateMachine));
        var errors = new List<Exception>();
        ResponseHolder.SetResponses(new Dictionary<object, EventHolder>());
        var id = new BehaviorId(BehaviorType.StateMachine, "handled", "one");
        Assert.AreEqual(EventStatus.Initialized, await processor.ProcessEventAsync(id, new Initialize().ToEventHolder(), errors));
        var before = StateflowsJsonConverter.SerializePolymorphicObject(storage.State);
        storage.ResetCounts();

        var result = await processor.ProcessEventAsync(id, new Mutate().ToEventHolder(), errors);

        Assert.IsTrue(probe.Handled);
        Assert.AreEqual(EventStatus.Failed, result);
        Assert.IsTrue(errors.Count > 0);
        Assert.AreEqual(before, StateflowsJsonConverter.SerializePolymorphicObject(storage.State));
        Assert.AreEqual(0, storage.Writes);
    }

    private static ServiceProvider CreateProvider(ReferenceStorage storage, FailureProbe probe)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(probe);
        services.AddSingleton<IStateflowsStorage>(storage);
        // Processors are exercised directly; the legacy runtime only supplies the IBehaviorFactory required by behavior providers.
        services.AddStateflows(sf => RegisterBehaviors(sf.AddLegacyRuntime(), probe));
        var provider = services.BuildServiceProvider();
        provider.GetRequiredService<ITenantAccessor>().CurrentTenantId = "host";
        return provider;
    }

    private static void RegisterBehaviors(IStateflowsBuilder sf, FailureProbe probe)
        => sf
            .AddStateMachines(m => m
                .AddStateMachine("rollback", b => b
                .AddInitialState("idle", s => s
                    .AddOnEntry(c =>
                    {
                        probe.UpdateContext((IStateflowsContextProvider)c);
                        probe.ThrowIfRequested();
                        return Task.CompletedTask;
                    })
                    .AddTransition<Mutate>("changed",
                    t => t.AddEffect(async c =>
                    {
                        probe.UpdateContext((IStateflowsContextProvider)c);
                        await c.Behavior.Values.SetAsync("value", probe.Fail ? "failed" : probe.Value);
                        probe.ThrowIfRequested();
                    })))
                .AddState("changed"))
                .AddStateMachine("handled", b => b
                    .AddInitialState("idle", s => s.AddTransition<Mutate>("changed"))
                    .AddState("changed", s => s.AddOnEntry(async c =>
                    {
                        await c.Behavior.Values.SetAsync("value", "failed");
                        throw new InvalidOperationException("Expected handled exception.");
                    }))
                    .AddExceptionHandler<HandledMachineException>()))
            .AddActivities(a => a
                .AddActivity("rollback", b => b
                .AddDefaultInitializer(c =>
                {
                    probe.UpdateContext((IStateflowsContextProvider)c);
                    probe.ThrowIfRequested();
                    return Task.FromResult(true);
                })
                .AddAcceptEventAction<Mutate>("mutate", async c =>
                {
                    probe.UpdateContext((IStateflowsContextProvider)c);
                    await c.Behavior.Values.SetAsync("value", probe.Fail ? "failed" : probe.Value);
                    probe.ThrowIfRequested();
                }))
                .AddActivity("handled", b => b
                    .AddInitial(i => i.AddControlFlow("mutate"))
                    .AddAction("mutate", async c =>
                    {
                        await c.Behavior.Values.SetAsync("value", "failed");
                        probe.ThrowIfRequested();
                    }, n => n.AddExceptionHandler<InvalidOperationException>(_ =>
                    {
                        probe.Handled = true;
                        return Task.CompletedTask;
                    }))))
            .AddActions(a => a.AddAction("rollback", async c =>
            {
                probe.UpdateContext((IStateflowsContextProvider)c);
                await c.Behavior.Values.SetAsync("value", probe.Fail ? "failed" : probe.Value);
                probe.ThrowIfRequested();
            }))
            .AddEntities(e => e.AddEntity<IRollbackEntity>("rollback", b => b
                .AddDefaultInitializer(c =>
                {
                    c.Entity.Items = new List<string> { "initial" };
                    probe.ThrowIfRequested();
                })));

    public sealed class Mutate;

    public interface IRollbackEntity
    {
        // Settable, so the FieldState write in the rejected-write scenario changes state and reaches storage.
        [Field(FieldAccess.Set)]
        List<string> Items { get; set; }

        [Mutation]
        void Change(Mutate _)
        {
            Items.Add("failed");
            throw new InvalidOperationException("Expected mutation failure.");
        }
    }

    private sealed class FailureProbe : IStateflowsExecutionInterceptor
    {
        public bool Fail;
        public bool Handled;
        public string Value = "initial";
        public bool FailAfterExecute;
        public bool BeforeExecute(BehaviorId id, EventHolder eventHolder) => true;
        public void AfterExecute(BehaviorId id, EventHolder eventHolder)
        {
            if (FailAfterExecute) throw new InvalidOperationException("Expected execution interceptor failure.");
        }
        public void UpdateContext(IStateflowsContextProvider provider)
        {
            var context = provider.Context;
            if (!context.Values.TryGetValue("nested", out var value))
            {
                value = new List<List<int>> { new() { 1 } };
                context.Values["nested"] = value;
            }
            if (Fail || Value == "changed")
            {
                ((List<List<int>>)value)[0][0] = 2;
                context.TriggerTime = DateTime.UtcNow.AddDays(1);
                context.TriggerOnStartup = true;
                context.PendingStartupEvents.Add(Guid.NewGuid(), new Startup());
            }
        }
        public void ThrowIfRequested()
        {
            if (Fail) throw new InvalidOperationException("Expected execution failure.");
        }
    }

    private sealed class HandledMachineException(FailureProbe probe) : StateMachineExceptionHandler
    {
        public override bool OnStateEntryException(IStateActionContext context, Exception exception)
        {
            probe.Handled = true;
            return true;
        }
    }

    private sealed class ReferenceStorage : IStateflowsStorage
    {
        public StateflowsContext? State;
        public int Reads;
        public int Writes;
        public bool RejectReads;
        public void ResetCounts() => (Reads, Writes) = (0, 0);
        public Task<StateflowsContext> HydrateAsync(BehaviorId id)
        {
            Reads++;
            if (RejectReads) throw new InvalidOperationException("Expected hydration failure.");
            return Task.FromResult(State ?? new StateflowsContext(id));
        }
        public Task DehydrateAsync(StateflowsContext context)
        {
            Writes++;
            State = context;
            return Task.CompletedTask;
        }
        public Task<IEnumerable<StateflowsContext>> GetAllContextsAsync(IEnumerable<BehaviorClass> classes)
            => throw new NotSupportedException();
        public Task<IEnumerable<BehaviorId>> GetAllContextIdsAsync(IEnumerable<BehaviorClass> classes)
            => throw new NotSupportedException();
        public Task<IEnumerable<StateflowsContext>> GetTimeTriggeredContextsAsync(IEnumerable<BehaviorClass> classes)
            => throw new NotSupportedException();
        public Task<IEnumerable<StateflowsContext>> GetStartupTriggeredContextsAsync(IEnumerable<BehaviorClass> classes)
            => throw new NotSupportedException();
        public void Dispose() { }
    }

    private static int[] GetPorts()
    {
        var listeners = Enumerable.Range(0, 2).Select(_ => new TcpListener(IPAddress.Loopback, 0)).ToArray();
        try
        {
            foreach (var listener in listeners) listener.Start();
            return listeners.Select(l => ((IPEndPoint)l.LocalEndpoint).Port).ToArray();
        }
        finally
        {
            foreach (var listener in listeners) listener.Stop();
        }
    }

    // Uses the silo's Orleans storage serializer, so persisted state is subject to the same type allow-list as real providers.
    private sealed class FaultStorage : IGrainStorage
    {
        private readonly ConcurrentDictionary<(string, GrainId), BinaryData> states = new();
        private IGrainStorageSerializer serializer = null!;
        public bool RejectNextWrite;
        public bool RejectNextClear;
        public string? ContextSnapshot;
        public int ContextReads;

        public IGrainStorage Attach(IGrainStorageSerializer storageSerializer)
        {
            serializer = storageSerializer;
            return this;
        }

        public Task ReadStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
        {
            if (typeof(T) == typeof(BehaviorContextState)) ContextReads++;
            grainState.RecordExists = states.TryGetValue((stateName, grainId), out var serialized);
            grainState.State = serialized != null
                ? serializer.Deserialize<T>(serialized)
                : Activator.CreateInstance<T>();
            return Task.CompletedTask;
        }

        public Task WriteStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
        {
            if (grainState.State is BehaviorContextState && RejectNextWrite)
            {
                RejectNextWrite = false;
                throw new InvalidOperationException("Expected storage failure.");
            }
            states[(stateName, grainId)] = serializer.Serialize(grainState.State);
            if (grainState.State is BehaviorContextState context) ContextSnapshot = context.Payload;
            grainState.RecordExists = true;
            return Task.CompletedTask;
        }

        public Task ClearStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
        {
            if (typeof(T) == typeof(BehaviorContextState) && RejectNextClear)
            {
                RejectNextClear = false;
                throw new InvalidOperationException("Expected clear failure.");
            }
            states.TryRemove((stateName, grainId), out _);
            grainState.RecordExists = false;
            return Task.CompletedTask;
        }
    }
}
