using OpenTelemetry;
using OpenTelemetry.Trace;
using WarszawskieDniInformatyki.Components;
using Stateflows;
using Stateflows.Extensions.OpenTelemetry;
using Stateflows.StateMachines;
using Stateflows.Extensions.MinimalAPIs;
using WarszawskieDniInformatyki.StateMachines.Document;
using Scalar.AspNetCore;
using Stateflows.Actions;
using Stateflows.Activities;
using Stateflows.Scheduler.StateMachine;
using WarszawskieDniInformatyki.Actions.Work;
using WarszawskieDniInformatyki.Activities.Process;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddStateflows(b => b
    .AddActions(b => b
        .AddAction<Work>()
    )
    .AddStateMachines(b => b
        .AddStateMachine<Document>()
    )
    .AddActivities(b => b
        .AddActivity<Process>()
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

app.MapStateflowsMinimalAPIsEndpoints();

app.Run();