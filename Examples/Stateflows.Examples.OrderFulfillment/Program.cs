using System.Text.Json.Serialization;
using FluentValidation;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Scalar.AspNetCore;
using Stateflows;
using Stateflows.Common;
using Stateflows.Examples.OrderFulfillment;
using Stateflows.Extensions.MinimalAPIs;
using Stateflows.StateMachines;

var builder = WebApplication.CreateBuilder(args);

if (!builder.Environment.IsDevelopment())
    throw new InvalidOperationException(
        "This example is development-only: storage, reminders and provider simulators are in memory.");

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddTransient<IOrderSimulator, OrderSimulator>();
builder.Services.AddSingleton<IValidator<SubmitOrder>, SubmitOrderValidator>();
builder.Services.AddSingleton<IValidator<CancelOrder>, CancelOrderValidator>();
builder.Services.AddStateflows(stateflows => stateflows
    .UseFullNamesFor(TypedElements.None)
    .AddFluentValidation()
    .AddPlantUml()
    .AddOpenTelemetry()
    .AddStateMachines(machines => machines.AddStateMachine<OrderFulfillment>(OrderFulfillment.Name)));

builder.Host.UseOrleans(silo => silo
    .UseLocalhostClustering(
        siloPort: builder.Configuration.GetValue("Orleans:SiloPort", 11115),
        gatewayPort: builder.Configuration.GetValue("Orleans:GatewayPort", 30005),
        serviceId: "order-fulfillment-example",
        clusterId: "order-fulfillment-example")
    .AddMemoryGrainStorageAsDefault()
    .UseInMemoryReminderService()
    .AddStateflowsRuntime());

var telemetry = builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("Stateflows.OrderFulfillment"));
telemetry.WithTracing(tracing =>
{
    tracing.AddAspNetCoreInstrumentation();
    if (string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]))
        tracing.AddConsoleExporter();
    else
        tracing.AddOtlpExporter();
});
telemetry.WithMetrics(metrics =>
{
    if (string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]))
        metrics.AddConsoleExporter();
    else
        metrics.AddOtlpExporter();
});

var app = builder.Build();
app.UseExceptionHandler();
app.MapOpenApi();
app.MapScalarApiReference();
app.MapGet("/", () => Results.Redirect("/scalar"));
app.MapStateflowsMinimalAPIsEndpoints(api => api
    .SetApiRoutePrefix("stateflows")
    .ConfigureStateMachines(machines => machines
        .ConfigureEventEndpoints<Stateflows.Common.Finalize, Reset>(endpoint => endpoint.Disable()))
    .ConfigureActivities(activities => activities.ConfigureAllEndpoints(endpoint => endpoint.Disable()))
    .ConfigureActions(actions => actions.ConfigureAllEndpoints(endpoint => endpoint.Disable()))
    .ConfigureEntities(entities => entities.ConfigureAllEndpoints(endpoint => endpoint.Disable())));
app.Run();

public partial class Program;
