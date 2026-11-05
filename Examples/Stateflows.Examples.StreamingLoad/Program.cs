using Stateflows;
using Stateflows.Activities;
using Stateflows.Examples.StreamingLoad;
using Stateflows.Examples.StreamingLoad.Components;

var builder = WebApplication.CreateBuilder(args);
if (!builder.Environment.IsDevelopment())
    throw new InvalidOperationException("StreamingLoad is development-only: it has shared controls, no authentication, and in-memory storage.");

builder.Services.AddRazorComponents().AddInteractiveServerComponents();
var limits = builder.Configuration.GetSection("SafetyLimits").Get<SafetyLimits>() ?? new SafetyLimits();
limits.Validate();
builder.Services.AddSingleton(limits);
builder.Services.AddSingleton<WorkerPool>();
builder.Services.AddSingleton<LoadMetrics>();
builder.Services.AddSingleton<LoadCoordinator>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<LoadCoordinator>());
builder.Services.AddStateflows(stateflows => stateflows.UseFullNamesFor(TypedElements.None)
    .AddActivities(activities => activities.AddActivity<LoadActivity>(LoadActivity.Name)));
builder.Host.UseOrleans(silo => silo
    .UseLocalhostClustering(
        siloPort: builder.Configuration.GetValue("Orleans:SiloPort", 11117),
        gatewayPort: builder.Configuration.GetValue("Orleans:GatewayPort", 30007),
        serviceId: "streaming-load-example", clusterId: "streaming-load-example")
    .AddMemoryGrainStorageAsDefault()
    .UseInMemoryReminderService()
    .AddStateflowsRuntime());
var app = builder.Build();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();

public partial class Program;
