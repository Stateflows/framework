using System.Diagnostics;
using Blazor.Server.Data;
using Examples.Common;
using Examples.Storage;
using Microsoft.AspNetCore.Mvc;
using Stateflows;
using Stateflows.Activities;
using Stateflows.Common;
using Stateflows.StateMachines;
using X;
using Microsoft.AspNetCore.OpenApi;
using OneOf;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Trace;
using Scalar.AspNetCore;
using Stateflows.Actions;
using Stateflows.Extensions.OpenTelemetry;
using Stateflows.StateMachines.Attributes;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.Transport.Http;
using Stateflows.Transport.REST;
using DependencyInjection = Stateflows.Transport.REST.DependencyInjection;
using Guards = Stateflows.StateMachines.Guards;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddCors();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Services.AddSignalR();

builder.Logging.AddOpenTelemetry(logging =>
{
    logging.IncludeFormattedMessage = true;
    logging.IncludeScopes = true;
});

var otel = builder.Services.AddOpenTelemetry();
otel.WithTracing(tracing =>
{
    tracing.AddAspNetCoreInstrumentation();
    tracing.AddHttpClientInstrumentation();
});

var OtlpEndpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
if (OtlpEndpoint != null)
{
    otel.UseOtlpExporter();
}

builder.Services.AddStateflows(b => b
    .AddPlantUml()
    .AddConsole()
    
    .AddOneOf()
    
    .AddOpenTelemetry()

    // .AddStorage()
    
    .AddActivities(b => b
        .AddActivity("a", b => b
            .AddInitial(b => b
                .AddControlFlow("action")
                .AddControlFlow("structured")
            )
            .AddAcceptEventAction<SomeEvent>(b => b
                .AddControlFlow("action")
            )
            .AddAction("action", async c =>
            {
                Debug.WriteLine("action");
            })
            .AddStructuredActivity("structured", b => b
                .AddInitial(b => b
                    .AddControlFlow("inner")
                )
                .AddAction("inner", async c => Debug.WriteLine("inner"))
                // .AddGet("/test", () => Results.Ok("test"))
            )
        )
    
        .AddActivity<ActivityX>()
    )
    
    .AddActions(b => b
        .AddAction("actionBehavior", async c =>
        {
            var boolTokens = c.GetTokensOfType<bool>();
            
            c.Behavior.Publish(new BehaviorInfo() { BehaviorStatus = BehaviorStatus.Initialized });
            
            // logic
        })
    )

    .AddStateMachines(b => b
        .AddStateMachine<StateMachine1>()
        .AddStateMachine("process", b => b
            .AddEndpoints(b =>
            {
                b.AddPost("/postX", (SomeDTO dto) => Results.Ok(dto)).WithOpenApi();
                // b.AddGet("/getX", (SomeDTO dto) => Results.Ok(dto)).WithOpenApi();
            })
            .AddInitialState("initial", b => b
                .AddTransition<OneOf<SomeEvent, ExampleRequest>>("state1")
                // .AddTransition<ExampleRequest>("state1")
                // .AddElseTransition<ExampleRequest, Choice>()
            )
            .AddChoice(b => b
                .AddTransition("state1")
                .AddElseTransition("state2")
            )
            .AddState("state1", b => b
                .AddDefaultTransition<Junction>()
            )
            .AddState("state2", b => b
                .AddEndpoints(b =>
                {
                    b.AddGet("/foo", () => Results.Ok(new OtherEvent()));
                })
            )
            .AddJunction(b => b
                .AddTransition("state3")
                .AddElseTransition("state4")
            )
            .AddState("state3", b => b
                .AddEndpoints(b =>
                {
                    b.AddPost("/postRoute", (SomeDTO dto) => Results.Ok(dto));
                })
            )
            .AddCompositeState("composite", b => b
                .AddState("state4")
                .AddEndpoints(b =>
                {
                    b.AddPost("/post", (SomeDTO dto) => Results.Ok(dto)).WithOpenApi(config =>
                    {
                        config.Deprecated = true;

                        return config;
                    });
                    // b.AddGet("/get", (SomeDTO dto) => Results.Ok(dto)).WithOpenApi();
                })
            )
            // .AddState<StateWithEndpoints>()
        )
    )
    .SetEnvironment(
        builder.Environment.IsDevelopment()
            ? $"{StateflowsEnvironments.Development}.{Environment.MachineName}"
            : StateflowsEnvironments.Production
    )
);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(options =>
    {
        options.RouteTemplate = "/openapi/{documentName}.json";
    });
    // app.UseSwaggerUI();
    app.MapScalarApiReference();
}

//app.MapStateflowsSignalRTransport();
// app.MapStateflowsHttpTransport();
DependencyInjection.MapStateflowsHttpTransport(app, string.Empty);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseStateflowsConsole();
app.UseRouting();
app.UseCors();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.MapGroup("/x");

app.MapGet("/swagger", () => { return Results.Redirect("/swagger/index.html"); }).WithOpenApi();

app.Run();

namespace X
{
    public class ActivityX : IActivity, IActivityEndpoints
    {
        public void Build(IActivityBuilder builder)
        {
            builder
                .AddInitial(b => b
                    .AddControlFlow("entry")
                    // .AddControlFlow<AcceptEventActionNode<SomeEvent>>()
                )
                .AddAction("entry", async c => { }, b => b
                    .AddControlFlow("second")
                )
                .AddAcceptEventAction<SomeEvent>(b => b
                    .AddControlFlow("second")
                )
                .AddAction("second", async c =>
                    {
                        c.OutputRange(Enumerable.Range(0, 10));
                        c.OutputRange(Enumerable.Range(0, 10).Select(i => $"string: {i.ToString()}"));
                    }, b => b
                    .AddFlow<int>("intProcessing")
                    .AddFlow<int>("stringProcessing", b => b
                        .AddGuard(async c => c.Token % 2 == 0)
                        .AddTransformation(async c => c.ToString())
                    )
                    .AddFlow<string>("stringProcessing")
                )
                .AddAction("intProcessing", async c => { })
                .AddStructuredActivity("stringProcessing", b => b
                    .AddInput(b => b
                        .AddFlow<string>("process")
                    )
                    .AddAction("process", async c =>
                    {
                        
                    })
                )
            ;
        }

        public void RegisterEndpoints(IEndpointsBuilder endpointsBuilder)
        {
            endpointsBuilder.AddGet("/foo", () => Results.Ok("Hello World!"));
        }
    }

    public class SomeDTO
    {
        public string StringData { get; set; }
        public required int IntData { get; set; }
    }

    public class StateWithEndpoints : IStateDefinition
    {
        public void Build(IStateBuilder builder)
        {
            builder
                .AddEndpoints(b =>
                {
                    b.AddGet("/foo", () => Results.Ok(new OtherEvent()));
                    b.AddPost("/postRoute", (SomeDTO dto) => Results.Ok(dto.StringData));
                })
                ;
        }
    }

    public class State3 : IStateEntry
    {
        private readonly ILogger<StateMachine1> Logger;
        public State3(ILogger<StateMachine1> logger)
        {
            Logger = logger;
        }

        public async Task OnEntryAsync()
        {
            Logger.LogWarning("warning for the deep");
        }
    }
    
    [StateMachineBehavior("stateMachine1")]
    public class StateMachine1 : IStateMachine
    {
        public void Build(IStateMachineBuilder builder)
        {
            builder
                .AddInitialState("initial", b => b
                        .AddTransition<SomeEvent>("state1", b => b
                            .AddGuard(Guards.Deny)
                        )
                        .AddTransition<SomeEvent>("state2")
                    // .AddInternalTransition<ExampleRequest>(b => b
                    //     .AddGuard(Guards.Deny)
                    // )
                    // .AddDeferredEvent<ExampleRequest>()
                )
                .AddState("state1")
                .AddCompositeState("state2", b => b
                    .AddInitialState("compositeInitial", b => b
                        .AddDefaultTransition("state3")
                    )
                    .AddState<State3>("state3", b => b
                        .AddTransition<ExampleRequest>("state4")
                    )
                    .AddTransition<Exception>("state5")
                )
                .AddState("state4", b => b
                    .AddOnEntry(async c => throw new Exception("test"))
                )
                .AddState("state5");
        }
    }
}