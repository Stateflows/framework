using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Stateflows.Entities.Attributes;
using Stateflows.Entities.Enums;
using Stateflows.Extensions.MinimalAPIs;

namespace Entity.IntegrationTests.Tests
{
    public record CounterIncrement(int Amount);

    public record AccountOpen(string Owner);

    public record AccountRename(string Owner);

    public class CounterView
    {
        public int Count { get; set; }
    }

    public class CounterSummary
    {
        public string Text { get; set; } = string.Empty;
    }

    public interface ICounterApiTemplate
    {
        [Field(FieldAccess.Get | FieldAccess.Set)]
        int Count { get; set; }

        [Field(FieldAccess.Get)]
        string Label { get; set; }

        [Field(FieldAccess.Set)]
        string Secret { get; set; }

        [Field]
        int Hidden { get; set; }

        [Projection(PublishScope.Self)]
        CounterView View => new() { Count = Count };

        [Projection]
        CounterSummary Summary => new() { Text = $"{Secret}:{Count}" };

        [Mutation]
        void Increment(CounterIncrement mutation) { Count += mutation.Amount; }
    }

    public interface IAccountApiTemplate
    {
        [Field(FieldAccess.Get)]
        string Owner { get; set; }

        [Mutation]
        void Rename(AccountRename mutation) { Owner = mutation.Owner; }
    }

    public interface IConflictingApiTemplate
    {
        [Field(FieldAccess.Get)]
        string Status { get; set; }
    }

    [TestClass]
    public class MinimalApis
    {
        private WebApplication? app;
        private HttpClient http = null!;

        [TestInitialize]
        public async Task Initialize()
        {
            var port = GetFreePort();
            var builder = WebApplication.CreateBuilder();
            builder.Logging.SetMinimumLevel(LogLevel.Error);
            builder.WebHost.UseUrls($"http://127.0.0.1:{port}");
            builder.Services.AddStateflows(b => b
                .AddLegacyRuntime()
                .AddEntities(entities => entities
                    .AddEntity<ICounterApiTemplate>("counter")
                    .AddEntity<IAccountApiTemplate>("account", entity => entity
                        .AddInitializer<AccountOpen>(context => context.Entity.Owner = context.InitializationEvent.Owner)
                    )
                )
            );

            app = builder.Build();
            app.MapStateflowsMinimalAPIsEndpoints();
            await app.StartAsync();

            http = new HttpClient { BaseAddress = new Uri($"http://127.0.0.1:{port}") };
        }

        [TestCleanup]
        public async Task Cleanup()
        {
            http.Dispose();
            if (app != null)
            {
                await app.StopAsync();
                await app.DisposeAsync();
            }
        }

        private static int GetFreePort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        private static StringContent Body(object @event)
            => new(JsonSerializer.Serialize(new { @event }), Encoding.UTF8, "application/json");

        private static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage response)
            => JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;

        private static string[] GetLinks(JsonElement body)
            => body.TryGetProperty("links", out var links)
                ? links.EnumerateArray().Select(l => $"{l.GetProperty("method").GetString()} {l.GetProperty("rel").GetString()}").ToArray()
                : [];

        private static string GetStatusText(JsonElement body)
            => body.GetProperty("metadata").GetProperty("behaviorStatusText").GetString()!;

        [TestMethod]
        public async Task ClassesAndInstancesAreListed()
        {
            StringAssert.Contains(await http.GetStringAsync("/stateflows/classes/entities"), "\"counter\"");

            Assert.AreEqual(HttpStatusCode.Created, (await http.PostAsync("/stateflows/entities/counter/a/initialize", Body(new { }))).StatusCode);

            StringAssert.Contains(await http.GetStringAsync("/stateflows/entities/counter"), "\"a\"");
            StringAssert.Contains(await http.GetStringAsync("/stateflows/entities"), "\"a\"");
        }

        [TestMethod]
        public async Task StandardEndpointsFollowEntityLifecycle()
        {
            var status = await http.GetAsync("/stateflows/entities/counter/a/status");
            Assert.AreEqual(HttpStatusCode.OK, status.StatusCode);
            var body = await ReadJsonAsync(status);
            Assert.AreEqual("NotInitialized", GetStatusText(body));
            CollectionAssert.Contains(GetLinks(body), "POST initialize");
            CollectionAssert.DoesNotContain(GetLinks(body), "POST counterIncrement");

            Assert.AreEqual(HttpStatusCode.Conflict,
                (await http.PostAsync("/stateflows/entities/counter/a/counterIncrement?implicitInitialization=false", Body(new { amount = 1 }))).StatusCode,
                "Mutations without implicit initialization are rejected for an uninitialized entity.");

            var initialize = await http.PostAsync("/stateflows/entities/counter/a/initialize", Body(new { }));
            Assert.AreEqual(HttpStatusCode.Created, initialize.StatusCode);

            body = await ReadJsonAsync(await http.GetAsync("/stateflows/entities/counter/a/status"));
            Assert.AreEqual("Initialized", GetStatusText(body));
            var links = GetLinks(body);
            CollectionAssert.IsSubsetOf(
                new[] { "POST counterIncrement", "GET count", "PUT count", "GET label", "PUT secret", "GET counterView", "GET counterSummary", "GET status", "GET notifications", "DELETE reset" },
                links);
            CollectionAssert.DoesNotContain(links, "POST initialize");
            CollectionAssert.DoesNotContain(links, "GET hidden");

            var reset = await http.DeleteAsync("/stateflows/entities/counter/a");
            Assert.AreEqual(HttpStatusCode.OK, reset.StatusCode);
            Assert.AreEqual("NotInitialized", GetStatusText(await ReadJsonAsync(reset)));

            body = await ReadJsonAsync(await http.GetAsync("/stateflows/entities/counter/a/status"));
            Assert.AreEqual("NotInitialized", GetStatusText(body));
        }

        [TestMethod]
        public async Task MutationsAndFieldsAreExposedAccordingToAccess()
        {
            var mutation = await http.PostAsync("/stateflows/entities/counter/a/counterIncrement", Body(new { amount = 2 }));
            Assert.AreEqual(HttpStatusCode.OK, mutation.StatusCode, await mutation.Content.ReadAsStringAsync());

            var count = await http.GetAsync("/stateflows/entities/counter/a/count");
            Assert.AreEqual(HttpStatusCode.OK, count.StatusCode);
            var body = await ReadJsonAsync(count);
            Assert.AreEqual(2, body.GetProperty("response").GetProperty("value").GetInt32());
            Assert.AreEqual("Count", body.GetProperty("response").GetProperty("name").GetString());

            Assert.AreEqual(HttpStatusCode.OK, (await http.PutAsync("/stateflows/entities/counter/a/count", Body(5))).StatusCode);
            body = await ReadJsonAsync(await http.GetAsync("/stateflows/entities/counter/a/count"));
            Assert.AreEqual(5, body.GetProperty("response").GetProperty("value").GetInt32());

            Assert.AreEqual(HttpStatusCode.OK, (await http.PutAsync("/stateflows/entities/counter/a/secret", Body("s3cret"))).StatusCode);
            body = await ReadJsonAsync(await http.GetAsync("/stateflows/entities/counter/a/counterSummary"));
            Assert.AreEqual("s3cret:5", body.GetProperty("response").GetProperty("text").GetString());

            Assert.AreEqual(HttpStatusCode.OK, (await http.GetAsync("/stateflows/entities/counter/a/label")).StatusCode);
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, (await http.PutAsync("/stateflows/entities/counter/a/label", Body("x"))).StatusCode,
                "Fields without Set access have no PUT endpoint.");
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, (await http.GetAsync("/stateflows/entities/counter/a/secret")).StatusCode,
                "Fields without Get access have no GET endpoint.");
            Assert.AreEqual(HttpStatusCode.NotFound, (await http.GetAsync("/stateflows/entities/counter/a/hidden")).StatusCode);
            Assert.AreEqual(HttpStatusCode.NotFound, (await http.PutAsync("/stateflows/entities/counter/a/hidden", Body(1))).StatusCode);
        }

        [TestMethod]
        public async Task ReadEndpointsDoNotInitializeImplicitlyByDefault()
        {
            Assert.AreEqual(HttpStatusCode.Conflict, (await http.GetAsync("/stateflows/entities/counter/b/count")).StatusCode);
            Assert.AreEqual(HttpStatusCode.Conflict, (await http.GetAsync("/stateflows/entities/counter/b/counterView")).StatusCode);
            Assert.AreEqual("NotInitialized", GetStatusText(await ReadJsonAsync(await http.GetAsync("/stateflows/entities/counter/b/status"))));

            var view = await http.GetAsync("/stateflows/entities/counter/b/counterView?implicitInitialization=true");
            Assert.AreEqual(HttpStatusCode.OK, view.StatusCode);
            Assert.AreEqual(0, (await ReadJsonAsync(view)).GetProperty("response").GetProperty("count").GetInt32());
            Assert.AreEqual("Initialized", GetStatusText(await ReadJsonAsync(await http.GetAsync("/stateflows/entities/counter/b/status"))));
        }

        [TestMethod]
        public async Task InitializersReplaceInitializeEndpoint()
        {
            Assert.AreEqual(HttpStatusCode.NotFound, (await http.PostAsync("/stateflows/entities/account/a/initialize", Body(new { }))).StatusCode);

            var status = await ReadJsonAsync(await http.GetAsync("/stateflows/entities/account/a/status"));
            CollectionAssert.Contains(GetLinks(status), "POST accountOpen");

            var open = await http.PostAsync("/stateflows/entities/account/a/accountOpen", Body(new { owner = "John" }));
            Assert.AreEqual(HttpStatusCode.Created, open.StatusCode, await open.Content.ReadAsStringAsync());

            var owner = await ReadJsonAsync(await http.GetAsync("/stateflows/entities/account/a/owner"));
            Assert.AreEqual("John", owner.GetProperty("response").GetProperty("value").GetString());

            Assert.AreEqual(HttpStatusCode.OK, (await http.PostAsync("/stateflows/entities/account/a/accountRename", Body(new { owner = "Jane" }))).StatusCode);
            owner = await ReadJsonAsync(await http.GetAsync("/stateflows/entities/account/a/owner"));
            Assert.AreEqual("Jane", owner.GetProperty("response").GetProperty("value").GetString());
        }

        [TestMethod]
        public async Task PublishedProjectionStreamsCurrentValueAndRecalculations()
        {
            Assert.AreEqual(HttpStatusCode.OK, (await http.PostAsync("/stateflows/entities/counter/a/counterIncrement", Body(new { amount = 3 }))).StatusCode);

            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            using var request = new HttpRequestMessage(HttpMethod.Get, "/stateflows/entities/counter/a/counterView?stream=true");
            using var response = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeout.Token);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("text/event-stream", response.Content.Headers.ContentType?.MediaType);

            using var reader = new StreamReader(await response.Content.ReadAsStreamAsync(timeout.Token));

            var first = await ReadEventAsync(reader, timeout.Token);
            Assert.AreEqual(3, first.GetProperty("payload").GetProperty("count").GetInt32());

            Assert.AreEqual(HttpStatusCode.OK, (await http.PostAsync("/stateflows/entities/counter/a/counterIncrement", Body(new { amount = 4 }))).StatusCode);

            var next = await ReadEventAsync(reader, timeout.Token);
            Assert.AreEqual(7, next.GetProperty("payload").GetProperty("count").GetInt32());
        }

        [TestMethod]
        public async Task UnpublishedProjectionHasNoStreamParameter()
        {
            Assert.AreEqual(HttpStatusCode.Created, (await http.PostAsync("/stateflows/entities/counter/a/initialize", Body(new { }))).StatusCode);

            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            var response = await http.GetAsync("/stateflows/entities/counter/a/counterSummary?stream=true", timeout.Token);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("application/json", response.Content.Headers.ContentType?.MediaType);
            Assert.AreEqual(":0", (await ReadJsonAsync(response)).GetProperty("response").GetProperty("text").GetString());
        }

        [TestMethod]
        public async Task ConflictingRoutesFailMapping()
        {
            var builder = WebApplication.CreateBuilder();
            builder.Logging.SetMinimumLevel(LogLevel.Error);
            builder.Services.AddStateflows(b => b
                .AddLegacyRuntime()
                .AddEntities(entities => entities
                    .AddEntity<IConflictingApiTemplate>("conflicting")
                )
            );

            await using var conflictingApp = builder.Build();

            var exception = Assert.ThrowsExactly<InvalidOperationException>(() => conflictingApp.MapStateflowsMinimalAPIsEndpoints());
            StringAssert.Contains(exception.Message, "GET /entities/conflicting/{instance}/status");
        }

        private static async Task<JsonElement> ReadEventAsync(StreamReader reader, CancellationToken cancellationToken)
        {
            while (true)
            {
                var line = await reader.ReadLineAsync(cancellationToken);
                if (line == null)
                {
                    throw new EndOfStreamException("Event stream ended unexpectedly.");
                }

                if (line.StartsWith("data: "))
                {
                    return JsonDocument.Parse(line["data: ".Length..]).RootElement;
                }
            }
        }
    }
}
