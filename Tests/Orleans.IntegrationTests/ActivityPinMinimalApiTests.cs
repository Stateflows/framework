using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stateflows;
using Stateflows.Activities;
using Stateflows.Common;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Utilities;
using Stateflows.Interfaces;
using Stateflows.Extensions.MinimalAPIs;
using Stateflows.Extensions.MinimalAPIs.Attributes;
using CollisionA = Orleans.IntegrationTests.ActivityPinTypesA;
using CollisionB = Orleans.IntegrationTests.ActivityPinTypesB;

namespace Orleans.IntegrationTests;

[TestClass]
public sealed class ActivityPinMinimalApiTests
{
    private WebApplication app = null!;
    private HttpClient http = null!;
    private readonly TaskCompletionSource started = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource release = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly ConcurrentBag<int> received = [];

    [TestInitialize]
    public async Task Initialize()
    {
        var ports = GetPorts();
        var cluster = Guid.NewGuid().ToString("N");
        var builder = WebApplication.CreateBuilder();
        builder.Logging.SetMinimumLevel(LogLevel.Error);
        builder.WebHost.UseUrls($"http://127.0.0.1:{ports[0]}");
        builder.Services.AddAuthentication("pin-test")
            .AddScheme<AuthenticationSchemeOptions, PinAuthenticationHandler>("pin-test", _ => { });
        builder.Services.AddAuthorization();
        builder.Host.UseOrleans(silo => silo
            .UseLocalhostClustering(ports[1], ports[2], null, cluster, cluster)
            .UseInMemoryReminderService()
            .AddMemoryGrainStorageAsDefault()
            .AddStateflowsRuntime());
        builder.Services.AddStateflows(sf => sf
            .AddDefaultInstance(new ActivityClass("pin-default"))
            .AddActivities(activities => activities
                .AddActivity("pin-http", activity => activity
                    .AddInputPin<int>(pin => pin.AddFlow("numbers"))
                    .AddInputPin<string>(pin => pin.AddFlow<OutputPin<string>>())
                    .AddOutputPin<int>().AddOutputPin<string>()
                    .AddAction("numbers", async context =>
                    {
                        var values = context.GetTokensOfType<int>().ToArray();
                        foreach (var value in values) received.Add(value);
                        if (values.Contains(-1))
                        {
                            started.TrySetResult();
                            await release.Task;
                        }
                        context.PassTokensOfTypeOn<int>();
                    }, node => node.AddFlow<int, OutputPin<int>>().Reentrant())
                    .AddInputPin<HiddenPin>(_ => { }).AddOutputPin<HiddenPin>())
                .AddActivity("pin-default", activity => activity
                    .AddInputPin<int>(pin => pin.AddFlow<OutputPin<int>>()).AddOutputPin<int>())
                .AddActivity("pin-nested", activity => activity
                    .AddInputPin<int>(pin => pin.AddFlow("inner"))
                    .AddStructuredActivity("inner", scope => scope
                        .AddInputPin<int>(pin => pin.AddFlow("inner-int"))
                        .AddInputPin<string>(pin => pin.AddFlow("inner-string"))
                        .AddOutputPin<int>("inner-int").AddOutputPin<string>("inner-string"))
                    .AddOutputPin<int>())
                .AddActivity("pin-versions", 1, activity => activity
                    .AddInputPin<int>(pin => pin.AddFlow<OutputPin<int>>()).AddOutputPin<int>())
                .AddActivity("pin-versions", 2, activity => activity
                    .AddInputPin<string>(pin => pin.AddFlow<OutputPin<string>>()).AddOutputPin<string>())
                .AddActivity("pin-removed", 1, activity => activity
                    .AddInputPin<int>(pin => pin.AddFlow<OutputPin<int>>()).AddOutputPin<int>())
                .AddActivity("pin-removed", 2, activity => activity
                    .AddInitial(initial => initial.AddControlFlow<FinalNode>()).AddFinal())
                .AddActivity("pin-secure", activity => activity
                    .AddInputPin<SecurePin>(pin => pin.AddFlow<OutputPin<SecurePin>>()).AddOutputPin<SecurePin>())
                .AddActivity("pin-configured", activity => activity
                    .AddInputPin<int>(pin => pin.AddFlow<OutputPin<int>>()).AddOutputPin<int>())));

        app = builder.Build();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapStateflowsMinimalAPIsEndpoints(api => api
            .ConfigureActivities(activities => activities.ConfigureActivity("pin-configured", activity => activity
                .ConfigureStandardInstances(instances => instances
                    .ConfigureEventEndpoint<TokensInput<int>>(endpoint => endpoint.UpdateRoute(_ => "/custom/input/{instance}"))
                    .ConfigureEventEndpoint<int>(endpoint => endpoint.UpdateRoute(_ => "/custom/output/{instance}"))))));
        await app.StartAsync();
        http = new HttpClient { BaseAddress = new Uri($"http://127.0.0.1:{ports[0]}") };
    }

    [TestCleanup]
    public async Task Cleanup()
    {
        release.TrySetResult();
        http?.Dispose();
        if (app != null)
        {
            await app.StopAsync();
            await app.DisposeAsync();
        }
    }

    private static int[] GetPorts()
    {
        var listeners = Enumerable.Range(0, 3).Select(_ => new TcpListener(IPAddress.Loopback, 0)).ToArray();
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

    private static StringContent Batch<T>(params T[] tokens)
        => new(JsonSerializer.Serialize(new { tokens }), Encoding.UTF8, "application/json");

    private static string Route(string suffix, string instance = "a")
        => $"/stateflows/activities/pin-http/{instance}/{suffix}";

    private static async Task<JsonElement> ReadSseAsync(StreamReader reader, CancellationToken token)
    {
        while (await reader.ReadLineAsync(token) is { } line)
            if (line.StartsWith("data: ", StringComparison.Ordinal))
                return JsonDocument.Parse(line[6..]).RootElement.Clone();
        throw new EndOfStreamException("Output SSE ended before a notification arrived.");
    }

    [TestMethod]
    [Timeout(60_000)]
    public async Task InputBatchesAndOutputSseUseTheDeclaredTypeWithoutReplayingOldTokens()
    {
        await PublishRetainedOutputAsync(99);
        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(20));
        using var request = new HttpRequestMessage(HttpMethod.Get, Route("output/int32?stream=true"));
        using var output = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellation.Token);
        Assert.AreEqual(HttpStatusCode.OK, output.StatusCode);
        Assert.AreEqual("text/event-stream", output.Content.Headers.ContentType?.MediaType);
        using var reader = new StreamReader(await output.Content.ReadAsStreamAsync(cancellation.Token));

        var input = await http.PostAsync(Route("input/int32"), Batch(1, 2), cancellation.Token);
        Assert.AreEqual(HttpStatusCode.OK, input.StatusCode, await input.Content.ReadAsStringAsync());
        var first = await ReadSseAsync(reader, cancellation.Token);
        var secondInBatch = await ReadSseAsync(reader, cancellation.Token);
        CollectionAssert.AreEqual(new[] { 1, 2 }, new[]
            { first.GetProperty("payload").GetInt32(), secondInBatch.GetProperty("payload").GetInt32() });

        Assert.AreEqual(HttpStatusCode.OK, (await http.PostAsync(Route("input/string"), Batch("not an integer"), cancellation.Token)).StatusCode);
        Assert.AreEqual(HttpStatusCode.OK, (await http.PostAsync(Route("input/int32"), Batch(3), cancellation.Token)).StatusCode);
        var second = await ReadSseAsync(reader, cancellation.Token);
        Assert.AreEqual(3, second.GetProperty("payload").GetInt32());
        cancellation.Cancel();
    }

    [TestMethod]
    [Timeout(60_000)]
    public async Task OutputWithoutStreamWaitsForNextNotificationRatherThanQueryingHistory()
    {
        Assert.AreEqual(HttpStatusCode.OK, (await http.PostAsync(Route("input/int32"), Batch(1))).StatusCode);
        await PublishRetainedOutputAsync(99);
        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        var next = http.GetAsync(Route("output/int32"), cancellation.Token);
        // Repeated new batches avoid assuming HTTP dispatch has already installed the transient watcher.
        while (!next.IsCompleted)
        {
            await http.PostAsync(Route("input/int32"), Batch(7), cancellation.Token);
            await Task.Delay(10, cancellation.Token);
        }
        using var response = await next;
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.AreEqual(7, json.RootElement.GetInt32());
    }

    private async Task PublishRetainedOutputAsync(int value)
    {
        var id = new BehaviorId(BehaviorType.Activity, "pin-http", "a");
        var subscriptions = app.Services.GetRequiredService<IGrainFactory>()
            .GetGrain<ISubscriptionsGrain>(StateflowsGrainKeys.ForBehavior("host", id));
        var notification = value.ToEventHolder(id);
        notification.SentAt = DateTime.Now;
        notification.Retained = true;
        await subscriptions.PublishAsync([notification]);
    }

    [TestMethod]
    [Timeout(60_000)]
    public async Task SendAsStreamReturnsAcceptedWhileAnOrdinaryInputIsStillExecuting()
    {
        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        using var request = new HttpRequestMessage(HttpMethod.Get, Route("output/int32?stream=true"));
        using var output = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellation.Token);
        using var reader = new StreamReader(await output.Content.ReadAsStreamAsync(cancellation.Token));
        var ordinary = http.PostAsync(Route("input/int32"), Batch(-1));
        await started.Task.WaitAsync(TimeSpan.FromSeconds(10));
        var stream = await http.PostAsync(Route("input/int32?sendAsStream=true"), Batch(5))
            .WaitAsync(TimeSpan.FromSeconds(10));
        Assert.AreEqual(HttpStatusCode.Accepted, stream.StatusCode, await stream.Content.ReadAsStringAsync());
        Assert.AreEqual("Accepted", JsonDocument.Parse(await stream.Content.ReadAsStringAsync()).RootElement.GetProperty("statusText").GetString());
        Assert.IsFalse(ordinary.IsCompleted);
        var notification = await ReadSseAsync(reader, cancellation.Token);
        Assert.AreEqual(5, notification.GetProperty("payload").GetInt32());
        Assert.IsFalse(ordinary.IsCompleted, "Streamed output must not wait for the active ordinary input to complete.");
        release.TrySetResult();
        Assert.AreEqual(HttpStatusCode.OK, (await ordinary.WaitAsync(TimeSpan.FromSeconds(10))).StatusCode);
        cancellation.Cancel();
    }

    [TestMethod]
    [Timeout(60_000)]
    public async Task StreamedInputPublishesTypedOutputOverSse()
    {
        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        using var request = new HttpRequestMessage(HttpMethod.Get, Route("output/int32?stream=true"));
        using var output = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellation.Token);
        using var reader = new StreamReader(await output.Content.ReadAsStreamAsync(cancellation.Token));
        using var input = await http.PostAsync(Route("input/int32?sendAsStream=true"), Batch(4, 6), cancellation.Token);
        Assert.AreEqual(HttpStatusCode.Accepted, input.StatusCode);
        var notification = await ReadSseAsync(reader, cancellation.Token);
        var second = await ReadSseAsync(reader, cancellation.Token);
        CollectionAssert.AreEqual(new[] { 4, 6 }, new[]
            { notification.GetProperty("payload").GetInt32(), second.GetProperty("payload").GetInt32() });
        cancellation.Cancel();
    }

    [TestMethod]
    [Timeout(60_000)]
    public async Task CancelingAnOutputWaitDoesNotBlockSubsequentInputOrWatch()
    {
        using var canceled = new CancellationTokenSource();
        var waiting = http.GetAsync(Route("output/int32"), canceled.Token);
        canceled.Cancel();
        await Assert.ThrowsAsync<OperationCanceledException>(async () => await waiting);

        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        using var request = new HttpRequestMessage(HttpMethod.Get, Route("output/int32?stream=true"));
        using var output = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellation.Token);
        using var reader = new StreamReader(await output.Content.ReadAsStreamAsync(cancellation.Token));
        Assert.AreEqual(HttpStatusCode.OK, (await http.PostAsync(Route("input/int32"), Batch(8), cancellation.Token)).StatusCode);
        var notification = await ReadSseAsync(reader, cancellation.Token);
        Assert.AreEqual(8, notification.GetProperty("payload").GetInt32());
        cancellation.Cancel();
    }

    [TestMethod]
    [Timeout(60_000)]
    public async Task TokenTypeAuthorizationAppliesToBothInputAndOutput()
    {
        const string root = "/stateflows/activities/pin-secure/a";
        Assert.AreEqual(HttpStatusCode.Unauthorized, (await http.PostAsync(root + "/input/securePin", Batch(new SecurePin(1)))).StatusCode);
        Assert.AreEqual(HttpStatusCode.Unauthorized, (await http.GetAsync(root + "/output/securePin")).StatusCode);

        http.DefaultRequestHeaders.Add("X-Pin-User", "reader");
        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        using var request = new HttpRequestMessage(HttpMethod.Get, root + "/output/securePin?stream=true");
        using var output = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellation.Token);
        Assert.AreEqual(HttpStatusCode.OK, output.StatusCode);
        using var reader = new StreamReader(await output.Content.ReadAsStreamAsync(cancellation.Token));
        Assert.AreEqual(HttpStatusCode.OK, (await http.PostAsync(root + "/input/securePin", Batch(new SecurePin(2)), cancellation.Token)).StatusCode);
        var notification = await ReadSseAsync(reader, cancellation.Token);
        Assert.AreEqual(2, notification.GetProperty("payload").GetProperty("value").GetInt32());
        cancellation.Cancel();
    }

    [TestMethod]
    public async Task CollidingTokenRouteNamesFailMappingExplicitly()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddStateflows(sf => sf.AddLegacyRuntime().AddActivities(activities =>
            activities.AddActivity("collision", activity => activity
                .AddInputPin<CollisionA.Token>(_ => { })
                .AddInputPin<CollisionB.Token>(_ => { }))));
        await using var collisionApp = builder.Build();
        var exception = Assert.Throws<InvalidOperationException>(() => collisionApp.MapStateflowsMinimalAPIsEndpoints());
        StringAssert.Contains(exception.Message, "same endpoint");
        StringAssert.Contains(exception.Message, "input/token");
    }

    [TestMethod]
    [DataRow("{}")]
    [DataRow("{\"tokens\":null}")]
    [DataRow("{\"tokens\":[\"bad\"]}")]
    public async Task InvalidTypedBatchesDoNotInjectTokens(string json)
    {
        var response = await http.PostAsync(Route("input/int32"), new StringContent(json, Encoding.UTF8, "application/json"));
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.AreEqual(0, received.Count);
    }

    [TestMethod]
    public async Task ImplicitInitializationCanBeSuppressed()
    {
        var response = await http.PostAsync(Route("input/int32?implicitInitialization=false", "cold"), Batch(1));
        Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);
        Assert.AreEqual(0, received.Count);
    }

    [TestMethod]
    [Timeout(60_000)]
    public async Task DefaultInstanceAndConfiguredRoutesWorkIndependentlyOfTheLastVisitedActivity()
    {
        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        using var request = new HttpRequestMessage(HttpMethod.Get, "/stateflows/activities/pin-default/output/int32?stream=true");
        using var output = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellation.Token);
        Assert.AreEqual(HttpStatusCode.OK, output.StatusCode);
        using var reader = new StreamReader(await output.Content.ReadAsStreamAsync(cancellation.Token));
        Assert.AreEqual(HttpStatusCode.OK, (await http.PostAsync("/stateflows/activities/pin-default/input/int32", Batch(9), cancellation.Token)).StatusCode);
        var notification = await ReadSseAsync(reader, cancellation.Token);
        Assert.AreEqual(9, notification.GetProperty("payload").GetInt32());
        Assert.AreEqual(HttpStatusCode.OK, (await http.PostAsync("/stateflows/custom/input/a", Batch(2), cancellation.Token)).StatusCode);
        cancellation.Cancel();
    }

    [TestMethod]
    public void EndpointDiscoveryUsesOnlyRootPinsFromTheNewestDefinitionAndHonorsExclusions()
    {
        var routes = ((IEndpointRouteBuilder)app).DataSources.SelectMany(source => source.Endpoints)
            .OfType<RouteEndpoint>().Select(endpoint => endpoint.RoutePattern.RawText!).ToArray();
        CollectionAssert.Contains(routes, "/stateflows/activities/pin-http/{instance}/input/int32");
        CollectionAssert.Contains(routes, "/stateflows/activities/pin-http/{instance}/output/string");
        CollectionAssert.DoesNotContain(routes, "/stateflows/activities/pin-http/{instance}/input/hiddenPin");
        CollectionAssert.DoesNotContain(routes, "/stateflows/activities/pin-nested/{instance}/input/string");
        CollectionAssert.DoesNotContain(routes, "/stateflows/activities/pin-nested/{instance}/output/string");
        Assert.AreEqual(1, routes.Count(route => route == "/stateflows/activities/pin-versions/{instance}/input/string"));
        CollectionAssert.DoesNotContain(routes, "/stateflows/activities/pin-versions/{instance}/input/int32");
        CollectionAssert.DoesNotContain(routes, "/stateflows/activities/pin-removed/{instance}/input/int32");
        CollectionAssert.DoesNotContain(routes, "/stateflows/activities/pin-removed/{instance}/output/int32");
        CollectionAssert.Contains(routes, "/stateflows/custom/output/{instance}");
    }

    private sealed class PinAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
            => Task.FromResult(Request.Headers.TryGetValue("X-Pin-User", out var user)
                ? AuthenticateResult.Success(new AuthenticationTicket(
                    new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, user.ToString())], Scheme.Name)), Scheme.Name))
                : AuthenticateResult.NoResult());
    }

}

[NoApiMapping]
public sealed class HiddenPin;

[Authorize]
public sealed record SecurePin(int Value);
