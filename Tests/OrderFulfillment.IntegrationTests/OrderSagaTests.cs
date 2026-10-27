using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Net;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Stateflows.Examples.OrderFulfillment;

namespace OrderFulfillment.IntegrationTests;

[TestClass]
[DoNotParallelize]
public sealed class OrderSagaTests
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };
    private static WebApplicationFactory<Program> factory = null!;
    private static HttpClient client = null!;

    [ClassInitialize]
    public static void Start(TestContext _)
    {
        var ports = FreePorts();
        factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder => builder
            .UseEnvironment("Development")
            .UseSetting("Orleans:SiloPort", ports[0].ToString())
            .UseSetting("Orleans:GatewayPort", ports[1].ToString()));
        client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.Timeout = TimeSpan.FromSeconds(30);
    }

    [ClassCleanup]
    public static async Task Stop()
    {
        client?.Dispose();
        if (factory is not null) await factory.DisposeAsync();
    }

    [TestMethod]
    [DataRow(Scenario.HappyPath)]
    [DataRow(Scenario.TransientFailures)]
    public async Task SuccessfulOrdersCompleteWithInvoiceAndIsolatedData(Scenario scenario)
    {
        var id = await SubmitAsync(scenario);
        var snapshot = await WaitAsync(id, "Completed");
        Assert.AreEqual(PaymentState.Captured, snapshot.Payment);
        Assert.AreEqual(StockState.Dispatched, snapshot.Stock);
        Assert.AreEqual(ShipmentState.Delivered, snapshot.Shipment);
        Assert.AreEqual(25m, snapshot.Invoice?.Total);
        Assert.AreEqual($"INV-{id}", snapshot.Invoice?.Number);
        foreach (var operation in Enum.GetValues<Operation>().Take(6))
        {
            var attempts = snapshot.Attempts.Where(attempt => attempt.Operation == operation).ToArray();
            Assert.AreEqual(scenario == Scenario.TransientFailures ? 2 : 1, attempts.Length, operation.ToString());
            Assert.AreEqual("Succeeded", attempts[^1].Outcome);
        }
        Assert.IsTrue(snapshot.Audit.Any(entry => entry.Message.Contains("Packing and labelling 2 units")));
        var another = await SubmitAsync(Scenario.HappyPath);
        Assert.AreNotEqual(id, another);
        Assert.AreEqual($"INV-{another}", (await WaitAsync(another, "Completed")).Invoice?.Number);
        Assert.AreEqual($"INV-{id}", (await SummaryAsync(id)).Invoice?.Number);
    }

    [TestMethod]
    [DataRow(Scenario.PaymentDeclined)]
    [DataRow(Scenario.OutOfStock)]
    [DataRow(Scenario.PreparationFailed)]
    [DataRow(Scenario.CaptureFailed)]
    [DataRow(Scenario.DispatchFailed)]
    public async Task FailuresCompensateOnlyAcquiredResources(Scenario scenario)
    {
        var id = await SubmitAsync(scenario);
        var snapshot = await WaitAsync(id, "Failed");
        Assert.IsNotNull(snapshot.FailureReason);
        Assert.IsTrue(snapshot.Audit.Any(entry => entry.Message == snapshot.FailureReason));
        Assert.IsFalse(snapshot.CancellationRequested);
        Assert.IsTrue(snapshot.Payment is PaymentState.None or PaymentState.Voided or PaymentState.Refunded);
        Assert.IsTrue(snapshot.Stock is StockState.None or StockState.Released);
        Assert.IsNull(snapshot.Invoice);
        Assert.IsTrue(snapshot.Attempts.GroupBy(attempt => attempt.Operation)
            .All(group => group.Count() <= OrderSimulator.MaximumAttempts));
        if (scenario == Scenario.CaptureFailed)
            Assert.AreEqual(3, snapshot.Attempts.Count(attempt => attempt.Operation == Operation.CapturePayment));
        if (scenario == Scenario.DispatchFailed)
        {
            Assert.AreEqual(PaymentState.Refunded, snapshot.Payment);
            Assert.AreEqual(StockState.Released, snapshot.Stock);
        }
        await Task.Delay(350);
        var later = await SummaryAsync(id);
        Assert.AreEqual(snapshot.Payment, later.Payment);
        Assert.AreEqual(snapshot.Stock, later.Stock);
        Assert.AreEqual("Failed", later.Stage);
    }

    [TestMethod]
    [DataRow("Checking")]
    [DataRow("PreparingShipment")]
    [DataRow("CapturingPayment")]
    [DataRow("DispatchingShipment")]
    public async Task CancellationIsSafeInEveryPreDispatchPhase(string stage)
    {
        var id = await SubmitAsync(Scenario.HappyPath, 400);
        await WaitAsync(id, stage);
        using var response = await PostAsync(id, "cancelOrder", new CancelOrder("Customer changed their mind."));
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());
        var snapshot = await WaitAsync(id, "Cancelled");
        Assert.IsTrue(snapshot.CancellationRequested);
        Assert.AreEqual("Customer changed their mind.", snapshot.FailureReason);
        Assert.IsTrue(snapshot.Payment is PaymentState.None or PaymentState.Voided or PaymentState.Refunded);
        Assert.IsTrue(snapshot.Stock is StockState.None or StockState.Released);
        Assert.AreNotEqual(ShipmentState.Dispatched, snapshot.Shipment);
        Assert.IsNull(snapshot.Invoice);
        await Task.Delay(600);
        Assert.AreEqual("Cancelled", (await SummaryAsync(id)).Stage);
    }

    [TestMethod]
    public async Task DeliveryNeedsOperatorRetryAndRejectsLateCancellation()
    {
        var id = await SubmitAsync(Scenario.DeliveryNeedsAttention);
        var snapshot = await WaitAsync(id, "NeedsAttention");
        Assert.AreEqual(PaymentState.Captured, snapshot.Payment);
        Assert.AreEqual(StockState.Dispatched, snapshot.Stock);
        Assert.AreEqual(ShipmentState.Dispatched, snapshot.Shipment);
        Assert.AreEqual(3, snapshot.Attempts.Count(attempt => attempt.Operation == Operation.DeliverShipment));
        using var cancel = await PostAsync(id, "cancelOrder", new CancelOrder("Too late."));
        Assert.AreEqual(HttpStatusCode.Conflict, cancel.StatusCode);
        using var retry = await PostAsync(id, "retryDelivery", new RetryDelivery());
        Assert.AreEqual(HttpStatusCode.OK, retry.StatusCode);
        snapshot = await WaitAsync(id, "Completed");
        Assert.AreEqual(4, snapshot.Attempts.Count(attempt => attempt.Operation == Operation.DeliverShipment));
        Assert.IsFalse(snapshot.Attempts.Any(attempt => attempt.Operation == Operation.RefundPayment));
    }

    [TestMethod]
    public async Task CompensationFailureRemainsVisibleUntilOperatorRetries()
    {
        var id = await SubmitAsync(Scenario.CompensationNeedsAttention);
        var snapshot = await WaitAsync(id, "CompensationNeedsAttention");
        Assert.AreEqual(PaymentState.Captured, snapshot.Payment);
        Assert.AreEqual(StockState.Reserved, snapshot.Stock);
        using var response = await PostAsync(id, "retryCompensation", new RetryCompensation());
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        snapshot = await WaitAsync(id, "Failed");
        Assert.AreEqual(PaymentState.Refunded, snapshot.Payment);
        Assert.AreEqual(StockState.Released, snapshot.Stock);
        Assert.AreEqual(4, snapshot.Attempts.Count(attempt => attempt.Operation == Operation.RefundPayment));
    }

    [TestMethod]
    public async Task ConcurrentOrdersDoNotShareEntityData()
    {
        var ids = await Task.WhenAll(SubmitAsync(Scenario.HappyPath), SubmitAsync(Scenario.DispatchFailed));
        var snapshots = await Task.WhenAll(WaitAsync(ids[0], "Completed"), WaitAsync(ids[1], "Failed"));
        Assert.AreEqual(Scenario.HappyPath, snapshots[0].Order?.Scenario);
        Assert.AreEqual(Scenario.DispatchFailed, snapshots[1].Order?.Scenario);
        Assert.AreEqual(PaymentState.Captured, snapshots[0].Payment);
        Assert.AreEqual(PaymentState.Refunded, snapshots[1].Payment);
        Assert.AreEqual($"INV-{ids[0]}", snapshots[0].Invoice?.Number);
        Assert.IsNull(snapshots[1].Invoice);
    }

    [TestMethod]
    public async Task CancellationRacingDispatchHasOnlyOneBusinessOutcome()
    {
        for (var iteration = 0; iteration < 5; iteration++)
        {
            var id = await SubmitAsync(Scenario.HappyPath);
            await WaitAsync(id, "DispatchingShipment");
            await Task.Delay(90);
            using var cancel = await PostAsync(id, "cancelOrder", new CancelOrder("Dispatch boundary race."));
            Assert.IsTrue(cancel.StatusCode is HttpStatusCode.OK or HttpStatusCode.Conflict);
            var snapshot = await WaitAsync(id, cancel.StatusCode == HttpStatusCode.OK ? "Cancelled" : "Completed");
            if (snapshot.Stage == "Cancelled")
            {
                Assert.AreEqual(PaymentState.Refunded, snapshot.Payment);
                Assert.AreEqual(StockState.Released, snapshot.Stock);
                Assert.AreNotEqual(ShipmentState.Dispatched, snapshot.Shipment);
            }
            else
            {
                Assert.AreEqual(PaymentState.Captured, snapshot.Payment);
                Assert.AreEqual(StockState.Dispatched, snapshot.Stock);
            }
        }
    }

    [TestMethod]
    public async Task TelemetryCoversAllFourBehaviorKindsAndExecutionMetrics()
    {
        var spans = new ConcurrentQueue<string>();
        var meters = new ConcurrentDictionary<string, bool>();
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == "Stateflows",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = activity => spans.Enqueue(activity.DisplayName)
        };
        ActivitySource.AddActivityListener(listener);
        using var meterListener = new MeterListener
        {
            InstrumentPublished = (instrument, _) => meters.TryAdd(instrument.Meter.Name, true)
        };
        meterListener.Start();
        await WaitAsync(await SubmitAsync(Scenario.HappyPath), "Completed");
        foreach (var prefix in new[] { "State Machine '", "Activity '", "Action '", "Entity '" })
            Assert.IsTrue(spans.Any(span => span.StartsWith(prefix)), $"Missing {prefix} trace.");
        Assert.IsTrue(meters.ContainsKey("Stateflows"));
    }

    [TestMethod]
    public async Task OwnerPublishesSharedEntityProjectionNotifications()
    {
        var id = await SubmitAsync(Scenario.HappyPath);
        await WaitAsync(id, "Completed");
        using var response = await client.GetAsync(
            $"{Root(id)}/notifications?names=OrderSnapshot&period=00:01:00");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var notifications = await response.Content.ReadAsStringAsync();
        StringAssert.Contains(notifications, "buyer@example.test");
        StringAssert.Contains(notifications, $"INV-{id}");
    }

    [TestMethod]
    public async Task InvalidCommandsNeverStartWorkAndDuplicateSubmissionIsRejected()
    {
        foreach (var command in new[]
        {
            new SubmitOrder("invalid", [new("SKU", 1, 10m)]),
            new SubmitOrder("buyer@example.test", []),
            new SubmitOrder("buyer@example.test", [new("SKU", 0, 10m)]),
            new SubmitOrder("buyer@example.test", [new("SKU", 1, 0m)]),
            new SubmitOrder("buyer@example.test", [new("SKU", 1, 1.001m)]),
            new SubmitOrder("buyer@example.test", [new("SKU", 1, 10m), new("sku", 1, 10m)]),
            new SubmitOrder("buyer@example.test", [new("SKU", 1, 10m)], (Scenario)999),
            new SubmitOrder("buyer@example.test", [new("SKU", 1, 10m)], CallbackDelayMilliseconds: 0),
            new SubmitOrder("buyer@example.test", null!),
            new SubmitOrder("buyer@example.test", [null!])
        })
        {
            using var invalid = await PostAsync(Guid.NewGuid().ToString("N"), "submitOrder", command);
            Assert.AreEqual(HttpStatusCode.BadRequest, invalid.StatusCode, await invalid.Content.ReadAsStringAsync());
        }

        var id = await SubmitAsync(Scenario.HappyPath);
        using var duplicate = await PostAsync(id, "submitOrder", Command(Scenario.HappyPath));
        Assert.AreEqual(HttpStatusCode.Conflict, duplicate.StatusCode);
        using var invalidCancel = await PostAsync(id, "cancelOrder", new CancelOrder(""));
        Assert.AreEqual(HttpStatusCode.BadRequest, invalidCancel.StatusCode);
        await WaitAsync(id, "Completed");
    }

    [TestMethod]
    public async Task ScalarOpenApiAndPlantUmlAreAvailableWithoutExposingWorkerCommands()
    {
        using var scalar = await client.GetAsync("/scalar/v1");
        Assert.AreEqual(HttpStatusCode.OK, scalar.StatusCode);
        using var openApi = await client.GetAsync("/openapi/v1.json");
        Assert.AreEqual(HttpStatusCode.OK, openApi.StatusCode);
        using var document = JsonDocument.Parse(await openApi.Content.ReadAsStringAsync());
        var routes = document.RootElement.GetProperty("paths").EnumerateObject().Select(path => path.Name).ToArray();
        Assert.IsTrue(routes.Any(route => route.EndsWith("/submitOrder")));
        Assert.IsFalse(routes.Any(route => route.Contains("/stepSucceeded") || route.Contains("/stepFailed")));
        Assert.IsFalse(routes.Any(route => route.EndsWith("/finalize")));
        Assert.IsFalse(document.RootElement.GetProperty("paths").EnumerateObject()
            .Any(path => path.Value.TryGetProperty("delete", out _)));
        Assert.IsFalse(routes.Any(route => route.Contains("/activities/") || route.Contains("/actions/") ||
            route.Contains("/entities/")));
        var id = await SubmitAsync(Scenario.HappyPath);
        await WaitAsync(id, "Completed");
        foreach (var diagram in new[] { "diagram", "diagrams/reservation", "diagrams/preparation" })
        {
            using var response = await client.GetAsync($"{Root(id)}/{diagram}");
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());
            StringAssert.Contains(await response.Content.ReadAsStringAsync(), "@startuml");
        }
    }

    private static SubmitOrder Command(Scenario scenario, int delay = 100) =>
        new("buyer@example.test", [new("BOOK", 2, 12.50m)], scenario, delay);

    private static async Task<string> SubmitAsync(Scenario scenario, int delay = 100)
    {
        var id = Guid.NewGuid().ToString("N");
        using var response = await PostAsync(id, "submitOrder", Command(scenario, delay));
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode, await response.Content.ReadAsStringAsync());
        return id;
    }

    private static Task<HttpResponseMessage> PostAsync<T>(string id, string command, T payload) =>
        client.PostAsJsonAsync($"{Root(id)}/{command}", new { @event = payload }, Json);

    private static string Root(string id) => $"/stateflows/stateMachines/OrderFulfillment/{id}";

    private static async Task<OrderSnapshot> SummaryAsync(string id)
    {
        using var response = await client.GetAsync($"{Root(id)}/summary");
        Assert.IsTrue(response.IsSuccessStatusCode,
            $"Summary returned {response.StatusCode}: {await response.Content.ReadAsStringAsync()}; " +
            $"status: {await client.GetStringAsync($"{Root(id)}/status")}");
        return await response.Content.ReadFromJsonAsync<OrderSnapshot>(Json)
            ?? throw new InvalidOperationException("The order summary was empty.");
    }

    private static async Task<OrderSnapshot> WaitAsync(string id, string stage)
    {
        var timeout = Stopwatch.StartNew();
        OrderSnapshot? last = null;
        while (timeout.Elapsed < TimeSpan.FromSeconds(25))
        {
            last = await SummaryAsync(id);
            if (last.Stage == stage) return last;
            await Task.Delay(25);
        }

        Assert.Fail($"Order {id} did not reach {stage}. Last snapshot: {JsonSerializer.Serialize(last, Json)}");
        throw new InvalidOperationException("Unreachable.");
    }

    private static int[] FreePorts()
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
