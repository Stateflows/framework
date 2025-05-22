using OpenTelemetry;
using OpenTelemetry.Trace;
using Stateflows;
using Stateflows.StateMachines;
using Stateflows.Examples.Blazor.Components;
using Stateflows.Examples.Behaviors.StateMachines.Document;
using Stateflows.Extensions.OpenTelemetry;
using Stateflows.Transport.Http;

var builder = WebApplication.CreateBuilder(args);

// In order to host Stateflows behaviors, Stateflows framework must be registered in the app.
builder.Services.AddStateflows(b => b
    
    // Each type of behavior must be registered explicitly - in this example only State Machines are used.
    .AddStateMachines(b => b
            
        // Single State Machine, defined in separate C# class, is registered here under the name "Doc".
        // If no name is provided, full name of class would be used as a behavior class name.
        .AddStateMachine<Document>("Doc")
    )
    
    // Add PlantUML extension to enable State Machines and Activities visualizations.
    .AddPlantUml()

    // Add OpenTelemetry extension to enable tracing and logging.
    .AddOpenTelemetry()
);


#region OpenTelemetry
// Setup logging to be exported via OpenTelemetry
builder.Logging.AddOpenTelemetry(logging =>
{
    logging.IncludeFormattedMessage = true;
    logging.IncludeScopes = true;
});

var otel = builder.Services.AddOpenTelemetry();

// Add Tracing for ASP.NET Core and our custom ActivitySource and export via OTLP
otel.WithTracing();

// Export OpenTelemetry data via OTLP, using env vars for the configuration
var OtlpEndpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
if (OtlpEndpoint != null)
{
    otel.UseOtlpExporter();
}
#endregion

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Stateflows.Examples.Blazor.Client._Imports).Assembly);


// For WebAssembly to interact with Stateflows behaviors, transport layer must be configured.
app.MapStateflowsHttpTransport();

app.Run();