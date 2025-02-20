using System.Diagnostics;
using Blazor.Server.Data;
using Examples.Common;
using Examples.Storage;
using Stateflows;
using Stateflows.Activities;
using Stateflows.Common;
using Stateflows.StateMachines;
using X;
using Microsoft.AspNetCore.OpenApi;
using Stateflows.Actions;
using Stateflows.Transport.Http;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddCors();
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

builder.Services.AddSignalR();

builder.Services.AddStateflows(b => b
    .AddPlantUml()
    .AddConsole()

    // .AddStorage()
    
    
    .AddActions(b => b
        .AddAction("action1", async c =>
        {
            await c.Action.Values.SetAsync<int>("TheAnswerToTheLifeWorldAndUniverse", 42);
            
            var answer = await c.Action.Values.GetOrDefaultAsync<int>("theAnswerToTheLifeWorldAndUniverse");
        })
    )
    
    .AddStateMachines(b => b
    
        .AddStateMachine("stateMachine1", b => b
            .AddInitialState("State1", b => b
                .AddTransition<SomeEvent>("State2")
            )
            .AddState("State2", b => b
                .AddDefaultTransition(FinalState.Name)
            )
            .AddFinalState()
        )
    )
    
    
    .AddActivities(b => b
        .AddActivity("activity1", b => b
            .AddAction("action1", async c => { })
        )
    )
    
    
    

    .AddActivities(b => b
        .AddActivity("a", b => b
            .AddInitial(b => b
                .AddControlFlow("action")
            )
            .AddAction("action", async c =>
            {
                Debug.WriteLine("action");
            })
        )
    )

    .AddStateMachines(b => b
        .AddStateMachine("stateMachine1", b => b
            .AddInitialState("initial", b => b
                .AddDefaultTransition<Choice>()
            )
            .AddChoice(b => b
                .AddTransition("state1")
                .AddElseTransition("state2")
            )
            .AddState("state1", b => b
                .AddDefaultTransition<Junction>()
            )
            .AddState("state2")
            .AddJunction(b => b
                .AddTransition("state3")
                .AddElseTransition("state4")
            )
            .AddState("state3")
            .AddState("state4")
        )
        .AddStateMachine("stateMachine1_orth", b => b
            .AddInitialState("state1", b => b
                .AddOnEntry(async c =>
                {
                    Debug.WriteLine("x");
                })
                .AddTransition<Startup>("state3")
                .AddInternalTransition<ExampleRequest>(b => b
                    .AddEffect(async c =>
                    {
                        c.Event.Respond(new ExampleResponse() { ResponseData = "Example response data" });
                    })
                )
                .AddDefaultTransition<Fork>()
            )
            .AddState("state2", b => b
                .AddOnEntry(async c =>
                {
                    c.StateMachine.Publish(new SomeNotification());
                })
                .AddTransition<SomeEvent>("state1")
                .AddTransition<OtherEvent>("state3")
            )
            .AddState("state3", b => b
                .AddTransition<SomeEvent>("state1")
                .AddTransition<OtherEvent, FinalState>()
                .AddDefaultTransition<Fork>()
            )
            .AddFork(b => b
                .AddTransition("reg1")
                .AddTransition("reg2")
                .AddTransition("reg3")
            )
            .AddOrthogonalState("orthy", b => b
                .AddRegion(b => b
                    .AddInitialState("reg1", b => b
                        .AddDefaultTransition<Join>()
                    )
                )
                .AddRegion(b => b
                    .AddState("reg2", b => b
                        .AddDefaultTransition<Join>()
                    )
                )
                .AddRegion(b => b
                    .AddState("reg3", b => b
                        .AddDefaultTransition("reg3_final")
                    )
                    .AddFinalState("reg3_final")
                )
            )
            .AddJoin(b => b
                .AddTransition<FinalState>()
            )
            .AddFinalState()
        )

        .AddStateMachine("startupInternal", b => b
            .AddInitialState("state1", b => b
                .AddInternalTransition<Startup>(b => b
                    .AddEffect(async c => Console.WriteLine("startup!"))
                )
            )
        )
    )
    .SetEnvironment(
        builder.Environment.IsDevelopment()
            ? $"{StateflowsEnvironments.Development}.{Environment.MachineName}"
            : StateflowsEnvironments.Production
    )
);

var app = builder.Build();

//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.MapStateflowsSignalRTransport();
app.MapStateflowsHttpTransport();

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
    public class State1 : IState
    {

    }

    public class ControlFlow : IControlFlow
    {

    }

    public class Guard : ITransitionGuard
    {
        public Task<bool> GuardAsync()
        {
            throw new NotImplementedException();
        }
    }
}