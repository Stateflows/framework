using System.Diagnostics;
using Blazor.Server.Data;
using Examples.Common;
using Stateflows;
using Stateflows.Common;
using Stateflows.Common.Data;
using Stateflows.StateMachines;
using Stateflows.StateMachines.Typed;
using Stateflows.StateMachines.Data;
using Stateflows.StateMachines.Sync;
using Stateflows.Activities;
using Stateflows.Activities.Data;
using Stateflows.Activities.Typed;
using Stateflows.Activities.Attributes;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.StateMachines.Attributes;
using Examples.Storage;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();

builder.Services.AddSignalR();

builder.Services.AddStateflows(b => b
    .AddPlantUml()

    .AddStorage()

    .AddTracing()

    .AddStateMachines(b => b
        .AddStateMachine("stateMachine1", b => b
            .AddInitialState("state1", b => b
                .AddTransition<SomeEvent>("state2")
                .AddInternalTransition<ExampleRequest>(b => b
                    .AddEffect(c => c.Event.Respond(new ExampleResponse() { ResponseData = "Example response data" }))
                )
            )
            .AddState("state2", b => b
                .AddOnEntry(c =>
                {
                    c.StateMachine.Publish(new SomeNotification());
                })
                .AddTransition<SomeEvent>("state1")
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

app.MapStateflowsSignalRTransport();

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

app.Run();
