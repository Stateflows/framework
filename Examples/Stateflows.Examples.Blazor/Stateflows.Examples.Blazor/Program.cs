using Medallion.Threading.SqlServer;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry;
using Scalar.AspNetCore;
using Stateflows;
using Stateflows.StateMachines;
using Stateflows.Examples.Blazor.Components;
using Stateflows.Examples.Behaviors.StateMachines.Document;
using Stateflows.Examples.Behaviors.StateMachines.Document.Interceptors;
using Stateflows.Examples.Blazor;
using Stateflows.Extensions.MinimalAPIs;
using Stateflows.Extensions.OpenTelemetry;

var builder = WebApplication.CreateBuilder(args);

// Uncomment, if you want to use storage:
//
builder.Services.AddDbContext<AppDbContext>(options
    => options.UseSqlServer(builder.Configuration.GetConnectionString("Default"))
);

// In order to host Stateflows behaviors, Stateflows framework must be registered in the app.
builder.Services.AddStateflows(b => b
            
    .SetMaxConcurrentBehaviorExecutions(10)
            
    // Each type of behavior must be registered explicitly - in this example only State Machines are used.
    .AddStateMachines(b => b
            
        // Single State Machine, defined in separate C# class, is registered here under the name "Doc".
        // If no name is provided, full name of class would be used as a behavior class name.
        .AddStateMachine<Document>("Doc")
    )
    
    .AddInterceptor<InfoEnhanceInterceptor>()
    
    // Add PlantUML extension to enable State Machines and Activities visualizations.
    .AddPlantUml()

    // Add OpenTelemetry extension to enable tracing and logging.
    .AddOpenTelemetry()

    // Uncomment, if you want to use storage:
    //
    .AddEntityFrameworkCoreStorage<AppDbContext>()
    
    .AddDistributedLock(async (serviceProvider, lockKey)
        => new SqlDistributedLock(lockKey, builder.Configuration.GetConnectionString("Default"))
    )
);

builder.Services.AddOpenApi();

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

app.MapOpenApi();
app.MapScalarApiReference();

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


// API interface must be exposed for WebAssembly to interact with Stateflows
app.MapStateflowsMinimalAPIsEndpoints();

app.Run();