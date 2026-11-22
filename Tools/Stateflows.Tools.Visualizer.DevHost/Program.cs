using System.Text.Json;
using System.Text.Json.Serialization;
using Stateflows;
using Stateflows.Activities;
using Stateflows.Common;
using Stateflows.Examples.StreamingTelemetry;
using Stateflows.Extensions.MinimalAPIs;
using Stateflows.StateMachines;
using Stateflows.Tools.Visualizer.DevHost;

var builder = WebApplication.CreateBuilder(args);
if (!builder.Environment.IsDevelopment())
    throw new InvalidOperationException("The visualizer host is development-only and uses volatile storage.");
builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddProblemDetails();
builder.Services.AddSingleton<DefinitionCatalog>();
builder.Services.AddSingleton<Capture>();
builder.Services.AddSingleton<DebugSession>();
builder.Services.AddStateflows(stateflows => stateflows
    .UseFullNamesFor(TypedElements.None)
#if LIFECARE_MODELS
    .AddResource(Lifecare_Clearing.Api.StateMachines.ProcessBase.ProcessBase.StateMachineResourceName, _ => { })
#endif
    .AddPlantUml()
    .AddStateMachines(machines => machines
        .AddObserver<MachineObserver>()
        .AddStateMachine("ReviewProcess", SampleModels.BuildMachine)
#if LIFECARE_MODELS
        .AddStateMachine<Lifecare_Clearing.Api.StateMachines.AssistanceHoursProcess.AssistanceHoursProcess>()
        .AddStateMachine<Lifecare_Clearing.Api.StateMachines.BackgroundCleaner.BackgroundCleanerStateMachine>()
        .AddStateMachine<Lifecare_Clearing.Api.StateMachines.BinderProcess.BinderProcess>()
        .AddStateMachine<Lifecare_Clearing.Api.StateMachines.DeviationProcess.DeviationProcess>()
        .AddStateMachine<Lifecare_Clearing.Api.StateMachines.DossierProcess.DossierProcess>()
        .AddStateMachine<Lifecare_Clearing.Api.StateMachines.ExecutionsProcess.ExecutionsProcess>()
        .AddStateMachine<Lifecare_Clearing.Api.StateMachines.FeesProcess.FeesProcess>()
        .AddStateMachine<Lifecare_Clearing.Api.StateMachines.HSL.DemandProcess.DemandForCareProcess>()
        .AddStateMachine<Lifecare_Clearing.Api.StateMachines.HSL.PatientJournalProcess.PatientJournalProcess>()
        .AddStateMachine<Lifecare_Clearing.Api.StateMachines.Identification.Identification>()
        .AddStateMachine<Lifecare_Clearing.Api.StateMachines.PersonalProcess.PersonalProcess>()
#endif
    )
    .AddActivities(activities => activities
        .AddObserver<NetworkObserver>()
        .AddActivity("NestedCalculation", SampleModels.BuildActivity)
        .AddActivity<TelemetryActivity>(TelemetryActivity.Name)));
builder.Host.UseOrleans(silo => silo
    .UseLocalhostClustering(11127, 30017, serviceId: "visualizer-development", clusterId: "visualizer-development")
    .AddMemoryGrainStorageAsDefault()
    .UseInMemoryReminderService()
    .AddStateflowsRuntime());
var app = builder.Build();
await app.Services.GetRequiredService<DefinitionCatalog>().LoadAsync(app.Services);
app.UseExceptionHandler();
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapGet("/api/definitions", (DefinitionCatalog catalog) => Results.Ok(new { catalog.Machines, catalog.Activities, catalog.Actions }));
app.MapPost("/api/debug/breakpoints", (BreakpointChange change, DebugSession debugger) =>
{
    var error = debugger.Validate(change);
    if (error is not null) return Results.BadRequest(new { error });
    debugger.Change(change);
    return Results.Ok(debugger.Read(change.Scope));
});
app.MapPost("/api/debug/resume/{pauseId:guid}", (Guid pauseId, DebugSession debugger) =>
    debugger.Resume(pauseId) ? Results.NoContent() : Results.NotFound(new { error = "The invocation is no longer paused." }));
app.MapGet("/api/debug/stream", async (HttpContext http, DebugSession debugger,
    string family, string name, int version, string instance) =>
{
    http.Response.ContentType = "text/event-stream";
    http.Response.Headers.CacheControl = "no-cache";
    var scope = new DebugScope(family, name, version, instance);
    var json = new JsonSerializerOptions(JsonSerializerDefaults.Web);
    long revision = -1;
    try
    {
        while (!http.RequestAborted.IsCancellationRequested)
        {
            var snapshot = debugger.Read(scope);
            if (snapshot.Revision != revision)
            {
                await http.Response.WriteAsync($"data: {JsonSerializer.Serialize(snapshot, json)}\n\n", http.RequestAborted);
                revision = snapshot.Revision;
            }
            else await http.Response.WriteAsync(": heartbeat\n\n", http.RequestAborted);
            await http.Response.Body.FlushAsync(http.RequestAborted);
            await Task.Delay(250, http.RequestAborted);
        }
    }
    catch (OperationCanceledException) when (http.RequestAborted.IsCancellationRequested) { }
});
app.MapGet("/api/capture", (Capture capture, long after = 0) => capture.Read(after));
app.MapGet("/api/capture/stream", async (HttpContext http, Capture capture, long after = 0) =>
{
    if (long.TryParse(http.Request.Headers["Last-Event-ID"], out var lastEventId))
        after = Math.Max(after, lastEventId);
    http.Response.ContentType = "text/event-stream";
    http.Response.Headers.CacheControl = "no-cache";
    var json = new JsonSerializerOptions(JsonSerializerDefaults.Web);
    try
    {
        while (!http.RequestAborted.IsCancellationRequested)
        {
            var batch = capture.Read(after);
            if (batch.Latest > after)
            {
                await http.Response.WriteAsync($"id: {batch.Latest}\ndata: {JsonSerializer.Serialize(batch, json)}\n\n", http.RequestAborted);
                await http.Response.Body.FlushAsync(http.RequestAborted);
                after = batch.Latest;
            }
            else
            {
                await http.Response.WriteAsync(": heartbeat\n\n", http.RequestAborted);
                await http.Response.Body.FlushAsync(http.RequestAborted);
            }
            await Task.Delay(500, http.RequestAborted);
        }
    }
    catch (OperationCanceledException) when (http.RequestAborted.IsCancellationRequested) { }
});
app.MapPost("/api/demo/machine/{instance}/{command}", async (string instance, string command, IStateMachineLocator locator, DefinitionCatalog catalog) =>
{
    if (!locator.TryLocateStateMachine(new StateMachineId("ReviewProcess", instance), out var machine))
        return Results.NotFound();
    using (machine)
    {
        SendResult result;
        switch (command)
        {
            case "initialize": result = await machine.SendAsync(new Initialize()); break;
            case "approve": result = await machine.SendAsync(new Approve(true)); break;
            case "reject": result = await machine.SendAsync(new Approve(false)); break;
            case "finish": result = await machine.SendAsync(new Finish()); break;
            case "cancel": result = await machine.SendAsync(new Cancel()); break;
            case "synchronize": result = await machine.SendAsync(new Synchronize()); break;
            case "resume": result = await machine.SendAsync(new Resume()); break;
            case "refresh": result = await machine.SendAsync(new Refresh()); break;
            default: return Results.BadRequest(new { error = "Unknown development command." });
        }
        var snapshot = await machine.GetStatusAsync();
        var stateIds = catalog.Machines.Single(model => model.Name == "ReviewProcess" && model.Version == 1)
            .States.ToDictionary(state => state.Name, state => state.Id);
        return Results.Ok(new { result.Status, SnapshotStatus = snapshot.Status, Snapshot = snapshot.Response,
            ActiveIds = snapshot.Response?.CurrentStates.GetAllNodes().Select(node => stateIds[node.Value]).ToArray() });
    }
});
app.MapPost("/api/demo/activity/{instance}", async (string instance, IActivityLocator locator) =>
{
    if (!locator.TryLocateActivity(new ActivityId("NestedCalculation", instance), out var activity))
        return Results.NotFound();
    using (activity)
        return Results.Ok(await activity.SendAsync(new Initialize()));
});
app.MapPost("/api/demo/telemetry/{instance}/{command}", async (string instance, string command, IActivityLocator locator) =>
{
    if (!locator.TryLocateActivity(new ActivityId(TelemetryActivity.Name, instance), out var activity))
        return Results.NotFound();
    using (activity)
    {
        SendResult result;
        switch (command)
        {
            case "initialize": result = await activity.SendAsync(new Initialize()); break;
            case "configure": result = await activity.SendAsync(ConfigureTelemetry.Default); break;
            case "samples":
                var now = DateTimeOffset.UtcNow;
                result = await activity.SendAsync(new TokensInput().Add(
                    new SensorSample(Guid.NewGuid().ToString("N"), "demo-sensor", 75m, now)).Add(
                    new SensorSample(Guid.NewGuid().ToString("N"), "demo-sensor", 20m, now)));
                break;
            default: return Results.BadRequest(new { error = "Unknown development command." });
        }
        return Results.Ok(new { result.Status });
    }
});
app.MapStateflowsMinimalAPIsEndpoints(api => api.SetApiRoutePrefix("api/stateflows"));
app.Run();
