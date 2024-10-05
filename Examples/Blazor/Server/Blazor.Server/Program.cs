using System.Diagnostics;
using Blazor.Server.Data;
using Examples.Common;
using Examples.Storage;
using Stateflows;
using Stateflows.Common;
using Stateflows.StateMachines;
using Stateflows.StateMachines.Typed;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();

builder.Services.AddSignalR();

builder.Services.AddStateflows(b => b
    .AddPlantUml()

    //.AddStorage()

    .AddStateMachines(b => b
        .AddStateMachine("stateMachine1", b => b
            .AddInitialState("state1", b => b
                .AddOnEntry(async c =>
                {
                    Debug.WriteLine("x");
                })
                .AddTransition<SomeEvent>("state2")
                .AddTransition<Startup>("state3")
                .AddInternalTransition<ExampleRequest>(b => b
                    .AddEffect(async c =>
                    {
                        c.Event.Respond(new ExampleResponse() { ResponseData = "Example response data" });
                    })
                    .AddGuard(c => throw new Exception("test"))
                )
                //.AddDoActivity<Activity3>()
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

//app.MapStateflowsSignalRTransport();

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
