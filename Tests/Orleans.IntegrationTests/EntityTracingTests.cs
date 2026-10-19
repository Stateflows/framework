using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OpenTelemetry;
using OpenTelemetry.Trace;
using Stateflows;
using Stateflows.Common;
using Stateflows.Entities;
using Stateflows.Entities.Attributes;
using Stateflows.Extensions.OpenTelemetry.Headers;

namespace Orleans.IntegrationTests;

[TestClass]
public sealed class EntityTracingTests
{
    [TestMethod]
    public async Task EntityOperationsAreInternalChildrenOfTheEventSpan()
    {
        var instance = Guid.NewGuid().ToString("N");
        var spans = new ConcurrentQueue<Activity>();
        using var host = CreateHost(instance, spans);
        await host.StartAsync();
        try
        {
            using var scope = host.Services.CreateScope();
            using var entity = Locate(scope.ServiceProvider, "traced-entity", instance);
            Assert.AreEqual(EventStatus.Consumed, (await entity.SendAsync(new TracedMutation { Amount = 3 })).Status);

            var root = spans.Single(span => span.DisplayName.StartsWith("Entity '"));
            StringAssert.StartsWith(root.DisplayName,
                $"Entity 'traced-entity:{instance}' processing '{Event<TracedMutation>.Name.ToShortName()}'");
            Assert.AreEqual(Event<TracedMutation>.Name.ToShortName(), root.GetTagItem("Event")?.ToString());
            Assert.AreEqual(nameof(EventStatus.Consumed), root.GetTagItem("EventStatus")?.ToString());
            Assert.AreNotEqual(ActivityStatusCode.Error, root.Status);
            var operations = spans.Where(span => span != root).ToArray();
            Assert.IsTrue(operations.Any(span => span.DisplayName == $"Mutation '{Event<TracedMutation>.Name.ToShortName()}'"));
            Assert.IsTrue(operations.Any(span => span.DisplayName == "Computed field 'Computed'"));
            Assert.IsTrue(operations.Any(span => span.DisplayName == $"Projection '{Event<TracedView>.Name.ToShortName()}'"));
            foreach (var span in spans)
            {
                Assert.AreEqual("Stateflows", span.Source.Name);
                Assert.AreEqual(ActivityKind.Internal, span.Kind);
                Assert.IsTrue(span.Duration > TimeSpan.Zero, "Every asynchronous scope must have ended.");
                Assert.IsTrue(span.GetTagItem("EntityId")?.ToString()?.Contains(instance) == true);
                if (span == root) continue;
                Assert.AreEqual(root.TraceId, span.TraceId);
                Assert.AreEqual(root.SpanId, span.ParentSpanId);
                Assert.IsTrue(new[] { "Mutation", "Field", "Projection" }.Any(tag => span.GetTagItem(tag) != null));
            }
        }
        finally
        {
            await host.StopAsync();
        }
    }

    [TestMethod]
    [DataRow(-1, "Mutation")]
    [DataRow(-2, "Computed field 'Computed'")]
    [DataRow(-3, "Projection")]
    public async Task FailedOperationsRecordExceptionsAndRollBackEvenWhenALaterHandlerThrows(int amount, string operation)
    {
        var instance = Guid.NewGuid().ToString("N");
        var spans = new ConcurrentQueue<Activity>();
        using var host = CreateHost(instance, spans);
        await host.StartAsync();
        try
        {
            using var scope = host.Services.CreateScope();
            using var entity = Locate(scope.ServiceProvider, "traced-entity", instance);
            Assert.AreEqual(EventStatus.Consumed, (await entity.SendAsync(new TracedMutation { Amount = 3 })).Status);
            spans.Clear();
            using var caller = new Activity("caller").SetIdFormat(ActivityIdFormat.W3C).Start();
            caller.ActivityTraceFlags = ActivityTraceFlags.Recorded;
            Assert.AreEqual(EventStatus.Failed, (await entity.SendAsync(new TracedMutation { Amount = amount })).Status);
            Assert.AreEqual(ActivityStatusCode.Unset, caller.Status, "Entity failures must not mark the caller's activity.");
            Assert.IsFalse(caller.Events.Any(e => e.Name == "exception"));
            var root = spans.Single(span => span.DisplayName.StartsWith("Entity '"));
            Assert.AreEqual(ActivityStatusCode.Error, root.Status);
            Assert.AreEqual(nameof(EventStatus.Failed), root.GetTagItem("EventStatus")?.ToString());
            var failedOperation = operation switch
            {
                "Mutation" => $"Mutation '{Event<TracedMutation>.Name.ToShortName()}'",
                "Projection" => $"Projection '{Event<TracedView>.Name.ToShortName()}'",
                _ => operation,
            };
            var failed = spans.Single(span => span.DisplayName == failedOperation);
            Assert.AreEqual(ActivityStatusCode.Error, failed.Status);
            Assert.IsTrue(failed.Events.Any(e => e.Name == "exception"));
            Assert.AreEqual(root.SpanId, failed.ParentSpanId);
            Assert.IsTrue(spans.All(span => span.Duration > TimeSpan.Zero));
            var result = await entity.RequestAsync(new ProjectionRequest<TracedView>(), NoTracingHeaders());
            Assert.AreEqual(3, result.Response!.Value);
        }
        finally
        {
            await host.StopAsync();
        }
    }

    [TestMethod]
    public async Task FailedInitializationMarksTheEventSpan()
    {
        var instance = Guid.NewGuid().ToString("N");
        var spans = new ConcurrentQueue<Activity>();
        using var host = CreateHost(instance, spans);
        await host.StartAsync();
        try
        {
            using var scope = host.Services.CreateScope();
            using var entity = Locate(scope.ServiceProvider, "failing-initialization", instance);
            Assert.AreEqual(EventStatus.Failed, (await entity.SendAsync(new Initialize())).Status);
            var root = spans.Single(span => span.DisplayName.StartsWith("Entity '"));
            Assert.AreEqual(ActivityStatusCode.Error, root.Status);
            Assert.AreEqual(nameof(EventStatus.Failed), root.GetTagItem("EventStatus")?.ToString());
            Assert.IsTrue(root.Events.Any(e => e.Name == "exception"));
            Assert.IsTrue(root.Duration > TimeSpan.Zero);
        }
        finally
        {
            await host.StopAsync();
        }
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public async Task AmbientParentSamplingControlsRootAndOperations(bool recorded)
    {
        var instance = Guid.NewGuid().ToString("N");
        var spans = new ConcurrentQueue<Activity>();
        using var host = CreateHost(instance, spans);
        await host.StartAsync();
        try
        {
            using var scope = host.Services.CreateScope();
            using var entity = Locate(scope.ServiceProvider, "traced-entity", instance);
            using var parent = new Activity("incoming").SetIdFormat(ActivityIdFormat.W3C).Start();
            parent.ActivityTraceFlags = recorded ? ActivityTraceFlags.Recorded : ActivityTraceFlags.None;
            Assert.AreEqual(EventStatus.Consumed, (await entity.SendAsync(new TracedMutation { Amount = 3 })).Status);
            if (!recorded)
            {
                Assert.AreEqual(0, spans.Count, "Operations must not become new roots when the event span is sampled out.");
                return;
            }
            var root = spans.Single(span => span.DisplayName.StartsWith("Entity '"));
            Assert.AreEqual(parent.TraceId, root.TraceId);
            Assert.AreEqual(parent.SpanId, root.ParentSpanId);
            Assert.IsTrue(spans.Count > 1);
        }
        finally
        {
            await host.StopAsync();
        }
    }

    [TestMethod]
    [DataRow(true, null)]
    [DataRow(false, null)]
    [DataRow(true, false)]
    [DataRow(false, true)]
    public async Task SerializedRemoteParentSamplingControlsRootAndOperations(bool recorded, bool? ambientRecorded)
    {
        var instance = Guid.NewGuid().ToString("N");
        var spans = new ConcurrentQueue<Activity>();
        using var host = CreateHost(instance, spans);
        await host.StartAsync();
        var ambient = Activity.Current;
        try
        {
            Activity.Current = null;
            using var caller = ambientRecorded.HasValue
                ? new Activity("conflicting ambient parent").SetIdFormat(ActivityIdFormat.W3C).Start()
                : null;
            if (caller != null)
                caller.ActivityTraceFlags = ambientRecorded == true ? ActivityTraceFlags.Recorded : ActivityTraceFlags.None;
            using var scope = host.Services.CreateScope();
            using var entity = Locate(scope.ServiceProvider, "traced-entity", instance);
            var traceId = ActivityTraceId.CreateRandom();
            var parentId = ActivitySpanId.CreateRandom();
            var header = JsonConvert.DeserializeObject<ActivityHeader>(JsonConvert.SerializeObject(new ActivityHeader
            {
                TraceParent = $"00-{traceId}-{parentId}-{(recorded ? "01" : "00")}",
                TraceState = "stateflows=test",
            }))!;
            Assert.IsTrue(header.ActivityContext.IsRemote);
            Assert.AreEqual(EventStatus.Consumed, (await entity.SendAsync(new TracedMutation { Amount = 3 },
                new Dictionary<string, EventHeader>
                {
                    [nameof(ActivityHeader)] = header,
                    ["EntityTracingRemoteParent"] = header,
                })).Status);
            if (!recorded)
            {
                Assert.AreEqual(0, spans.Count,
                    "An unrecorded remote parent must suppress operations even when the ambient parent is recorded.");
                return;
            }
            var root = spans.Single(span => span.DisplayName.StartsWith("Entity '"));
            Assert.AreEqual(traceId, root.TraceId);
            Assert.AreEqual(parentId, root.ParentSpanId);
            Assert.AreEqual("stateflows=test", root.TraceStateString);
            Assert.IsTrue(spans.Count > 1);
            Assert.IsTrue(spans.Where(span => span != root).All(span =>
                span.TraceId == traceId && span.ParentSpanId == root.SpanId));
        }
        finally
        {
            Activity.Current = ambient;
            await host.StopAsync();
        }
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public async Task NoTracingSuppressesAllEntitySpansAndTracerLogsWithoutChangingResults(bool useAttribute)
    {
        var instance = Guid.NewGuid().ToString("N");
        var spans = new ConcurrentQueue<Activity>();
        var logs = new TracerLogs();
        using var host = CreateHost(instance, spans, logs);
        await host.StartAsync();
        try
        {
            using var scope = host.Services.CreateScope();
            using var entity = Locate(scope.ServiceProvider, "traced-entity", instance);
            using var caller = new Activity("caller").SetIdFormat(ActivityIdFormat.W3C).Start();
            caller.ActivityTraceFlags = ActivityTraceFlags.Recorded;
            foreach (var amount in new[] { 3, -1 })
            {
                var result = useAttribute
                    ? await entity.SendAsync(new QuietMutation { Amount = amount })
                    : await entity.SendAsync(new TracedMutation { Amount = amount }, NoTracingHeaders());
                Assert.AreEqual(amount == 3 ? EventStatus.Consumed : EventStatus.Failed, result.Status);
            }
            var projection = await entity.RequestAsync(new ProjectionRequest<TracedView>(), NoTracingHeaders());
            Assert.AreEqual(3, projection.Response!.Value);
            Assert.AreEqual(0, spans.Count);
            Assert.AreEqual(0, logs.Messages.Count);
            Assert.AreEqual(ActivityStatusCode.Unset, caller.Status);
            Assert.IsFalse(caller.Events.Any(e => e.Name == "exception"));
        }
        finally
        {
            await host.StopAsync();
        }
    }

    private static IHost CreateHost(string instance, ConcurrentQueue<Activity> spans, TracerLogs? logs = null)
    {
        var ports = GetFreeTcpPorts();
        var cluster = Guid.NewGuid().ToString("N");
        return Host.CreateDefaultBuilder()
            .ConfigureLogging(logging =>
            {
                logging.SetMinimumLevel(LogLevel.Warning);
                if (logs != null) logging.AddProvider(logs);
            })
            .ConfigureServices(services => services
                .AddStateflows(sf => sf.AddOpenTelemetry()
                    .AddClientInterceptor<RestoreRemoteParentInterceptor>()
                    .AddEntities(entities => entities
                        .AddObserver((_, _) => new AsyncObserver())
                        .AddExceptionHandler((_, _) => new ThrowingHandler())
                        .AddEntity<ITracedEntity>("traced-entity")
                        .AddEntity<ITracedEntity>("failing-initialization", entity => entity
                            .AddDefaultInitializer(_ => throw new InvalidOperationException("initialization"))))
                )
                .AddOpenTelemetry().WithTracing(tracing =>
                    tracing.AddProcessor(new CapturingProcessor(spans, instance))
                ))
            .UseOrleans(silo => silo
                .UseLocalhostClustering(ports[0], ports[1], null, cluster, cluster)
                .AddMemoryGrainStorage("stateflows")
                .UseInMemoryReminderService()
                .AddStateflowsRuntime())
            .Build();
    }

    private static IBehavior Locate(IServiceProvider services, string name, string instance)
    {
        Assert.IsTrue(services.GetRequiredService<IBehaviorLocator>()
            .TryLocateBehavior(new EntityId(name, instance), out var entity));
        return entity;
    }

    private static Dictionary<string, EventHeader> NoTracingHeaders() => new() { [nameof(NoTracing)] = new NoTracing() };

    public sealed class TracedMutation
    {
        public int Amount { get; set; }
    }

    [NoTracing]
    public sealed class QuietMutation
    {
        public int Amount { get; set; }
    }

    public sealed record TracedView(int Value);

    public interface ITracedEntity
    {
        [Field]
        int Count { get; set; }

        [Field]
        int Computed => Count == -2 ? throw new InvalidOperationException("computed") : Count;

        [Projection]
        TracedView View => Count == -3 ? throw new InvalidOperationException("projection") : new(Computed);

        [Mutation]
        void Change(TracedMutation mutation)
        {
            Count = mutation.Amount;
            if (Count == -1) throw new InvalidOperationException("mutation");
        }

        [Mutation]
        void Quiet(QuietMutation mutation)
        {
            Count = mutation.Amount;
            if (Count == -1) throw new InvalidOperationException("quiet mutation");
        }
    }

    private sealed class AsyncObserver : IEntityObserver
    {
        public async Task OnMutationAsync<TTemplate, TMutationEvent>(
            IMutationObservationContext<TTemplate, TMutationEvent> context, Func<Task> next)
        {
            await Task.Yield();
            await next();
            await Task.Yield();
        }

        public async Task OnComputedFieldAsync<TTemplate, TField>(
            IComputedFieldObservationContext<TTemplate> context, Func<Task<TField>> next)
        {
            await Task.Yield();
            await next();
            await Task.Yield();
        }

        public async Task OnProjectionAsync<TTemplate, TProjection>(
            IProjectionObservationContext<TTemplate> context, Func<Task<TProjection>> next)
        {
            await Task.Yield();
            await next();
            await Task.Yield();
        }
    }

    private sealed class ThrowingHandler : IEntityExceptionHandler
    {
        public Task<bool> OnEntityInitializationExceptionAsync(
            IEntityInitializationExceptionContext context, Exception exception) => Task.FromResult(false);

        public Task<bool> OnMutationExceptionAsync<TTemplate, TMutationEvent>(
            IMutationObservationContext<TTemplate, TMutationEvent> context, Exception exception)
            => throw new InvalidOperationException("later exception handler");

        public Task<bool> OnComputedFieldExceptionAsync<TTemplate, TField>(
            IComputedFieldObservationContext<TTemplate> context, Exception exception) => Task.FromResult(false);

        public Task<bool> OnProjectionExceptionAsync<TTemplate, TProjection>(
            IProjectionObservationContext<TTemplate> context, Exception exception) => Task.FromResult(false);
    }

    private sealed class CapturingProcessor(ConcurrentQueue<Activity> spans, string instance) : BaseProcessor<Activity>
    {
        public override void OnEnd(Activity activity)
        {
            if (activity.Source.Name == "Stateflows" &&
                activity.GetTagItem("EntityId")?.ToString()?.Contains(instance) == true)
                spans.Enqueue(activity);
        }
    }

    public sealed class RestoreRemoteParentInterceptor : IStateflowsClientInterceptor
    {
        public bool BeforeDispatchEvent(EventHolder eventHolder)
        {
            // Simulate a transport-supplied context after the standard client interceptor propagates ambient context.
            if (eventHolder.Headers.Remove("EntityTracingRemoteParent", out var remote))
                eventHolder.Headers[nameof(ActivityHeader)] = remote;
            return true;
        }

        public void AfterDispatchEvent(EventHolder eventHolder) { }
    }

    private sealed class TracerLogs : ILoggerProvider
    {
        public ConcurrentQueue<string> Messages { get; } = new();
        public ILogger CreateLogger(string categoryName) => new Logger(this, categoryName);
        public void Dispose() { }

        private sealed class Logger(TracerLogs owner, string category) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
            public bool IsEnabled(LogLevel logLevel) => true;
            public void Log<TState>(LogLevel logLevel, Microsoft.Extensions.Logging.EventId eventId, TState state, Exception? exception,
                Func<TState, Exception?, string> formatter)
            {
                if (category == "Stateflows.Extensions.OpenTelemetry.EntityTracer")
                    owner.Messages.Enqueue(formatter(state, exception));
            }
        }
    }

    private static int[] GetFreeTcpPorts()
    {
        var listeners = new[] { new TcpListener(IPAddress.Loopback, 0), new TcpListener(IPAddress.Loopback, 0) };
        try
        {
            foreach (var listener in listeners) listener.Start();
            return listeners.Select(listener => ((IPEndPoint)listener.LocalEndpoint).Port).ToArray();
        }
        finally
        {
            foreach (var listener in listeners) listener.Stop();
        }
    }
}
