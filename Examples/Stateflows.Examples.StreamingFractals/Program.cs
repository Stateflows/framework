using System.Text.Json.Serialization;
using FluentValidation;
using Stateflows;
using Stateflows.Activities;
using Stateflows.Common;
using Stateflows.Examples.StreamingFractals;
using Stateflows.Extensions.MinimalAPIs;
using Stateflows.Extensions.PlantUml.Events;

var builder = WebApplication.CreateBuilder(args);
if (!builder.Environment.IsDevelopment())
    throw new InvalidOperationException("StreamingFractals is development-only: no authentication or durable storage is configured.");

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddProblemDetails();
builder.Services.AddSingleton<IJuliaTileRenderer>(services => RenderingOptions.Create(
    services.GetRequiredService<IConfiguration>(), services.GetRequiredService<ILoggerFactory>()));
builder.Services.AddSingleton<IValidator<ConfigureRendering>, ConfigureRenderingValidator>();
builder.Services.AddSingleton<IValidator<TokensInput<TileRequest>>, TileBatchValidator>();
builder.Services.AddStateflows(stateflows => stateflows
    .UseFullNamesFor(TypedElements.None)
    .AddFluentValidation()
    .AddPlantUml()
    .AddActivities(activities => activities.AddActivity<FractalActivity>(FractalActivity.Name)));
builder.Host.UseOrleans(silo => silo
    .UseLocalhostClustering(
        siloPort: builder.Configuration.GetValue("Orleans:SiloPort", 11118),
        gatewayPort: builder.Configuration.GetValue("Orleans:GatewayPort", 30008),
        serviceId: "streaming-fractals-example", clusterId: "streaming-fractals-example")
    .AddMemoryGrainStorageAsDefault()
    .UseInMemoryReminderService()
    .AddStateflowsRuntime());

var app = builder.Build();
// Fail before accepting traffic if the explicitly selected CUDA backend cannot initialize.
_ = app.Services.GetRequiredService<IJuliaTileRenderer>();
app.UseExceptionHandler();
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapGet("/api/fractal", () => new
{
    OutputEvent = Event<TileResult>.Name,
    ActivityPath = $"/stateflows/activities/{FractalActivity.Name}",
    DefaultIterations = ConfigureRendering.DefaultIterations
});
app.MapStateflowsMinimalAPIsEndpoints(api => api.ConfigureActivities(activities => activities
    .ConfigureEventEndpoint<Stateflows.Common.Finalize>(endpoint => endpoint.Disable())));
app.MapPost("/api/sessions/{instance}/stop", async (string instance, IBehaviorLocator locator) =>
{
    if (!locator.TryLocateBehavior(new ActivityId(FractalActivity.Name, instance), out var behavior))
        return Results.NotFound();
    using (behavior)
    {
        var result = await behavior.SendAsync(new Stateflows.Common.Finalize { Mode = FinalizationMode.Immediate });
        if (result.Status is not (EventStatus.Consumed or EventStatus.Initialized or EventStatus.Cancelled))
            return Results.Problem($"Cannot stop Activity ({result.Status}).", statusCode: 409);
        var reset = await behavior.ResetAsync();
        return reset.Status == EventStatus.Consumed
            ? Results.Ok(new { Status = reset.Status })
            : Results.Problem($"Activity stopped, but reset failed ({reset.Status}).", statusCode: 409);
    }
});
app.MapGet("/api/sessions/{instance}/diagram", async (string instance, IBehaviorLocator locator) =>
{
    if (!locator.TryLocateBehavior(new ActivityId(FractalActivity.Name, instance), out var behavior))
        return Results.NotFound();
    using (behavior)
    {
        var result = await behavior.RequestAsync<PlantUmlInfo>(new PlantUmlInfoRequest());
        return result.Status == EventStatus.Consumed && result.Response is not null
            ? Results.Text(result.Response.PlantUml, "text/plain")
            : Results.Problem($"Diagram unavailable ({result.Status}).", statusCode: 409);
    }
});
app.Run();

public partial class Program;
