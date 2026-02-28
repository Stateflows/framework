using System.Diagnostics;
using Microsoft.ClearScript;
using Microsoft.ClearScript.V8;
using OpenTelemetry;
using Scalar.AspNetCore;
using Stateflows;
using Stateflows.Actions;
using Stateflows.StateMachines;
using Stateflows.Activities;
using Stateflows.Common;
using Stateflows.Common.Utilities;
using Stateflows.Examples.Behaviors.StateMachines.Document.Interceptors;
using Stateflows.Extensions.MinimalAPIs;
using Stateflows.Extensions.OpenTelemetry;
using Document = Stateflows.Examples.Behaviors.StateMachines.Document.Document;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseOrleans(static b => b
    .UseLocalhostClustering()
    .AddMemoryGrainStorage("stateflows")
    .AddMemoryGrainStorage("stateflows")
    .UseInMemoryReminderService()
);

// In order to host Stateflows behaviors, Stateflows framework must be registered in the app.
builder.Services.AddStateflows(b => b
    .AddResource("heavy-work", b => b
        .SetMaxConcurrentBehaviorExecutions(3)
    )
    
    .AddOrleansHosting()
    
    .AddClearScript(_ => Task.FromResult<IScriptEngine>(new V8ScriptEngine()))
    
    .AddActions(b => b
        .AddAction_ClearScript("script", "Console.WriteLine('test');")
        .AddAction("serialize", async c =>
        {
            var h = new TokenHolder<int> { Payload = 42 };
            Debug.WriteLine(StateflowsJsonConverter.SerializePolymorphicObject(h));
        })
    )
        
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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.MapStateflowsMinimalAPIsEndpoints(b => b
    .SetApiRoutePrefix("sf")
    .ConfigureAllEndpoints(b => b
        .UpdateRoute(route => route
            .Replace("stateMachines/", "")
            .Replace("actions/", "")
            .Replace("activities/", "")
        )
    )
);

app.MapGet("/doc", async (IStateMachineLocator locator) =>
{
    if (locator.TryLocateStateMachine(new StateMachineId("Doc", "x"), out var stateMachine))
    {
        return Results.Ok(await stateMachine.GetStatusAsync());
    }
    
    return Results.NotFound();
});

app.Run();