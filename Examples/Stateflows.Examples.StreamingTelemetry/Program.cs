using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using Stateflows;
using Stateflows.Activities;
using Stateflows.Common;
using Stateflows.Examples.StreamingTelemetry;
using Stateflows.Extensions.MinimalAPIs;

var builder = WebApplication.CreateBuilder(args);
if (!builder.Environment.IsDevelopment())
    throw new InvalidOperationException("This example is development-only: Orleans storage and reminders are in memory.");

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddOpenApi(options => options.AddOperationTransformer((operation, context, cancellationToken) =>
{
    if (operation.Responses is not null)
        foreach (var response in operation.Responses.Values)
            if (response.Content?.TryGetValue("text/event-stream", out var content) == true)
                content.Schema = new OpenApiSchema { Type = JsonSchemaType.String };
    return Task.CompletedTask;
}));
builder.Services.AddProblemDetails();
builder.Services.AddSingleton<IValidator<ConfigureTelemetry>, ConfigureTelemetryValidator>();
builder.Services.AddSingleton<IValidator<TokensInput<SensorSample>>, SensorBatchValidator>();
builder.Services.AddStateflows(stateflows => stateflows
    .UseFullNamesFor(TypedElements.None)
    .AddFluentValidation()
    .AddPlantUml()
    .AddActivities(activities => activities.AddActivity<TelemetryActivity>(TelemetryActivity.Name)));
builder.Host.UseOrleans(silo => silo
    .UseLocalhostClustering(
        siloPort: builder.Configuration.GetValue("Orleans:SiloPort", 11116),
        gatewayPort: builder.Configuration.GetValue("Orleans:GatewayPort", 30006),
        serviceId: "streaming-telemetry-example",
        clusterId: "streaming-telemetry-example")
    .AddMemoryGrainStorageAsDefault()
    .UseInMemoryReminderService()
    .AddStateflowsRuntime());

var app = builder.Build();
app.UseExceptionHandler();
app.MapOpenApi();
app.MapScalarApiReference();
app.MapGet("/", () => Results.Redirect("/scalar")).ExcludeFromDescription();
app.MapStateflowsMinimalAPIsEndpoints(api => api
    .ConfigureActivities(activities => activities
        .ConfigureEventEndpoint<Stateflows.Common.Finalize>(endpoint => endpoint.Disable())
        .ConfigureEventEndpoint<TokensInput<SensorSample>>(endpoint => endpoint.ConfigureHandler(handler => handler
            .WithSummary("Submit samples: sendAsStream=true admits without waiting for processing.")
            .WithDescription("Batch of 1-64 samples. Ordinary input waits for processing; streamed input returns 202 after volatile admission. Open output SSE watches first.")))
        .ConfigureEventEndpoint<TokensOutput<ProcessedReading>>(endpoint => endpoint.ConfigureHandler(handler => handler
            .WithSummary("Watch calibrated readings: stream=true selects SSE.")
            .WithDescription("Without stream, waits for one future batch. With stream, SSE data contains a notification envelope, with readings under payload.tokens[].payload.")
            .WithMetadata(new ProducesResponseTypeMetadata(200, typeof(ActivityInputBody<ProcessedReading>), ["application/json", "text/event-stream"]))))
        .ConfigureEventEndpoint<TokensOutput<TelemetryAlert>>(endpoint => endpoint.ConfigureHandler(handler => handler
            .WithSummary("Watch readings at or above the configured threshold.")
            .WithMetadata(new ProducesResponseTypeMetadata(200, typeof(ActivityInputBody<TelemetryAlert>), ["application/json", "text/event-stream"]))))
        .ConfigureEventEndpoint<TokensOutput<RejectedSample>>(endpoint => endpoint.ConfigureHandler(handler => handler
            .WithSummary("Watch rejected samples, independently of valid readings.")
            .WithMetadata(new ProducesResponseTypeMetadata(200, typeof(ActivityInputBody<RejectedSample>), ["application/json", "text/event-stream"]))))));
app.MapPost("/stateflows/activities/Telemetry/{instance}/checkpoint",
    (string instance, IBehaviorLocator locator) =>
        TelemetryEndpoints.SendAsync(instance, new ActivityCheckpoint(), locator))
    .WithTags("Activity Telemetry").WithSummary("Checkpoint values and network state at a firing-boundary barrier.");
app.MapPost("/stateflows/activities/Telemetry/{instance}/finalize",
    (string instance, IBehaviorLocator locator, FinalizationMode mode = FinalizationMode.Immediate) =>
        TelemetryEndpoints.SendAsync(instance, new Stateflows.Common.Finalize { Mode = mode }, locator))
    .WithTags("Activity Telemetry").WithSummary("Finalize immediately (cancel executing work) or queue behind ordinary events.");
app.Run();

public partial class Program;
