using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Stateflows;
using Stateflows.Common;
using Stateflows.Common.Utilities;
using Stateflows.Interfaces;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace Orleans.IntegrationTests;

public sealed partial class BehaviorNotificationTests
{
    private static readonly TimeSpan WatchTimeout = TimeSpan.FromSeconds(5);

    [TestMethod]
    public async Task WatchRenewalRepeatsWithSameObserverAndStopsAfterUnwatch()
    {
        var calls = new WatchCallFilter();
        using var host = CreateHost(GetFreeTcpPorts(2), calls);
        await host.StartAsync();
        try
        {
            var id = new BehaviorId(BehaviorType.StateMachine, "notifying-machine", "renewal");
            var client = TrackingClusterClient.Create(host.Services.GetRequiredService<IClusterClient>());
            var tracker = (TrackingClusterClient)client;
            await using var behavior = new GrainBehavior("host", id,
                client, new WatchLogger(), TimeSpan.FromMilliseconds(50));
            var watcher = await behavior.WatchAsync<CustomNotification>(_ => Task.CompletedTask);
            var adds = new List<WatchCall>();
            for (var i = 0; i < 4; i++)
                adds.Add(await calls.Adds.Reader.ReadAsync().AsTask().WaitAsync(WatchTimeout));
            Assert.IsTrue(adds.All(a => a.Observer.Equals(adds[0].Observer)),
                "Every renewal must reuse the registered observer reference.");
            Assert.AreEqual(1, tracker.Created);

            await watcher.DisposeAsync();
            var removal = await calls.Removes.Reader.ReadAsync().AsTask().WaitAsync(WatchTimeout);
            Assert.AreEqual(adds[0].Observer, removal.Observer);
            Assert.AreEqual(1, tracker.Deleted);
            CollectionAssert.AreEquivalent(new[] { Event<CustomNotification>.Name }, removal.Names);
            var count = calls.AddCount;
            await Task.Delay(200);
            Assert.AreEqual(count, calls.AddCount, "Unwatch must stop renewal.");

            await using var replacement = await behavior.WatchAsync<CustomNotification>(_ => Task.CompletedTask);
            Assert.AreEqual(2, tracker.Created);
            while (calls.Adds.Reader.TryRead(out _)) { }
            await calls.Adds.Reader.ReadAsync().AsTask().WaitAsync(WatchTimeout);
        }
        finally
        {
            await host.StopAsync();
        }
    }

    [TestMethod]
    public async Task AsyncAndSynchronousHandlersDoNotBlockPublisherOrOtherHandlers()
    {
        using var host = CreateHost(GetFreeTcpPorts(2));
        await host.StartAsync();
        using var synchronousRelease = new ManualResetEventSlim();
        var asyncRelease = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        try
        {
            var id = new BehaviorId(BehaviorType.StateMachine, "notifying-machine", "nonblocking");
            await using var behavior = new GrainBehavior("host", id,
                host.Services.GetRequiredService<IClusterClient>(), new WatchLogger(), TimeSpan.FromMinutes(4));
            var synchronousEntered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var asyncEntered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var fastReceived = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            await using var synchronous = await behavior.WatchAsync<CustomNotification>(_ =>
            {
                synchronousEntered.TrySetResult();
                if (!synchronousRelease.Wait(WatchTimeout))
                    throw new TimeoutException("Test did not release the synchronous handler.");
                return Task.CompletedTask;
            });
            await using var asynchronous = await behavior.WatchAsync<CustomNotification>(async _ =>
            {
                asyncEntered.TrySetResult();
                await asyncRelease.Task;
            });
            await using var fast = await behavior.WatchAsync<CustomNotification>(_ =>
            {
                fastReceived.TrySetResult();
                return Task.CompletedTask;
            });
            var subscriptions = GetSubscriptions(host.Services, id);

            await subscriptions.PublishAsync([new CustomNotification().ToEventHolder(id)]).WaitAsync(WatchTimeout);
            await Task.WhenAll(synchronousEntered.Task, asyncEntered.Task, fastReceived.Task).WaitAsync(WatchTimeout);
            await subscriptions.PublishAsync([new AnotherNotification().ToEventHolder(id)]).WaitAsync(WatchTimeout);
        }
        finally
        {
            synchronousRelease.Set();
            asyncRelease.TrySetResult();
            await host.StopAsync();
        }
    }

    [TestMethod]
    public async Task HandlerFailuresAreLoggedAndDoNotInterruptOtherHandlers()
    {
        using var host = CreateHost(GetFreeTcpPorts(2));
        await host.StartAsync();
        try
        {
            var id = new BehaviorId(BehaviorType.StateMachine, "notifying-machine", "errors");
            var logger = new WatchLogger();
            await using var behavior = new GrainBehavior("host", id,
                host.Services.GetRequiredService<IClusterClient>(), logger, TimeSpan.FromMinutes(4));
            var received = Channel.CreateUnbounded<bool>();
            await using var synchronous = await behavior.WatchAsync<CustomNotification>(
                _ => throw new InvalidOperationException("Synchronous handler failure."));
            await using var asynchronous = await behavior.WatchAsync<CustomNotification>(async _ =>
            {
                await Task.Yield();
                throw new InvalidOperationException("Asynchronous handler failure.");
            });
            await using var healthy = await behavior.WatchAsync<CustomNotification>(_ =>
            {
                received.Writer.TryWrite(true);
                return Task.CompletedTask;
            });
            for (var i = 0; i < 2; i++)
            {
                await GetSubscriptions(host.Services, id).PublishAsync([new CustomNotification().ToEventHolder(id)]);
                await received.Reader.ReadAsync().AsTask().WaitAsync(WatchTimeout);
                for (var j = 0; j < 2; j++)
                    Assert.IsInstanceOfType<InvalidOperationException>(
                        await logger.Errors.Reader.ReadAsync().AsTask().WaitAsync(WatchTimeout));
            }
        }
        finally
        {
            await host.StopAsync();
        }
    }

    [TestMethod]
    public async Task DisposingBehaviorRemovesAllWatchesAndStopsCallbacksAndRenewal()
    {
        var calls = new WatchCallFilter();
        using var host = CreateHost(GetFreeTcpPorts(2), calls);
        await host.StartAsync();
        try
        {
            var id = new BehaviorId(BehaviorType.StateMachine, "notifying-machine", "dispose");
            var client = TrackingClusterClient.Create(host.Services.GetRequiredService<IClusterClient>());
            var tracker = (TrackingClusterClient)client;
            var behavior = new GrainBehavior("host", id, client,
                new WatchLogger(), TimeSpan.FromMilliseconds(50));
            var deliveries = 0;
            await using var watcher = await behavior.WatchAsync<CustomNotification>(_ =>
            {
                Interlocked.Increment(ref deliveries);
                return Task.CompletedTask;
            });
            var add = await calls.Adds.Reader.ReadAsync().AsTask().WaitAsync(WatchTimeout);

            behavior.Dispose();
            await behavior.DisposeAsync();
            await behavior.DisposeAsync();
            var remove = await calls.Removes.Reader.ReadAsync().AsTask().WaitAsync(WatchTimeout);
            Assert.AreEqual(add.Observer, remove.Observer);
            Assert.AreEqual(1, tracker.Created);
            Assert.AreEqual(1, tracker.Deleted);
            Assert.IsNull(remove.Names, "Behavior disposal must remove all remote watches.");
            var count = calls.AddCount;
            await behavior.NotifyAsync([new CustomNotification().ToEventHolder(id)]);
            await Task.Delay(200);
            Assert.AreEqual(0, deliveries);
            Assert.AreEqual(count, calls.AddCount);
            await Assert.ThrowsExactlyAsync<ObjectDisposedException>(
                () => behavior.WatchAsync<CustomNotification>(_ => Task.CompletedTask));
        }
        finally
        {
            await host.StopAsync();
        }
    }

    [TestMethod]
    public async Task DisposalFailureIsReportedAndStillDeletesLocalObserverReference()
    {
        var calls = new WatchCallFilter { RejectRemoves = 1 };
        using var host = CreateHost(GetFreeTcpPorts(2), calls);
        await host.StartAsync();
        try
        {
            var id = new BehaviorId(BehaviorType.StateMachine, "notifying-machine", "cleanup-failure");
            var client = TrackingClusterClient.Create(host.Services.GetRequiredService<IClusterClient>());
            var tracker = (TrackingClusterClient)client;
            var logger = new WatchLogger();
            var behavior = new GrainBehavior("host", id, client, logger, TimeSpan.FromMinutes(4));
            await using var watcher = await behavior.WatchAsync<CustomNotification>(_ => Task.CompletedTask);

            await Assert.ThrowsAsync<Exception>(() => behavior.DisposeAsync().AsTask());
            await logger.Errors.Reader.ReadAsync().AsTask().WaitAsync(WatchTimeout);
            Assert.AreEqual(1, tracker.Deleted, "Remote cleanup failure must not leak the local observer reference.");
        }
        finally
        {
            await host.StopAsync();
        }
    }

    [TestMethod]
    public async Task FailedRegistrationRollsBackLocalHandlersAndCanBeRetried()
    {
        var calls = new WatchCallFilter { RejectAdds = 1 };
        using var host = CreateHost(GetFreeTcpPorts(2), calls);
        await host.StartAsync();
        try
        {
            var id = new BehaviorId(BehaviorType.StateMachine, "notifying-machine", "registration-failure");
            await using var behavior = new GrainBehavior("host", id, host.Services.GetRequiredService<IClusterClient>(),
                new WatchLogger(), TimeSpan.FromMinutes(4));
            var unexpected = 0;
            await Assert.ThrowsAsync<Exception>(() => behavior.WatchAsync<CustomNotification>(_ =>
            {
                Interlocked.Increment(ref unexpected);
                return Task.CompletedTask;
            }));
            var received = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            await using var watcher = await behavior.WatchAsync<CustomNotification>(_ =>
            {
                received.TrySetResult();
                return Task.CompletedTask;
            });
            await GetSubscriptions(host.Services, id).PublishAsync([new CustomNotification().ToEventHolder(id)]);
            await received.Task.WaitAsync(WatchTimeout);
            await Task.Delay(100);
            Assert.AreEqual(0, unexpected);
        }
        finally
        {
            await host.StopAsync();
        }
    }

    [TestMethod]
    public async Task RenewalFailureIsLoggedAndNextTickRetries()
    {
        var calls = new WatchCallFilter();
        using var host = CreateHost(GetFreeTcpPorts(2), calls);
        await host.StartAsync();
        try
        {
            var id = new BehaviorId(BehaviorType.StateMachine, "notifying-machine", "renewal-failure");
            var logger = new WatchLogger();
            await using var behavior = new GrainBehavior("host", id, host.Services.GetRequiredService<IClusterClient>(),
                logger, TimeSpan.FromMilliseconds(50));
            await using var watcher = await behavior.WatchAsync<CustomNotification>(_ => Task.CompletedTask);
            var initial = await calls.Adds.Reader.ReadAsync().AsTask().WaitAsync(WatchTimeout);
            Interlocked.Exchange(ref calls.RejectAdds, 1);
            await logger.Errors.Reader.ReadAsync().AsTask().WaitAsync(WatchTimeout);
            var failed = await calls.Adds.Reader.ReadAsync().AsTask().WaitAsync(WatchTimeout);
            var retried = await calls.Adds.Reader.ReadAsync().AsTask().WaitAsync(WatchTimeout);
            Assert.AreEqual(initial.Observer, failed.Observer);
            Assert.AreEqual(initial.Observer, retried.Observer);
        }
        finally
        {
            await host.StopAsync();
        }
    }

    [TestMethod]
    public async Task DisposingDuringRegistrationDoesNotLeaveRemoteWatch()
    {
        var calls = new WatchCallFilter
        {
            AddEntered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously),
            ContinueAdd = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously)
        };
        using var host = CreateHost(GetFreeTcpPorts(2), calls);
        await host.StartAsync();
        try
        {
            var id = new BehaviorId(BehaviorType.StateMachine, "notifying-machine", "dispose-race");
            var behavior = new GrainBehavior("host", id, host.Services.GetRequiredService<IClusterClient>(),
                new WatchLogger(), TimeSpan.FromMilliseconds(50));
            var registration = behavior.WatchAsync<CustomNotification>(_ => Task.CompletedTask);
            await calls.AddEntered.Task.WaitAsync(WatchTimeout);
            behavior.Dispose();
            calls.ContinueAdd.TrySetResult();
            await Assert.ThrowsExactlyAsync<ObjectDisposedException>(() => registration);
            await behavior.DisposeAsync();
            var removal = await calls.Removes.Reader.ReadAsync().AsTask().WaitAsync(WatchTimeout);
            Assert.IsNull(removal.Names, "Disposal racing registration must remove the entire observer.");
            Assert.AreEqual(1, calls.AddCount);
        }
        finally
        {
            calls.ContinueAdd.TrySetResult();
            await host.StopAsync();
        }
    }

    [TestMethod]
    public async Task HandlerCanDisposeItsOwnBehaviorWithoutDeadlocking()
    {
        using var host = CreateHost(GetFreeTcpPorts(2));
        await host.StartAsync();
        try
        {
            var id = new BehaviorId(BehaviorType.StateMachine, "notifying-machine", "self-disposal");
            await using var behavior = new GrainBehavior("host", id, host.Services.GetRequiredService<IClusterClient>(),
                new WatchLogger(), TimeSpan.FromMinutes(4));
            var completed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            await using var watcher = await behavior.WatchAsync<CustomNotification>(async _ =>
            {
                await behavior.DisposeAsync();
                completed.TrySetResult();
            });
            await GetSubscriptions(host.Services, id).PublishAsync([new CustomNotification().ToEventHolder(id)])
                .WaitAsync(WatchTimeout);
            await completed.Task.WaitAsync(WatchTimeout);
        }
        finally
        {
            await host.StopAsync();
        }
    }

    [TestMethod]
    public async Task SharedUntypedDelegateHasIndependentWatcherRegistrations()
    {
        var calls = new WatchCallFilter();
        using var host = CreateHost(GetFreeTcpPorts(2), calls);
        await host.StartAsync();
        try
        {
            var id = new BehaviorId(BehaviorType.StateMachine, "notifying-machine", "shared-handler");
            await using var behavior = new GrainBehavior("host", id, host.Services.GetRequiredService<IClusterClient>(),
                new WatchLogger(), TimeSpan.FromMinutes(4));
            var received = Channel.CreateUnbounded<string>();
            Func<EventHolder, Task> handler = notification =>
            {
                received.Writer.TryWrite(notification.Name);
                return Task.CompletedTask;
            };
            var first = await behavior.WatchAsync([Event<CustomNotification>.Name, Event<AnotherNotification>.Name], handler);
            await using var second = await behavior.WatchAsync([Event<CustomNotification>.Name], handler);
            await first.DisposeAsync();
            var remove = await calls.Removes.Reader.ReadAsync().AsTask().WaitAsync(WatchTimeout);
            CollectionAssert.AreEquivalent(new[] { Event<AnotherNotification>.Name }, remove.Names);

            await GetSubscriptions(host.Services, id).PublishAsync([new CustomNotification().ToEventHolder(id)]);
            Assert.AreEqual(Event<CustomNotification>.Name,
                await received.Reader.ReadAsync().AsTask().WaitAsync(WatchTimeout));
        }
        finally
        {
            await host.StopAsync();
        }
    }

    private static ISubscriptionsGrain GetSubscriptions(IServiceProvider services, BehaviorId id)
        => services.GetRequiredService<IGrainFactory>().GetGrain<ISubscriptionsGrain>(
            StateflowsGrainKeys.ForBehavior("host", id));

    private sealed record WatchCall(IBehaviorGrainObserver Observer, string[]? Names);

    private sealed class WatchCallFilter : IIncomingGrainCallFilter
    {
        public Channel<WatchCall> Adds { get; } = Channel.CreateUnbounded<WatchCall>();
        public Channel<WatchCall> Removes { get; } = Channel.CreateUnbounded<WatchCall>();
        public int AddCount;
        public int RejectAdds;
        public int RejectRemoves;
        public TaskCompletionSource? AddEntered;
        public TaskCompletionSource? ContinueAdd;

        public async Task Invoke(IIncomingGrainCallContext context)
        {
            if (context.Grain is ISubscriptionsGrain)
            {
                var method = context.InterfaceMethod.Name;
                if (method is nameof(ISubscriptionsGrain.AddWatchAsync) or nameof(ISubscriptionsGrain.RemoveWatchAsync))
                {
                    var call = new WatchCall((IBehaviorGrainObserver)context.Request.GetArgument(0)!,
                        (string[]?)context.Request.GetArgument(1));
                    if (method == nameof(ISubscriptionsGrain.AddWatchAsync))
                    {
                        Interlocked.Increment(ref AddCount);
                        Adds.Writer.TryWrite(call);
                        AddEntered?.TrySetResult();
                        if (ContinueAdd != null) await ContinueAdd.Task;
                        if (Interlocked.Exchange(ref RejectAdds, 0) > 0)
                            throw new InvalidOperationException("Expected watch registration/renewal failure.");
                    }
                    else
                    {
                        Removes.Writer.TryWrite(call);
                        if (Interlocked.Exchange(ref RejectRemoves, 0) > 0)
                            throw new InvalidOperationException("Expected watch removal failure.");
                    }
                }
            }
            await context.Invoke();
        }
    }

    private sealed class WatchLogger : ILogger<GrainBehavior>
    {
        public Channel<Exception> Errors { get; } = Channel.CreateUnbounded<Exception>();
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, Microsoft.Extensions.Logging.EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (exception != null) Errors.Writer.TryWrite(exception);
        }
    }

    public class TrackingClusterClient : DispatchProxy
    {
        private IClusterClient target = null!;
        public int Created;
        public int Deleted;

        public static IClusterClient Create(IClusterClient target)
        {
            var client = Create<IClusterClient, TrackingClusterClient>();
            ((TrackingClusterClient)client).target = target;
            return client;
        }

        protected override object? Invoke(MethodInfo? method, object?[]? args)
        {
            try
            {
                var result = method!.Invoke(target, args);
                if (method.Name == "CreateObjectReference") Interlocked.Increment(ref Created);
                if (method.Name == "DeleteObjectReference") Interlocked.Increment(ref Deleted);
                return result;
            }
            catch (TargetInvocationException exception) when (exception.InnerException != null)
            {
                ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
                throw;
            }
        }
    }
}
