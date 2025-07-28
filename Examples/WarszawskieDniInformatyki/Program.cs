using OpenTelemetry;
using OpenTelemetry.Trace;
using WarszawskieDniInformatyki.Components;
using Stateflows;
using Stateflows.Extensions.OpenTelemetry;
using Stateflows.StateMachines;
using Stateflows.Extensions.MinimalAPIs;
using WarszawskieDniInformatyki.StateMachines.Document;
using Scalar.AspNetCore;
using Stateflows.Activities;
using Stateflows.Scheduler.StateMachine;
using WarszawskieDniInformatyki.Activities.Process;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddStateflows(b => b
    .AddStateMachines(b => b
        .AddStateMachine<Document>("Doc")
    )
    .AddActivities(b => b
        .AddActivity<Process>("Proc")
    )

    #region extensions
    .AddPlantUml()
    .AddOpenTelemetry()
    .AddScheduling()
    .AddOneOf()
    #endregion
);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

#region OpenTelemetry
// Setup logging to be exported via OpenTelemetry
builder.Logging.AddOpenTelemetry(logging =>
{
    logging.IncludeFormattedMessage = true;
    logging.IncludeScopes = true;
});

var otel = builder.Services.AddOpenTelemetry();

// Add Tracing for ASP.NET Core and our custom ActivitySource and export via OTLP
otel.WithTracing(tracing =>
{
    // tracing.AddAspNetCoreInstrumentation();
    tracing.AddHttpClientInstrumentation();
});

// Export OpenTelemetry data via OTLP, using env vars for the configuration
var OtlpEndpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
if (OtlpEndpoint != null)
{
    otel.UseOtlpExporter();
}
#endregion

#region OpenAPI
builder.Services.AddOpenApi();
#endregion

var app = builder.Build();

#region OpenAPI
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
#endregion

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapStateflowsMinimalAPIsEndpoints(b => b
    .ConfigureGetAllClassesEndpoint(b => b
        .Disable()
    )
    .ConfigureStateMachines(b => b
        .ConfigureStateMachine("Doc", b => b
            .ConfigureAllEndpoints(b => b
                .UpdateRoute(b => b
                    .Replace("Doc", "Dupa")
                )
            )
        )
    )
);

app.Run();


public class I : EndpointDefinitionInterceptor
{
    public override bool BeforeGetAllClassesEndpointDefinition(ref string method, ref string route)
    {
        return false;
    }

    public override bool BeforeGetInstancesEndpointDefinition(BehaviorClass behaviorClass, ref string method, ref string route)
    {
        return false;
    }

    public override bool BeforeEventEndpointDefinition<TEvent>(BehaviorClass behaviorClass, ref string method, ref string route)
    {
        route = route.Replace("Stateflows.Scheduler.StateMachine.StateflowsScheduler", "scheduler");
        return true;
    }
}