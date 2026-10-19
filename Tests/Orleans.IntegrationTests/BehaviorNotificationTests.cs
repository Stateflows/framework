using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Hosting;
using Stateflows;
using Stateflows.Common;
using Stateflows.Common.Interfaces;
using Stateflows.StateMachines;

namespace Orleans.IntegrationTests;

[TestClass]
public sealed partial class BehaviorNotificationTests
{
    public sealed class PublishCustomNotification
    {
        public string Text { get; set; } = string.Empty;
    }

    public sealed class CustomNotification
    {
        public string Text { get; set; } = string.Empty;
    }

    public sealed class PublishAnotherNotification
    {
        public int Count { get; set; }
    }

    public sealed class AnotherNotification
    {
        public int Count { get; set; }
    }

    public sealed class PublishRetainedNotification
    {
        public int Value { get; set; }
    }

    [Retain]
    public sealed class RetainedCustomNotification
    {
        public int Value { get; set; }
    }

    public sealed class MoveNextEvent { }

    private static IHost CreateHost(int[] ports, IIncomingGrainCallFilter? filter = null)
    {
        var clusterId = $"orleans-notify-{Guid.NewGuid():N}";
        var serviceId = $"stateflows-notify-{Guid.NewGuid():N}";

        return Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
            {
                services.AddStateflows(stateflows => stateflows
                    .AddStateMachines(machines => machines.AddStateMachine(
                        "notifying-machine",
                        machine => machine
                            .AddInitialState("idle", state => state
                                .AddInternalTransition<PublishCustomNotification>(t => t
                                    .AddEffect(c => c.Behavior.Publish(new CustomNotification { Text = c.Event.Text })))
                                .AddInternalTransition<PublishAnotherNotification>(t => t
                                    .AddEffect(c =>
                                        c.Behavior.Publish(new AnotherNotification { Count = c.Event.Count })))
                                .AddInternalTransition<PublishRetainedNotification>(t => t
                                    .AddEffect(c => c.Behavior.PublishRetained(new RetainedCustomNotification
                                        { Value = c.Event.Value })))
                                .AddTransition<MoveNextEvent>("running"))
                            .AddState("running"))));
            })
            .ConfigureLogging(logging => logging.SetMinimumLevel(LogLevel.Warning))
            .UseOrleans(silo => silo
                .UseLocalhostClustering(ports[0], ports[1], null, serviceId, clusterId)
                .ConfigureServices(services =>
                {
                    if (filter != null) services.AddSingleton(filter);
                })
                .AddMemoryGrainStorage("stateflows")
                .UseInMemoryReminderService()
                .AddStateflowsRuntime())
            .Build();
    }

    private static async Task<IBehavior> LocateBehaviorAsync(IBehaviorLocator locator, BehaviorId behaviorId, TimeSpan timeout)
    {
        var sw = Stopwatch.StartNew();
        while (sw.Elapsed < timeout)
        {
            if (locator.TryLocateBehavior(behaviorId, out var behavior))
            {
                return behavior;
            }
            await Task.Delay(50);
        }

        Assert.Fail($"Behavior '{behaviorId}' could not be located within {timeout}.");
        return null!;
    }

    private static async Task<IStateMachineBehavior> LocateStateMachineAsync(IStateMachineLocator locator, StateMachineId machineId, TimeSpan timeout)
    {
        var sw = Stopwatch.StartNew();
        while (sw.Elapsed < timeout)
        {
            if (locator.TryLocateStateMachine(machineId, out var machine))
            {
                return machine;
            }
            await Task.Delay(50);
        }

        Assert.Fail($"State machine '{machineId}' could not be located within {timeout}.");
        return null!;
    }

    [TestMethod]
    public async Task CustomNotificationDeliveredViaTypedWatchAsync()
    {
        var ports = GetFreeTcpPorts(2);
        using var host = CreateHost(ports);
        await host.StartAsync();

        try
        {
            using var scope = host.Services.CreateScope();
            var locator = scope.ServiceProvider.GetRequiredService<IBehaviorLocator>();
            var behaviorId = new BehaviorId(BehaviorType.StateMachine, "notifying-machine", "typed-1");
            var behavior = await LocateBehaviorAsync(locator, behaviorId, TimeSpan.FromSeconds(5));

            var receivedTcs = new TaskCompletionSource<CustomNotification>(TaskCreationOptions.RunContinuationsAsynchronously);

            await using (var watcher = await behavior.WatchAsync<CustomNotification>(n => receivedTcs.TrySetResult(n)))
            {
                var sendResult = await behavior.SendAsync(new PublishCustomNotification { Text = "hello-orleans" });
                Assert.AreEqual(EventStatus.Consumed, sendResult.Status);

                var notification = await receivedTcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
                Assert.AreEqual("hello-orleans", notification.Text);
            }
        }
        finally
        {
            await host.StopAsync();
        }
    }

    [TestMethod]
    public async Task CustomNotificationDeliveredViaUntypedWatchAsync()
    {
        var ports = GetFreeTcpPorts(2);
        using var host = CreateHost(ports);
        await host.StartAsync();

        try
        {
            using var scope = host.Services.CreateScope();
            var locator = scope.ServiceProvider.GetRequiredService<IBehaviorLocator>();
            var behaviorId = new BehaviorId(BehaviorType.StateMachine, "notifying-machine", "untyped-1");
            var behavior = await LocateBehaviorAsync(locator, behaviorId, TimeSpan.FromSeconds(5));

            var receivedTcs = new TaskCompletionSource<EventHolder>(TaskCreationOptions.RunContinuationsAsynchronously);

            await using (var watcher = await behavior.WatchAsync([Event<CustomNotification>.Name], holder => receivedTcs.TrySetResult(holder)))
            {
                var sendResult = await behavior.SendAsync(new PublishCustomNotification { Text = "hello-holder" });
                Assert.AreEqual(EventStatus.Consumed, sendResult.Status);

                var holder = await receivedTcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
                Assert.AreEqual(Event<CustomNotification>.Name, holder.Name);
                var payload = ((EventHolder<CustomNotification>)holder).Payload;
                Assert.AreEqual("hello-holder", payload.Text);
            }
        }
        finally
        {
            await host.StopAsync();
        }
    }

    [TestMethod]
    public async Task RetainedNotificationReplayedToSubsequentWatcher()
    {
        var ports = GetFreeTcpPorts(2);
        using var host = CreateHost(ports);
        await host.StartAsync();

        try
        {
            using var scope = host.Services.CreateScope();
            var locator = scope.ServiceProvider.GetRequiredService<IBehaviorLocator>();
            var behaviorId = new BehaviorId(BehaviorType.StateMachine, "notifying-machine", "retained-1");
            var behavior = await LocateBehaviorAsync(locator, behaviorId, TimeSpan.FromSeconds(5));

            // Publish retained notification before any watcher is registered
            var sendResult = await behavior.SendAsync(new PublishRetainedNotification { Value = 42 });
            Assert.AreEqual(EventStatus.Consumed, sendResult.Status);

            // Small delay to allow async persistence of retained notification in SubscriptionsGrain
            await Task.Delay(200);

            var receivedTcs = new TaskCompletionSource<RetainedCustomNotification>(TaskCreationOptions.RunContinuationsAsynchronously);

            // Register watcher AFTER the notification was published
            await using (var watcher = await behavior.WatchAsync<RetainedCustomNotification>(n => receivedTcs.TrySetResult(n)))
            {
                var notification = await receivedTcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
                Assert.AreEqual(42, notification.Value);
            }
        }
        finally
        {
            await host.StopAsync();
        }
    }

    [TestMethod]
    public async Task UnwatchingStopsNotificationDelivery()
    {
        var ports = GetFreeTcpPorts(2);
        using var host = CreateHost(ports);
        await host.StartAsync();

        try
        {
            using var scope = host.Services.CreateScope();
            var locator = scope.ServiceProvider.GetRequiredService<IBehaviorLocator>();
            var behaviorId = new BehaviorId(BehaviorType.StateMachine, "notifying-machine", "unwatch-1");
            var behavior = await LocateBehaviorAsync(locator, behaviorId, TimeSpan.FromSeconds(5));

            var count = 0;
            var firstNotificationTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            var watcher = await behavior.WatchAsync<CustomNotification>(_ =>
            {
                Interlocked.Increment(ref count);
                firstNotificationTcs.TrySetResult(true);
            });

            await behavior.SendAsync(new PublishCustomNotification { Text = "first" });
            await firstNotificationTcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.AreEqual(1, System.Threading.Volatile.Read(ref count));

            // Dispose watcher to unwatch
            await watcher.DisposeAsync();

            // Send a second notification
            await behavior.SendAsync(new PublishCustomNotification { Text = "second" });
            await Task.Delay(500);

            // Count should remain 1 because watcher was unsubscribed
            Assert.AreEqual(1, System.Threading.Volatile.Read(ref count));
        }
        finally
        {
            await host.StopAsync();
        }
    }

    [TestMethod]
    public async Task MultipleWatchersReceiveRespectiveNotifications()
    {
        var ports = GetFreeTcpPorts(2);
        using var host = CreateHost(ports);
        await host.StartAsync();

        try
        {
            using var scope = host.Services.CreateScope();
            var locator = scope.ServiceProvider.GetRequiredService<IBehaviorLocator>();
            var behaviorId = new BehaviorId(BehaviorType.StateMachine, "notifying-machine", "multi-1");
            var behavior = await LocateBehaviorAsync(locator, behaviorId, TimeSpan.FromSeconds(5));

            var customTcs = new TaskCompletionSource<CustomNotification>(TaskCreationOptions.RunContinuationsAsynchronously);
            var anotherTcs = new TaskCompletionSource<AnotherNotification>(TaskCreationOptions.RunContinuationsAsynchronously);

            await using (var watcher1 = await behavior.WatchAsync<CustomNotification>(n => customTcs.TrySetResult(n)))
            await using (var watcher2 = await behavior.WatchAsync<AnotherNotification>(n => anotherTcs.TrySetResult(n)))
            {
                await behavior.SendAsync(new PublishCustomNotification { Text = "for-watcher-1" });
                await behavior.SendAsync(new PublishAnotherNotification { Count = 99 });

                var custom = await customTcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
                var another = await anotherTcs.Task.WaitAsync(TimeSpan.FromSeconds(5));

                Assert.AreEqual("for-watcher-1", custom.Text);
                Assert.AreEqual(99, another.Count);
            }
        }
        finally
        {
            await host.StopAsync();
        }
    }

    [TestMethod]
    public async Task NextAsyncAwaitsNextNotification()
    {
        var ports = GetFreeTcpPorts(2);
        using var host = CreateHost(ports);
        await host.StartAsync();

        try
        {
            using var scope = host.Services.CreateScope();
            var locator = scope.ServiceProvider.GetRequiredService<IBehaviorLocator>();
            var behaviorId = new BehaviorId(BehaviorType.StateMachine, "notifying-machine", "next-1");
            var behavior = await LocateBehaviorAsync(locator, behaviorId, TimeSpan.FromSeconds(5));

            var nextTask = behavior.NextAsync<CustomNotification>();

            await behavior.SendAsync(new PublishCustomNotification { Text = "from-next" });

            var notification = await nextTask.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.AreEqual("from-next", notification.Text);
        }
        finally
        {
            await host.StopAsync();
        }
    }

    [TestMethod]
    public async Task StateMachineStatusWatchedViaWatchStatusAsync()
    {
        var ports = GetFreeTcpPorts(2);
        using var host = CreateHost(ports);
        await host.StartAsync();

        try
        {
            using var scope = host.Services.CreateScope();
            var machineLocator = scope.ServiceProvider.GetRequiredService<IStateMachineLocator>();
            var machineId = new StateMachineId("notifying-machine", "status-1");
            var machine = await LocateStateMachineAsync(machineLocator, machineId, TimeSpan.FromSeconds(5));

            var statusTcs = new TaskCompletionSource<StateMachineInfo>(TaskCreationOptions.RunContinuationsAsynchronously);

            await using (var watcher = await machine.WatchStatusAsync(info =>
            {
                if (info.CurrentStates?.Value == "running")
                {
                    statusTcs.TrySetResult(info);
                }
            }))
            {
                await machine.SendAsync(new MoveNextEvent());

                var receivedInfo = await statusTcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
                Assert.AreEqual("running", receivedInfo.CurrentStates.Value);
                Assert.AreEqual(BehaviorStatus.Initialized, receivedInfo.BehaviorStatus);
            }
        }
        finally
        {
            await host.StopAsync();
        }
    }

    private static int[] GetFreeTcpPorts(int count)
    {
        var listeners = new TcpListener[count];
        var ports = new int[count];
        try
        {
            for (var i = 0; i < count; i++)
            {
                listeners[i] = new TcpListener(IPAddress.Loopback, 0);
                listeners[i].Start();
                ports[i] = ((IPEndPoint)listeners[i].LocalEndpoint).Port;
            }
        }
        finally
        {
            for (var i = 0; i < count; i++)
            {
                listeners[i]?.Stop();
            }
        }

        return ports;
    }
}
