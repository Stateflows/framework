using System.Net;
using System.Net.Sockets;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Stateflows;
using Stateflows.Common;
using Stateflows.Common.Interfaces;
using Stateflows.Events;
using Stateflows.Extensions.MinimalAPIs;
using Stateflows.StateMachines;

namespace Orleans.IntegrationTests;

public sealed record PublishWatchedNotifications(int Count);
public sealed record RequestWatchedNotifications(int Count) : IRequest<WatchedNotificationsReply>;
public sealed record WatchedNotificationsReply(int Count);
public sealed record WatchedNotification(int Index);

[TestClass]
public sealed class NotificationWatchDeliveryTests
{
    [TestMethod]
    [DataRow(false, false)]
    [DataRow(false, true)]
    [DataRow(true, false)]
    [DataRow(true, true)]
    [Timeout(60_000)]
    public async Task DeliveryBarrierAwaitsSseFlushAttemptWithoutRetry(bool orleans, bool fail)
    {
        await using var app = CreateApp(orleans);
        await app.StartAsync();
        using var output = new GatedFlushStream(fail);
        try
        {
            using var behavior = app.Services.GetRequiredService<IBehaviorFactory>()
                .CreateBehavior(new StateMachineId("watch-delivery", "flush"));
            var context = new DefaultHttpContext();
            context.Response.Body = output;
            await using var watcher = await behavior.WatchAsync(
                [Event<WatchedNotification>.Name], notification => context.WriteEventAsync(notification));
            Assert.AreEqual(EventStatus.Consumed, (await behavior.SendAsync(new PublishWatchedNotifications(1))).Status);
            await output.Entered.Task.WaitAsync(TimeSpan.FromSeconds(10));
            var barrier = watcher.WaitForDeliveryAsync();
            Assert.IsFalse(barrier.IsCompleted, "SSE delivery must wait for the flush attempt, not just enqueue the frame.");
            output.Release.TrySetResult();
            await barrier.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.AreEqual(1, output.Attempts, "Failed SSE writes must not be retried.");
        }
        finally
        {
            output.Release.TrySetResult();
            await app.StopAsync();
        }
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    [Timeout(60_000)]
    public async Task BarrierWaitsForAsyncCallbacksWithoutBlockingPublisherOrIndependentWatch(bool orleans)
    {
        await using var app = CreateApp(orleans);
        await app.StartAsync();
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        try
        {
            using var behavior = app.Services.GetRequiredService<IBehaviorFactory>()
                .CreateBehavior(new StateMachineId("watch-delivery", "barrier"));
            var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var consumed = new List<int>();
            await using var slow = await behavior.WatchAsync<WatchedNotification>(async notification =>
            {
                entered.TrySetResult();
                await release.Task;
                await Task.Yield();
                consumed.Add(notification.Index);
            });
            var fastCount = 0;
            await using var fast = await behavior.WatchAsync<WatchedNotification>(_ =>
                Interlocked.Increment(ref fastCount));

            Assert.AreEqual(EventStatus.Consumed,
                (await behavior.SendAsync(new PublishWatchedNotifications(8)).WaitAsync(TimeSpan.FromSeconds(10))).Status);
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(10));
            await fast.WaitForDeliveryAsync().WaitAsync(TimeSpan.FromSeconds(10));
            Assert.AreEqual(8, fastCount, "An independent watch must not wait for a slow watch.");
            var barrier = slow.WaitForDeliveryAsync();
            Assert.IsFalse(barrier.IsCompleted, "Dispatch or observer acknowledgment alone must not satisfy the barrier.");
            release.TrySetResult();
            await barrier.WaitAsync(TimeSpan.FromSeconds(10));
            CollectionAssert.AreEqual(Enumerable.Range(0, 8).ToArray(), consumed.ToArray());
        }
        finally
        {
            release.TrySetResult();
            await app.StopAsync();
        }
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    [Timeout(60_000)]
    public async Task CallbackCanSendBackToPublisherAndFailureIsNotRetried(bool orleans)
    {
        await using var app = CreateApp(orleans);
        await app.StartAsync();
        try
        {
            using var behavior = app.Services.GetRequiredService<IBehaviorFactory>()
                .CreateBehavior(new StateMachineId("watch-delivery", "callback"));
            var attempts = 0;
            var returned = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            await using var watcher = await behavior.WatchAsync<WatchedNotification>(async _ =>
            {
                Interlocked.Increment(ref attempts);
                var result = await behavior.GetStatusAsync();
                Assert.AreEqual(EventStatus.Consumed, result.Status);
                returned.TrySetResult();
                throw new InvalidOperationException("Best-effort callback failure.");
            });
            Assert.AreEqual(EventStatus.Consumed, (await behavior.SendAsync(new PublishWatchedNotifications(1))).Status);
            await watcher.WaitForDeliveryAsync().WaitAsync(TimeSpan.FromSeconds(10));
            await returned.Task.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.AreEqual(1, attempts);
            await watcher.WaitForDeliveryAsync().WaitAsync(TimeSpan.FromSeconds(10));
            Assert.AreEqual(1, attempts, "A delivery barrier must not retry a failed callback.");
        }
        finally
        {
            await app.StopAsync();
        }
    }

    [TestMethod]
    [DataRow(false, false, false)]
    [DataRow(false, true, false)]
    [DataRow(true, false, false)]
    [DataRow(true, true, false)]
    [DataRow(false, false, true)]
    [DataRow(false, true, true)]
    [DataRow(true, false, true)]
    [DataRow(true, true, true)]
    [Timeout(60_000)]
    public async Task JsonResponseContainsEveryNotificationFromSendOrRequest(bool orleans, bool request, bool structural)
    {
        await using var app = CreateApp(orleans);
        await app.StartAsync();
        try
        {
            using var http = new HttpClient { BaseAddress = new Uri(app.Urls.Single()) };
            for (var iteration = 0; iteration < 3; iteration++)
            {
                var endpoint = structural
                    ? request ? "watchedQuery" : "watchedEvent"
                    : request ? "requestWatchedNotifications" : "publishWatchedNotifications";
                using var response = await http.PostAsJsonAsync(
                    $"/stateflows/stateMachines/watch-delivery/json-{iteration}/{endpoint}",
                    new
                    {
                        @event = new { count = 64 },
                        requestedNotifications = new[] { Event<WatchedNotification>.Name }
                    });
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());
                using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                var indices = body.RootElement.GetProperty("notifications").EnumerateArray()
                    .Select(notification => notification.GetProperty("payload").GetProperty("index").GetInt32()).ToArray();
                CollectionAssert.AreEqual(Enumerable.Range(0, 64).ToArray(), indices);
            }
        }
        finally
        {
            await app.StopAsync();
        }
    }

    [TestMethod]
    [DataRow(false, false, false)]
    [DataRow(false, true, false)]
    [DataRow(true, false, false)]
    [DataRow(true, true, false)]
    [DataRow(false, false, true)]
    [DataRow(false, true, true)]
    [DataRow(true, false, true)]
    [DataRow(true, true, true)]
    [Timeout(60_000)]
    public async Task SseFramesContainEveryNotificationInOrderForSendOrRequest(bool orleans, bool request, bool structural)
    {
        await using var app = CreateApp(orleans);
        await app.StartAsync();
        try
        {
            using var http = new HttpClient { BaseAddress = new Uri(app.Urls.Single()) };
            using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            var endpoint = structural
                ? request ? "watchedQuery" : "watchedEvent"
                : request ? "requestWatchedNotifications" : "publishWatchedNotifications";
            using var message = new HttpRequestMessage(HttpMethod.Post,
                $"/stateflows/stateMachines/watch-delivery/sse/{endpoint}?stream=true")
            {
                Content = JsonContent.Create(new
                {
                    @event = new { count = 64 },
                    requestedNotifications = new[] { Event<WatchedNotification>.Name }
                })
            };
            using var response = await http.SendAsync(message, HttpCompletionOption.ResponseHeadersRead, cancellation.Token);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("text/event-stream", response.Content.Headers.ContentType?.MediaType);
            using var reader = new StreamReader(await response.Content.ReadAsStreamAsync(cancellation.Token));
            var received = new List<int>();
            while (received.Count < 64)
            {
                var eventLine = await reader.ReadLineAsync(cancellation.Token);
                Assert.AreEqual($"event: {Event<WatchedNotification>.Name}", eventLine);
                var dataLine = await reader.ReadLineAsync(cancellation.Token);
                Assert.IsNotNull(dataLine);
                StringAssert.StartsWith(dataLine, "data: ");
                using var body = JsonDocument.Parse(dataLine[6..]);
                received.Add(body.RootElement.GetProperty("payload").GetProperty("index").GetInt32());
                Assert.AreEqual("", await reader.ReadLineAsync(cancellation.Token));
            }
            CollectionAssert.AreEqual(Enumerable.Range(0, 64).ToArray(), received.ToArray());
            cancellation.Cancel();
        }
        finally
        {
            await app.StopAsync();
        }
    }

    private static WebApplication CreateApp(bool orleans)
    {
        var listeners = Enumerable.Range(0, 3).Select(_ => new TcpListener(IPAddress.Loopback, 0)).ToArray();
        int[] ports;
        try
        {
            foreach (var listener in listeners) listener.Start();
            ports = listeners.Select(listener => ((IPEndPoint)listener.LocalEndpoint).Port).ToArray();
        }
        finally
        {
            foreach (var listener in listeners) listener.Stop();
        }
        var builder = WebApplication.CreateBuilder();
        builder.Logging.SetMinimumLevel(LogLevel.Error);
        builder.WebHost.UseUrls($"http://127.0.0.1:{ports[0]}");
        builder.Services.AddStateflows(sf =>
        {
            if (!orleans)
                sf.AddLegacyRuntime();
            sf.UseFullNamesFor(TypedElements.None)
            .AddEvents(events => events
                .AddEvent("watchedEvent", ev => ev.AddField("count", FieldType.Int32))
                .AddRequest<WatchedNotificationsReply>("watchedQuery", ev => ev.AddField("count", FieldType.Int32)))
            .AddStateMachines(machines => machines.AddStateMachine("watch-delivery", machine => machine
                .AddInitialState("ready", state => state
                    .AddInternalTransition("watchedEvent", transition => transition.AddEffect(context =>
                    {
                        for (var index = 0; index < context.Event.Get<int>("count"); index++)
                            context.Behavior.Publish(new WatchedNotification(index));
                    }))
                    .AddInternalTransition("watchedQuery", transition => transition.AddEffect(context =>
                    {
                        for (var index = 0; index < context.Event.Get<int>("count"); index++)
                            context.Behavior.Publish(new WatchedNotification(index));
                        context.Event.Respond(new WatchedNotificationsReply(context.Event.Get<int>("count")));
                    }))
                    .AddInternalTransition<PublishWatchedNotifications>(transition => transition.AddEffect(context =>
                    {
                        for (var index = 0; index < context.Event.Count; index++)
                            context.Behavior.Publish(new WatchedNotification(index));
                    }))
                    .AddInternalTransition<RequestWatchedNotifications>(transition => transition.AddEffect(context =>
                    {
                        for (var index = 0; index < context.Event.Count; index++)
                            context.Behavior.Publish(new WatchedNotification(index));
                        context.Event.Respond(new WatchedNotificationsReply(context.Event.Count));
                    })))));
        });
        if (orleans)
        {
            var cluster = Guid.NewGuid().ToString("N");
            builder.Host.UseOrleans(silo => silo.UseLocalhostClustering(ports[1], ports[2], null, cluster, cluster)
                .AddMemoryGrainStorageAsDefault().UseInMemoryReminderService().AddStateflowsRuntime());
        }
        var app = builder.Build();
        app.MapStateflowsMinimalAPIsEndpoints();
        return app;
    }

    private sealed class GatedFlushStream(bool fail) : MemoryStream
    {
        public TaskCompletionSource Entered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource Release { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public int Attempts;

        public override async Task FlushAsync(CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref Attempts);
            Entered.TrySetResult();
            await Release.Task.WaitAsync(cancellationToken);
            if (fail)
                throw new IOException("Simulated disconnected SSE client.");
            await base.FlushAsync(cancellationToken);
        }
    }
}
