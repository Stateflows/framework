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
using Stateflows.Transport.Http;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();

builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

builder.Services.AddSignalR();

builder.Services.AddStateflows(b => b
    .AddPlantUml()

    //.AddStorage()

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
            .AddInitialState("state1", b => b
                .AddOnEntry(async c =>
                {
                    Debug.WriteLine("x");
                })
                //.AddOnEntry<ActionX>()
                //.AddTransition<SomeEvent>("state2", b => b
                //    .AddGuard<X.Guard>()
                //    .AddEffect<ActionX>()
                //)
                .AddTransition<Startup>("state3")
                .AddInternalTransition<ExampleRequest>(b => b
                    .AddEffect(async c =>
                    {
                        c.Event.Respond(new ExampleResponse() { ResponseData = "Example response data" });
                    })
                    .AddGuard(c => throw new Exception("test"))
                )
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
            )
            .AddFinalState()
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

app.UseRouting();

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