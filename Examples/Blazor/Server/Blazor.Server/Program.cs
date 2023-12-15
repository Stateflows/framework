using Blazor.Server;
using Blazor.Server.Data;
using Examples.Common;
using Examples.Storage;
using Stateflows;
using Stateflows.Common;
using Stateflows.StateMachines;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();

builder.Services.AddSignalR();

builder.Services.AddStateflows(b => b
    .AddPlantUml()
    .AddStateMachines(b => b
        .AddStateMachine("stateMachine1", b => b
            .AddInitialState("state1", b => b
                .AddTransition<OtherEvent>("state2")
            )
            .AddState("state2", b => b
                .AddTransition<OtherEvent>("state3")
            )
            .AddState("state3", b => b
                .AddTransition<OtherEvent>("state1", b => b
                    .AddGuard(c => Random.Shared.Next(1, 10) % 2 == 0)
                    .AddEffect(c => Debug.WriteLine("Even, going to state1"))
                )
                .AddElseTransition<OtherEvent>("state2", b => b
                    .AddEffect(c => Debug.WriteLine("Odd, going to state2"))
                )
            )
        )
    )

    .SetEnvironment(
        builder.Environment.IsDevelopment()
            ? $"{StateflowsEnvironments.Development}.{Environment.MachineName}"
            : StateflowsEnvironments.Production
    )

    //.AddStorage()
);

var app = builder.Build();

app.MapStateflowsTransportHub();

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