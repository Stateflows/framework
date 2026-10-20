using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Hosting;
using OpenTelemetry;
using OpenTelemetry.Trace;
using Stateflows;
using Stateflows.Activities;
using Stateflows.Common;
using Stateflows.StateMachines;

namespace Orleans.IntegrationTests;

[TestClass]
public sealed class BehaviorTracingTests
{
    [TestMethod]
    [DataRow(null)]
    [DataRow(true)]
    [DataRow(false)]
    public async Task DoActivityInitializationHonorsOwnerTraceSampling(bool? recorded)
    {
        var instance = Guid.NewGuid().ToString("N");
        var spans = new ConcurrentQueue<Activity>();
        using var host = CreateHostBuilder(instance, spans).Build();
        try
        {
            await host.StartAsync();
            using var scope = host.Services.CreateScope();
            var locator = scope.ServiceProvider.GetRequiredService<IStateMachineLocator>();
            Assert.IsTrue(locator.TryLocateStateMachine(new StateMachineId("traced", instance), out var machine));
            using (machine)
            {
                using var incoming = recorded.HasValue ? new Activity("incoming request").SetIdFormat(ActivityIdFormat.W3C).Start() : null;
                if (incoming != null)
                    incoming.ActivityTraceFlags = recorded == true ? ActivityTraceFlags.Recorded : ActivityTraceFlags.None;

                Assert.AreEqual(EventStatus.Initialized, (await machine.SendAsync(new Initialize())).Status);

                var activityLocator = scope.ServiceProvider.GetRequiredService<IActivityLocator>();
                Assert.IsTrue(activityLocator.TryLocateActivity(new ActivityId("traced.running.do", instance), out var doActivity));
                using (doActivity)
                    await doActivity.SendAsync(new BehaviorInfoRequest());

                if (recorded == false)
                {
                    Assert.IsFalse(spans.Any(span => span.DisplayName.Contains("Initialize")),
                        "Parent-based sampling must not export initialization spans for an unrecorded parent.");
                    return;
                }

                var elapsed = Stopwatch.StartNew();
                Activity? activityInitialization;
                do
                {
                    activityInitialization = spans.FirstOrDefault(span =>
                        span.DisplayName.StartsWith("Activity '") && span.DisplayName.Contains("Initialize"));
                    if (activityInitialization != null) break;
                    await Task.Delay(20);
                } while (elapsed.Elapsed < TimeSpan.FromSeconds(5));

                Assert.IsNotNull(activityInitialization, "The doActivity must process its initialization.");
                var machineInitialization = spans.Single(span =>
                    span.DisplayName.StartsWith("State Machine '") && span.DisplayName.Contains("Initialize"));

                Assert.AreNotEqual(default, activityInitialization.TraceId,
                    "The doActivity initialization span must carry a valid trace id.");
                Assert.AreEqual(machineInitialization.TraceId, activityInitialization.TraceId,
                    "The doActivity initialization must join the owner state machine trace.");
                Assert.AreNotEqual(default, activityInitialization.ParentSpanId);
                if (incoming != null)
                    Assert.AreEqual(incoming.TraceId, machineInitialization.TraceId);
            }
        }
        finally
        {
            await host.StopAsync();
        }
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task HttpRequestSamplingControlsBehaviorTraceExport(bool instrumentAspNetCore)
    {
        var instance = Guid.NewGuid().ToString("N");
        var spans = new ConcurrentQueue<Activity>();
        var builder = CreateHostBuilder(instance, spans)
            .ConfigureWebHost(web => web.UseKestrel().UseUrls("http://127.0.0.1:0")
                .Configure(app => app.Run(async context =>
                {
                    var requestActivity = Activity.Current!;
                    var locator = context.RequestServices.GetRequiredService<IStateMachineLocator>();
                    Assert.IsTrue(locator.TryLocateStateMachine(new StateMachineId("traced", instance), out var machine));
                    using (machine)
                    {
                        Assert.AreEqual(EventStatus.Initialized, (await machine.SendAsync(new Initialize())).Status);
                        Assert.AreEqual(EventStatus.Forwarded, (await machine.SendAsync(new TracedEvent())).Status);
                    }

                    var activityLocator = context.RequestServices.GetRequiredService<IActivityLocator>();
                    Assert.IsTrue(activityLocator.TryLocateActivity(new ActivityId("traced.running.do", instance), out var doActivity));
                    using (doActivity)
                        await doActivity.SendAsync(new BehaviorInfoRequest());

                    Assert.AreEqual(instrumentAspNetCore, requestActivity.Recorded);
                    await context.Response.WriteAsync(requestActivity.TraceId.ToHexString());
                })));
        if (instrumentAspNetCore)
            builder.ConfigureServices(services => services.AddOpenTelemetry()
                .WithTracing(tracing => tracing.AddAspNetCoreInstrumentation()));

        using var host = builder.Build();
        try
        {
            await host.StartAsync();
            var server = host.Services.GetRequiredService<IServer>();
            var address = server.Features.Get<IServerAddressesFeature>()!.Addresses.Single();
            using var client = new HttpClient();
            var traceId = await client.GetStringAsync(address);
            var initializationSpans = spans.Where(span => span.DisplayName.Contains("Initialize")).ToArray();
            var eventSpans = spans.Where(span => span.DisplayName.Contains(nameof(TracedEvent))).ToArray();
            if (instrumentAspNetCore)
            {
                Assert.IsTrue(initializationSpans.Any(span => span.DisplayName.StartsWith("State Machine '")));
                Assert.IsTrue(initializationSpans.Any(span => span.DisplayName.StartsWith("Activity '")));
                foreach (var span in initializationSpans)
                    Assert.AreEqual(traceId, span.TraceId.ToHexString());
                Assert.IsTrue(eventSpans.Any(span => span.DisplayName.StartsWith("State Machine '")));
                Assert.IsTrue(eventSpans.Any(span => span.DisplayName.StartsWith("Activity '")));
                foreach (var span in eventSpans)
                    Assert.AreEqual(traceId, span.TraceId.ToHexString());
            }
            else
            {
                Assert.AreEqual(0, initializationSpans.Length,
                    "Without ASP.NET Core instrumentation, the unrecorded HTTP parent suppresses behavior spans.");
                Assert.AreEqual(0, eventSpans.Length);
            }
        }
        finally
        {
            await host.StopAsync();
        }
    }

    private static IHostBuilder CreateHostBuilder(string instance, ConcurrentQueue<Activity> spans)
    {
        var ports = GetFreeTcpPorts(2);
        var clusterId = Guid.NewGuid().ToString("N");
        return Host.CreateDefaultBuilder()
            .ConfigureLogging(logging => logging.SetMinimumLevel(LogLevel.Warning))
            .ConfigureServices(services => services
                .AddStateflows(stateflows => stateflows
                    .AddOpenTelemetry()
                    .AddStateMachines(machines => machines.AddStateMachine(
                        "traced",
                        machine => machine
                            .AddInitialState("running", state => state
                                .AddDoActivity(activity => activity
                                    .AddAcceptEventAction<TracedEvent>(_ => Task.CompletedTask)))))
                )
                .AddOpenTelemetry().WithTracing(tracing =>
                    tracing.AddProcessor(new CapturingProcessor(spans, instance))
                )
            )
            .UseOrleans(silo => silo
                .UseLocalhostClustering(ports[0], ports[1], null, clusterId, clusterId)
                .AddMemoryGrainStorageAsDefault()
                .UseInMemoryReminderService()
                .AddStateflowsRuntime());
    }

    public sealed class TracedEvent;

    private sealed class CapturingProcessor(ConcurrentQueue<Activity> spans, string instance) : BaseProcessor<Activity>
    {
        public override void OnEnd(Activity activity)
        {
            if (activity.DisplayName.Contains(instance))
                spans.Enqueue(activity);
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
            foreach (var listener in listeners)
            {
                listener?.Stop();
            }
        }
        return ports;
    }
}
