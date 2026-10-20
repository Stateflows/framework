using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Storage;
using Stateflows;
using Stateflows.Common;
using Stateflows.Common.Interfaces;
using Stateflows.StateMachines;

namespace Orleans.IntegrationTests;

public sealed partial class BehaviorNotificationTests
{
    public sealed record DispatchTurn(bool Fail);
    public sealed record DeferredNotice(int Value);
    public sealed record HeaderProgress(int Value);

    [ImmediatePublish]
    public sealed record AttributeProgress(int Value);

    private sealed class DispatchProbe
    {
        public TaskCompletionSource Entered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource Release { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    private static IHost CreateDispatchHost(bool orleans, DispatchProbe probe, FaultingNotificationsStorage storage)
    {
        var builder = Host.CreateDefaultBuilder()
            .ConfigureLogging(logging => logging.SetMinimumLevel(LogLevel.Error))
            .ConfigureServices(services => services.AddStateflows(sf =>
            {
                if (!orleans)
                    sf.AddLegacyRuntime();
                sf.AddStateMachines(machines => machines.AddStateMachine("dispatch", machine => machine
                    .AddInitialState("idle", state => state.AddInternalTransition<DispatchTurn>(transition =>
                        transition.AddEffect(async c =>
                        {
                            c.Behavior.Publish(new DeferredNotice(1));
                            c.Behavior.Publish(new HeaderProgress(2), new Dictionary<string, EventHeader>
                            {
                                ["dispatch"] = new ImmediatePublish()
                            });
                            c.Behavior.Publish(new AttributeProgress(3));
                            probe.Entered.TrySetResult();
                            await probe.Release.Task;
                            if (c.Event.Fail)
                                throw new InvalidOperationException("Expected execution failure.");
                        })))));
            }));
        if (orleans)
        {
            var ports = GetFreeTcpPorts(2);
            var cluster = Guid.NewGuid().ToString("N");
            builder.UseOrleans(silo => silo.UseLocalhostClustering(ports[0], ports[1], null, cluster, cluster)
                .ConfigureServices(services => services.AddKeyedSingleton<IGrainStorage>("Default",
                    (provider, _) => storage.Attach(provider.GetRequiredService<IGrainStorageSerializer>())))
                .UseInMemoryReminderService().AddStateflowsRuntime());
        }
        return builder.Build();
    }

    [TestMethod]
    [DataRow(false, false)]
    [DataRow(false, true)]
    [DataRow(true, false)]
    [DataRow(true, true)]
    public async Task ImmediateNotificationsArriveDuringExecutionWhileDefaultsWaitForCommit(bool orleans, bool fail)
    {
        var probe = new DispatchProbe();
        var storage = new FaultingNotificationsStorage();
        using var host = CreateDispatchHost(orleans, probe, storage);
        await host.StartAsync();
        try
        {
            using var scope = host.Services.CreateScope();
            using var behavior = await LocateBehaviorAsync(scope.ServiceProvider.GetRequiredService<IBehaviorLocator>(),
                new BehaviorId(BehaviorType.StateMachine, "dispatch", Guid.NewGuid().ToString("N")), WatchTimeout);
            var header = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
            var attribute = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
            var deferred = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
            await behavior.WatchAsync<HeaderProgress>(n => header.TrySetResult(n.Value));
            await behavior.WatchAsync<AttributeProgress>(n => attribute.TrySetResult(n.Value));
            var committedAtDispatch = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            await behavior.WatchAsync<DeferredNotice>(async n =>
            {
                var committed = orleans
                    ? storage.HasContext(behavior.Id)
                    : (await scope.ServiceProvider.GetRequiredService<IStateflowsStorage>().HydrateAsync(behavior.Id)).Status
                        == BehaviorStatus.Initialized;
                committedAtDispatch.TrySetResult(committed);
                deferred.TrySetResult(n.Value);
            });

            var send = behavior.SendAsync(new DispatchTurn(fail));
            await probe.Entered.Task.WaitAsync(WatchTimeout);
            Assert.AreEqual(2, await header.Task.WaitAsync(WatchTimeout));
            Assert.AreEqual(3, await attribute.Task.WaitAsync(WatchTimeout));
            Assert.IsFalse(send.IsCompleted, "Progress must arrive while execution is still blocked.");
            Assert.IsFalse(deferred.Task.IsCompleted, "Default publication must wait for commit.");
            if (orleans)
                Assert.IsFalse(storage.HasContext(behavior.Id), "Immediate dispatch must precede publisher persistence.");

            probe.Release.TrySetResult();
            Assert.AreEqual(fail ? EventStatus.Failed : EventStatus.Consumed,
                (await send.WaitAsync(WatchTimeout)).Status);
            if (!fail)
            {
                Assert.AreEqual(1, await deferred.Task.WaitAsync(WatchTimeout));
                Assert.IsTrue(await committedAtDispatch.Task.WaitAsync(WatchTimeout),
                    "Default notifications must not be visible before durable publisher state.");
                if (orleans)
                    Assert.AreEqual(BehaviorStatus.Initialized, storage.GetContext(behavior.Id).Status);
            }

            Assert.AreEqual(fail ? 0 : 1,
                (await behavior.GetNotificationsAsync<DeferredNotice>(DateTime.MinValue)).Count());
            Assert.AreEqual(1, (await behavior.GetNotificationsAsync<HeaderProgress>(DateTime.MinValue)).Count());
            Assert.AreEqual(1, (await behavior.GetNotificationsAsync<AttributeProgress>(DateTime.MinValue)).Count());
            if (fail)
                Assert.IsFalse(deferred.Task.IsCompleted);
        }
        finally
        {
            probe.Release.TrySetResult();
            await host.StopAsync();
        }
    }

    [TestMethod]
    public async Task CommitFailureDiscardsDefaultsButCannotRetractImmediateNotifications()
    {
        var probe = new DispatchProbe();
        var storage = new FaultingNotificationsStorage();
        using var host = CreateDispatchHost(true, probe, storage);
        await host.StartAsync();
        try
        {
            using var scope = host.Services.CreateScope();
            using var behavior = await LocateBehaviorAsync(scope.ServiceProvider.GetRequiredService<IBehaviorLocator>(),
                new BehaviorId(BehaviorType.StateMachine, "dispatch", "commit-failure"), WatchTimeout);
            var send = behavior.SendAsync(new DispatchTurn(false));
            await probe.Entered.Task.WaitAsync(WatchTimeout);
            storage.FailNextContextWrite(false);
            probe.Release.TrySetResult();
            Assert.AreEqual(EventStatus.Failed, (await send.WaitAsync(WatchTimeout)).Status);
            Assert.IsFalse(storage.HasContext(behavior.Id));
            Assert.IsFalse((await behavior.GetNotificationsAsync<DeferredNotice>(DateTime.MinValue)).Any());
            Assert.AreEqual(1, (await behavior.GetNotificationsAsync<HeaderProgress>(DateTime.MinValue)).Count());
            Assert.AreEqual(1, (await behavior.GetNotificationsAsync<AttributeProgress>(DateTime.MinValue)).Count());
        }
        finally
        {
            probe.Release.TrySetResult();
            await host.StopAsync();
        }
    }
}
