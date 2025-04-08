using OpenTelemetry;
using OpenTelemetry.Trace;
using WarszawskieDniInformatyki.Components;
using Stateflows;
using Stateflows.Extensions.OpenTelemetry;
using Stateflows.StateMachines;
using Stateflows.Transport.REST;
using WarszawskieDniInformatyki.StateMachines.Document;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddStateflows(b => b
    .AddStateMachines(b => b
        .AddStateMachine<Document>("Doc")
    )

    #region extensions
    .AddPlantUml()
    .AddOpenTelemetry()
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

app.MapStateflowsHttpTransport(string.Empty);

app.Run();