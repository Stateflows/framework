using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Stateflows.Actions;
using Stateflows.Activities;
using Stateflows.Common;
using Stateflows.Events;
using Stateflows.Extensions.MinimalAPIs;
using Stateflows.StateMachines;

namespace Entity.IntegrationTests.Tests
{
    public sealed class EvenLengthAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value) => value is not string text || text.Length % 2 == 0;
    }

    public sealed class StrictRangeAttribute() : RangeAttribute(1, 10)
    {
        public override bool IsValid(object? value) => base.IsValid(value) && value is int i && i % 2 == 0;
    }

    [Event("orders.ship")]
    public sealed class OrdersShipEvent
    {
        public string? Value { get; set; }
    }

    public sealed class UnmappedRuleAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value) => true;
    }

    public sealed class StructuralClrReply
    {
        [Required]
        public string OrderId { get; set; } = "";

        [Range(1, 5)]
        public int Count { get; set; }
    }

    [TestClass]
    public class StructuralMinimalApis
    {
        private WebApplication? app;
        private HttpClient http = null!;
        private static string? hostedPingValue;

        [TestInitialize]
        public async Task Initialize()
        {
            hostedPingValue = null;
            var port = GetFreePort();
            var builder = WebApplication.CreateBuilder();
            builder.Logging.SetMinimumLevel(LogLevel.Error);
            builder.WebHost.UseUrls($"http://127.0.0.1:{port}");
            builder.Services.AddOpenApi();
            builder.Services.AddStateflows(b => b
                .AddLegacyRuntime()
                .AddEvents(e => e
                    .AddEvent("submit", ev => ev
                        .AddField("orderId", FieldType.String, f => f.AddValidation(new RequiredAttribute()))
                        .AddField("quantity", FieldType.Int32, f => f.AddValidation(new RangeAttribute(1, 10)))
                        .AddField("note", FieldType.String)
                        .AddField("amount", FieldType.Decimal, f => f.AddValidation(new RangeAttribute(0.5, 100.0) { MinimumIsExclusive = true }))
                        .AddField("kind", FieldType.Enum, f => f.AddEnumValues("small", "large"))
                        .AddField("payload", FieldType.Binary, f => f.AddValidation(new MaxLengthAttribute(16)))
                        .AddField("day", FieldType.DateOnly)
                        .AddField("at", FieldType.TimeOnly)
                        .AddField("when", FieldType.DateTimeOffset)
                        .AddField("id", FieldType.Guid)
                        .AddField("code", FieldType.String, f => f
                            .AddValidation(new RegularExpressionAttribute("[A-Z]+"))
                            .AddValidation(new StringLengthAttribute(8) { MinimumLength = 2 }))
                        .AddCollectionField("tags", FieldType.String, f => f
                            .AddValidation(new MaxLengthAttribute(3))
                            .AddItemValidation(new StringLengthAttribute(5)))
                        .AddCollectionField("scores", FieldType.Int32, f => f
                            .AddItemValidation(new RequiredAttribute())
                            .AddItemValidation(new RangeAttribute(0, 100)))
                        .AddObjectField("address", o => o
                            .AddValidation(new RequiredAttribute())
                            .AddField("city", FieldType.String, f => f.AddValidation(new RequiredAttribute()))
                            .AddField("zip", FieldType.String))
                        .AddObjectField("billing", o => o
                            .AddField("city", FieldType.String, f => f.AddValidation(new RequiredAttribute()))))
                    .AddEvent("rules", ev => ev
                        .AddField("label", FieldType.String, f => f.AddValidation(new RangeAttribute(1, 2)))
                        .AddField("even", FieldType.String, f => f.AddValidation(new EvenLengthAttribute()))
                        .AddField("custom", FieldType.String, f => f.AddValidation(new UnmappedRuleAttribute()))
                        .AddField("derived", FieldType.Int32, f => f.AddValidation(new StrictRangeAttribute()))
                        .AddField("size", FieldType.String, f => f.AddValidation(new AllowedValuesAttribute("s", "m")))
                        .AddField("level", FieldType.Int64, f => f.AddValidation(new AllowedValuesAttribute(1, 2))))
                    .AddEvent("hostedShip", ev => ev.AddField("value", FieldType.String))
                    .AddEvent("reply", ev => ev.AddField("orderId", FieldType.String))
                    .AddEvent("ping", ev => ev.AddField("value", FieldType.String))
                    .AddEvent("hostedPing", ev => ev.AddField("value", FieldType.String))
                    .AddRequest("query", "reply", ev => ev.AddField("orderId", FieldType.String))
                    .AddRequest<StructuralClrReply>("clrQuery", ev => ev.AddField("orderId", FieldType.String)))
                .AddStateMachines(s => s
                    .AddStateMachine("structuralDoAction", sm => sm
                        .AddInitialState("working", st => st
                            .AddDoAction(_ => Task.CompletedTask, ab => ab.AddConsumedEvent("hostedShip"))))
                    .AddStateMachine("structuralApi", sm => sm
                        .AddInitialState("waiting", st => st
                            .AddInternalTransition("query", t => t.AddEffect(c =>
                                c.Event.Respond(c.Behavior.ServiceProvider.GetRequiredService<IEventFactory>()
                                    .Create("reply", v => v.Set("orderId", c.Event.Get<string>("orderId"))))))
                            .AddInternalTransition("clrQuery", t => t.AddEffect(c =>
                                c.Event.Respond(new StructuralClrReply { OrderId = c.Event.Get<string>("orderId"), Count = 2 })))
                            .AddInternalTransition("rules")
                            .AddTransition("submit", "hosting"))
                        .AddState("hosting", st => st
                            .AddDoActivity(a => a
                                .AddInitial(i => i.AddControlFlow("receive"))
                                .AddAcceptEventAction("hostedPing", "receive", c =>
                                {
                                    hostedPingValue = c.Event.Get<string>("value");
                                    return Task.CompletedTask;
                                })))))
                .AddActivities(a => a
                    .AddActivity("structuralActivity", ac => ac
                        .AddInitial(i => i.AddControlFlow("receive"))
                        .AddAcceptEventAction("ping", "receive", _ => Task.CompletedTask)))
                .AddActions(a => a
                    .AddAction("structuralAction", _ => Task.CompletedTask, ab => ab.AddConsumedEvent("ping")))
            );

            app = builder.Build();
            app.MapOpenApi();
            app.MapStateflowsMinimalAPIsEndpoints(b => b
                .MapValidationSchema<EvenLengthAttribute>((_, schema) => schema.Pattern = "^(..)*$"));
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

        private static async Task<(WebApplication App, HttpClient Http)> StartCollidingAppAsync(System.Action<Stateflows.Extensions.MinimalAPIs.Interfaces.IMinimalAPIsBuilder> configure, bool clrOrders = false)
        {
            var port = GetFreePort();
            var builder = WebApplication.CreateBuilder();
            builder.Logging.SetMinimumLevel(LogLevel.Error);
            builder.WebHost.UseUrls($"http://127.0.0.1:{port}");
            builder.Services.AddStateflows(b => b
                .AddLegacyRuntime()
                .AddEvents(e =>
                {
                    if (!clrOrders)
                    {
                        e.AddEvent("orders.ship", ev => ev.AddField("value", FieldType.String));
                    }

                    e.AddEvent("billing.ship", ev => ev.AddField("value", FieldType.String));
                })
                .AddStateMachines(m => m
                    .AddStateMachine("colliding", sm => sm
                        .AddInitialState("waiting", s =>
                        {
                            if (clrOrders)
                            {
                                s.AddInternalTransition<OrdersShipEvent>(_ => { });
                            }
                            else
                            {
                                s.AddInternalTransition("orders.ship");
                            }

                            s.AddInternalTransition("billing.ship");
                        }))));

            var collidingApp = builder.Build();
            try
            {
                collidingApp.MapStateflowsMinimalAPIsEndpoints(configure);
                await collidingApp.StartAsync();
            }
            catch
            {
                await collidingApp.DisposeAsync();
                throw;
            }

            return (collidingApp, new HttpClient { BaseAddress = new Uri($"http://127.0.0.1:{port}") });
        }

        [TestMethod]
        public async Task CollidingShortRoutesFailFastAndCanBeRenamed()
        {
            var exception = await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => StartCollidingAppAsync(_ => { }));
            StringAssert.Contains(exception.Message, "orders.ship");
            StringAssert.Contains(exception.Message, "billing.ship");

            var (renamedApp, renamedHttp) = await StartCollidingAppAsync(b => b
                .ConfigureEventEndpoint("billing.ship", e => e.UpdateRoute(r => r.Replace("/ship", "/billingShip"))));
            try
            {
                var orders = await renamedHttp.PostAsync("/stateflows/stateMachines/colliding/c/ship", Body(new { value = "a" }));
                Assert.AreEqual(HttpStatusCode.OK, orders.StatusCode, await orders.Content.ReadAsStringAsync());
                var billing = await renamedHttp.PostAsync("/stateflows/stateMachines/colliding/c/billingShip", Body(new { value = "b" }));
                Assert.AreEqual(HttpStatusCode.OK, billing.StatusCode, await billing.Content.ReadAsStringAsync());
            }
            finally
            {
                renamedHttp.Dispose();
                await renamedApp.StopAsync();
                await renamedApp.DisposeAsync();
            }
        }

        [TestMethod]
        public async Task ClrAndStructuralShortRouteCollisionFailsFastAndCanBeRenamed()
        {
            var exception = await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => StartCollidingAppAsync(_ => { }, clrOrders: true));
            StringAssert.Contains(exception.Message, "orders.ship");
            StringAssert.Contains(exception.Message, "billing.ship");

            var (renamedApp, renamedHttp) = await StartCollidingAppAsync(b => b
                .ConfigureEventEndpoint("billing.ship", e => e.UpdateRoute(r => r.Replace("/ship", "/billingShip"))), clrOrders: true);
            try
            {
                var orders = await renamedHttp.PostAsync("/stateflows/stateMachines/colliding/c/ship", Body(new { value = "a" }));
                Assert.AreEqual(HttpStatusCode.OK, orders.StatusCode, await orders.Content.ReadAsStringAsync());
                var billing = await renamedHttp.PostAsync("/stateflows/stateMachines/colliding/c/billingShip", Body(new { value = "b" }));
                Assert.AreEqual(HttpStatusCode.OK, billing.StatusCode, await billing.Content.ReadAsStringAsync());
            }
            finally
            {
                renamedHttp.Dispose();
                await renamedApp.StopAsync();
                await renamedApp.DisposeAsync();
            }
        }

        [TestMethod]
        public async Task HostedDelegateDoActionConsumedEventIsExposedOnParentStateMachine()
        {
            // The parent machine has no trigger for hostedShip; the endpoint exists only through the do-action's AddConsumedEvent
            const string root = "/stateflows/stateMachines/structuralDoAction/d";
            var valid = await http.PostAsync(root + "/hostedShip", Body(new { value = "v" }));
            var validText = await valid.Content.ReadAsStringAsync();
            Assert.AreNotEqual(HttpStatusCode.NotFound, valid.StatusCode, validText);
            Assert.AreNotEqual(HttpStatusCode.MethodNotAllowed, valid.StatusCode, validText);
            Assert.IsTrue(JsonDocument.Parse(validText).RootElement.TryGetProperty("statusText", out _), validText);

            var invalid = await http.PostAsync(root + "/hostedShip", Body(new { value = 5 }));
            Assert.AreEqual(HttpStatusCode.BadRequest, invalid.StatusCode, await invalid.Content.ReadAsStringAsync());

            var document = await GetOpenApiAsync();
            var schema = EventSchema(document, "/stateflows/stateMachines/structuralDoAction/{instance}/hostedShip");
            Assert.IsTrue(schema.GetProperty("properties").TryGetProperty("value", out _), schema.ToString());
        }

        [TestMethod]
        public void MappingBaseValidationAttributeIsRejected()
        {
            var builder = WebApplication.CreateBuilder();
            builder.Services.AddStateflows(b => b.AddLegacyRuntime());
            var localApp = builder.Build();
            Assert.ThrowsExactly<ArgumentException>(() => localApp.MapStateflowsMinimalAPIsEndpoints(b => b
                .MapValidationSchema<ValidationAttribute>((_, _) => { })));
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

        private async Task<JsonElement> GetOpenApiAsync()
            => JsonDocument.Parse(await http.GetStringAsync("/openapi/v1.json")).RootElement;

        private static JsonElement Resolve(JsonElement document, JsonElement schema)
        {
            while (schema.TryGetProperty("$ref", out var reference))
            {
                schema = document.GetProperty("components").GetProperty("schemas")
                    .GetProperty(reference.GetString()!.Split('/').Last());
            }

            return schema;
        }

        private static JsonElement Operation(JsonElement document, string path)
            => document.GetProperty("paths").GetProperty(path).GetProperty("post");

        private static JsonElement EventSchema(JsonElement document, string path)
            => Resolve(document, Resolve(document, Operation(document, path).GetProperty("requestBody").GetProperty("content")
                .GetProperty("application/json").GetProperty("schema")).GetProperty("properties").GetProperty("event"));

        private static JsonElement ResponseSchema(JsonElement document, string path, string status = "200")
            => Resolve(document, Operation(document, path).GetProperty("responses").GetProperty(status).GetProperty("content")
                .GetProperty("application/json").GetProperty("schema"));

        private static string[] Types(JsonElement schema)
            => schema.TryGetProperty("type", out var type)
                ? type.ValueKind == JsonValueKind.Array ? type.EnumerateArray().Select(t => t.GetString()!).OrderBy(t => t).ToArray() : [type.GetString()!]
                : [];

        private static string[] Required(JsonElement schema)
            => schema.TryGetProperty("required", out var required) ? required.EnumerateArray().Select(r => r.GetString()!).ToArray() : [];

        private static string Text(JsonElement schema, string property)
            => schema.TryGetProperty(property, out var value) ? value.ToString() : "";

        [TestMethod]
        public async Task StructuralEventIsValidatedAndConsumed()
        {
            const string root = "/stateflows/stateMachines/structuralApi/a";

            var invalid = await http.PostAsync(root + "/submit", Body(new { orderId = "x", quantity = 0, address = new { city = "c" } }));
            Assert.AreEqual(HttpStatusCode.BadRequest, invalid.StatusCode, await invalid.Content.ReadAsStringAsync());

            var wrongType = await http.PostAsync(root + "/submit", Body(new { orderId = "x", quantity = "many", address = new { city = "c" } }));
            Assert.AreEqual(HttpStatusCode.BadRequest, wrongType.StatusCode, await wrongType.Content.ReadAsStringAsync());

            var valid = await http.PostAsync(root + "/submit", Body(new { orderId = "x", quantity = 2, address = new { city = "c" } }));
            var body = await valid.Content.ReadAsStringAsync();
            Assert.AreEqual(HttpStatusCode.OK, valid.StatusCode, body);
            StringAssert.Contains(body, "hosting");
        }

        [TestMethod]
        public async Task StructuralRequestReturnsPlainResponse()
        {
            var result = await http.PostAsync("/stateflows/stateMachines/structuralApi/b/query", Body(new { orderId = "42" }));
            var text = await result.Content.ReadAsStringAsync();
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode, text);
            var response = JsonDocument.Parse(text).RootElement.GetProperty("response");
            Assert.AreEqual("42", response.GetProperty("orderId").GetString(), text);
            Assert.IsFalse(response.TryGetProperty("fields", out _), text);
            Assert.IsFalse(response.TryGetProperty("name", out _), text);

            var links = (await ReadJsonAsync(await http.GetAsync("/stateflows/stateMachines/structuralApi/b/status")))
                .GetProperty("links").EnumerateArray().Select(l => l.GetProperty("rel").GetString()).ToArray();
            CollectionAssert.Contains(links, "submit");
            CollectionAssert.Contains(links, "query");
        }

        [TestMethod]
        public async Task StructuralRequestWithClrResponseReturnsAndDescribesClrResponse()
        {
            var result = await http.PostAsync("/stateflows/stateMachines/structuralApi/c/clrQuery", Body(new { orderId = "7" }));
            var text = await result.Content.ReadAsStringAsync();
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode, text);
            var response = JsonDocument.Parse(text).RootElement.GetProperty("response");
            Assert.AreEqual("7", response.GetProperty("orderId").GetString(), text);
            Assert.AreEqual(2, response.GetProperty("count").GetInt32(), text);

            var document = await GetOpenApiAsync();
            var envelope = ResponseSchema(document, "/stateflows/stateMachines/structuralApi/{instance}/clrQuery");
            var clrSchema = Resolve(document, envelope.GetProperty("properties").GetProperty("response"));
            CollectionAssert.Contains(Required(clrSchema), "orderId", clrSchema.ToString());
            var count = Resolve(document, clrSchema.GetProperty("properties").GetProperty("count"));
            Assert.AreEqual("1", Text(count, "minimum"), count.ToString());
            Assert.AreEqual("5", Text(count, "maximum"), count.ToString());
        }

        [TestMethod]
        public async Task HostedDoActivityStructuralEventIsExposedOnParentStateMachine()
        {
            const string root = "/stateflows/stateMachines/structuralApi/h";
            var submit = await http.PostAsync(root + "/submit", Body(new { orderId = "x", quantity = 1, address = new { city = "c" } }));
            Assert.AreEqual(HttpStatusCode.OK, submit.StatusCode, await submit.Content.ReadAsStringAsync());

            var ping = await http.PostAsync(root + "/hostedPing", Body(new { value = "hello" }));
            var text = await ping.Content.ReadAsStringAsync();
            Assert.IsTrue(ping.StatusCode is HttpStatusCode.OK or HttpStatusCode.Accepted, $"{ping.StatusCode}: {text}");
            for (var i = 0; i < 50 && hostedPingValue == null; i++) await Task.Delay(100);
            Assert.AreEqual("hello", hostedPingValue, text);

            var document = await GetOpenApiAsync();
            var schema = EventSchema(document, "/stateflows/stateMachines/structuralApi/{instance}/hostedPing");
            Assert.IsTrue(schema.GetProperty("properties").TryGetProperty("value", out _), schema.ToString());
        }

        [TestMethod]
        public async Task StructuralActivityAndActionEndpointsAreRegistered()
        {
            var activity = await http.PostAsync("/stateflows/activities/structuralActivity/a/ping", Body(new { value = "v" }));
            var activityText = await activity.Content.ReadAsStringAsync();
            Assert.IsTrue(activity.StatusCode is HttpStatusCode.OK or HttpStatusCode.Accepted, $"{activity.StatusCode}: {activityText}");

            var action = await http.PostAsync("/stateflows/actions/structuralAction/a/ping", Body(new { value = "v" }));
            var actionText = await action.Content.ReadAsStringAsync();
            Assert.AreNotEqual(HttpStatusCode.NotFound, action.StatusCode, actionText);
            Assert.AreNotEqual(HttpStatusCode.MethodNotAllowed, action.StatusCode, actionText);
            Assert.IsTrue(JsonDocument.Parse(actionText).RootElement.TryGetProperty("statusText", out _), actionText);

            var invalid = await http.PostAsync("/stateflows/activities/structuralActivity/a/ping", Body(new { value = 5 }));
            Assert.AreEqual(HttpStatusCode.BadRequest, invalid.StatusCode, await invalid.Content.ReadAsStringAsync());

            var document = await GetOpenApiAsync();
            foreach (var path in new[]
            {
                "/stateflows/activities/structuralActivity/{instance}/ping",
                "/stateflows/actions/structuralAction/{instance}/ping"
            })
            {
                var schema = EventSchema(document, path);
                CollectionAssert.AreEquivalent(new[] { "null", "string" }, Types(schema.GetProperty("properties").GetProperty("value")), path);
            }
        }

        [TestMethod]
        public async Task OpenApiDescribesStructuralFieldSchemas()
        {
            var document = await GetOpenApiAsync();
            var schema = EventSchema(document, "/stateflows/stateMachines/structuralApi/{instance}/submit");
            var properties = schema.GetProperty("properties");
            JsonElement P(string name) => Resolve(document, properties.GetProperty(name));
            var all = schema.ToString();

            CollectionAssert.AreEquivalent(new[] { "orderId", "address" }, Required(schema), all);

            // Required string: non-nullable, non-empty, non-whitespace
            CollectionAssert.AreEqual(new[] { "string" }, Types(P("orderId")), all);
            Assert.AreEqual("1", Text(P("orderId"), "minLength"), all);
            Assert.AreEqual(@"\S", Text(P("orderId"), "pattern"), all);

            // Optional scalars are nullable
            CollectionAssert.AreEqual(new[] { "null", "string" }, Types(P("note")), all);
            CollectionAssert.AreEqual(new[] { "integer", "null" }, Types(P("quantity")), all);
            Assert.AreEqual("int32", Text(P("quantity"), "format"), all);
            Assert.AreEqual("1", Text(P("quantity"), "minimum"), all);
            Assert.AreEqual("10", Text(P("quantity"), "maximum"), all);

            CollectionAssert.AreEqual(new[] { "null", "number" }, Types(P("amount")), all);
            Assert.AreEqual("0.5", Text(P("amount"), "exclusiveMinimum"), all);
            Assert.AreEqual("100", Text(P("amount"), "maximum"), all);

            // Enum including null for optional field
            var kind = P("kind");
            CollectionAssert.AreEqual(new[] { "null", "string" }, Types(kind), all);
            CollectionAssert.AreEqual(new[] { "small", "large", "" },
                kind.GetProperty("enum").EnumerateArray().Select(v => v.ValueKind == JsonValueKind.Null ? "" : v.GetString()).ToArray(), all);

            // Formats
            Assert.AreEqual("byte", Text(P("payload"), "format"), all);
            Assert.AreEqual("date", Text(P("day"), "format"), all);
            Assert.AreEqual("time", Text(P("at"), "format"), all);
            Assert.AreEqual("date-time", Text(P("when"), "format"), all);
            Assert.AreEqual("uuid", Text(P("id"), "format"), all);

            // MaxLength on binary counts bytes, not base64 characters: not represented as maxLength
            Assert.AreEqual("", Text(P("payload"), "maxLength"), all);
            StringAssert.Contains(Text(P("payload"), "description"), "MaxLength", all);

            // Regular expression is anchored (whole value must match), string length mapped
            Assert.AreEqual("^(?:[A-Z]+)$", Text(P("code"), "pattern"), all);
            Assert.AreEqual("2", Text(P("code"), "minLength"), all);
            Assert.AreEqual("8", Text(P("code"), "maxLength"), all);

            var rules = EventSchema(document, "/stateflows/stateMachines/structuralApi/{instance}/rules").GetProperty("properties");
            JsonElement R(string name) => Resolve(document, rules.GetProperty(name));

            // Range on string field cannot be represented
            Assert.AreEqual("", Text(R("label"), "minimum"), all);
            StringAssert.Contains(Text(R("label"), "description"), "Range", all);

            // Custom schema mapping and unmapped custom validator
            Assert.AreEqual("^(..)*$", Text(R("even"), "pattern"), all);
            Assert.AreEqual("", Text(R("even"), "description"), all);
            StringAssert.Contains(Text(R("custom"), "description"), "UnmappedRule", all);

            // Derived built-in attribute may change semantics, so it is reported instead of mapped
            Assert.AreEqual("", Text(R("derived"), "minimum"), R("derived").ToString());
            StringAssert.Contains(Text(R("derived"), "description"), "StrictRange", all);

            // AllowedValues: exact type match maps to enum; mismatched value types (int vs Int64) are not represented
            CollectionAssert.AreEqual(new[] { "s", "m" }, R("size").GetProperty("enum").EnumerateArray().Select(v => v.GetString()).ToArray(), all);
            Assert.IsFalse(R("level").TryGetProperty("enum", out _), all);
            StringAssert.Contains(Text(R("level"), "description"), "AllowedValues", all);

            // Collections: field validation on array, item validation on items; items nullable unless required
            var tags = P("tags");
            CollectionAssert.AreEqual(new[] { "array", "null" }, Types(tags), all);
            Assert.AreEqual("3", Text(tags, "maxItems"), all);
            var tagItems = Resolve(document, tags.GetProperty("items"));
            CollectionAssert.AreEqual(new[] { "null", "string" }, Types(tagItems), all);
            Assert.AreEqual("5", Text(tagItems, "maxLength"), all);

            var scoreItems = Resolve(document, P("scores").GetProperty("items"));
            CollectionAssert.AreEqual(new[] { "integer" }, Types(scoreItems), all);
            Assert.AreEqual("0", Text(scoreItems, "minimum"), all);
            Assert.AreEqual("100", Text(scoreItems, "maximum"), all);

            // Nested objects: required object is non-nullable, optional one nullable, nested required honoured
            var address = P("address");
            CollectionAssert.AreEqual(new[] { "object" }, Types(address), all);
            CollectionAssert.AreEqual(new[] { "city" }, Required(address), all);
            CollectionAssert.AreEqual(new[] { "null", "string" }, Types(Resolve(document, address.GetProperty("properties").GetProperty("zip"))), all);
            var billing = P("billing");
            CollectionAssert.AreEqual(new[] { "null", "object" }, Types(billing), all);
            CollectionAssert.AreEqual(new[] { "city" }, Required(billing), all);

            // Request envelope requires event because it has required fields
            var envelope = Resolve(document, Operation(document, "/stateflows/stateMachines/structuralApi/{instance}/submit")
                .GetProperty("requestBody").GetProperty("content").GetProperty("application/json").GetProperty("schema"));
            CollectionAssert.Contains(Required(envelope), "event");

            // Response envelopes
            var responses = Operation(document, "/stateflows/stateMachines/structuralApi/{instance}/submit").GetProperty("responses");
            foreach (var status in new[] { "200", "201", "202", "400", "404", "409", "500", "503" })
            {
                Assert.IsTrue(responses.TryGetProperty(status, out _), status);
            }
            Assert.IsTrue(ResponseSchema(document, "/stateflows/stateMachines/structuralApi/{instance}/submit", "400")
                .GetProperty("properties").TryGetProperty("eventValidation", out _));
            Assert.IsTrue(responses.GetProperty("200").GetProperty("content").TryGetProperty("text/event-stream", out _));

            // Structural response event
            var queryResponse = Resolve(document, ResponseSchema(document, "/stateflows/stateMachines/structuralApi/{instance}/query")
                .GetProperty("properties").GetProperty("response"));
            Assert.IsTrue(queryResponse.GetProperty("properties").TryGetProperty("orderId", out _), queryResponse.ToString());
        }
    }
}