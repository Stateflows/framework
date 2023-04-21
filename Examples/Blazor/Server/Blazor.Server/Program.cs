using Blazor.Server.Data;
using Examples.Common;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Stateflows;
using Stateflows.StateMachines;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();

builder.Services.AddSignalR();

builder.Services.AddStateflows(b => b
    .AddPlantUml()

    .AddStateMachine("stateMachine1", b => b
        .AddInitialState("state1", b => b
            .AddTransition<SomeEvent>("state2")
            .AddInternalTransition<ExampleRequest>(b => b
                .AddEffect(c => c.Event.Respond(new ExampleResponse() { ResponseData = "Example response data" }))
            )
        )
        .AddState("state2", b => b
            .AddTransition<OtherEvent>("state3", b => b
                .AddGuard(c => c.Event.AnswerToLifeUniverseAndEverything == 42)
            )
        )
        .AddCompositeState("state3", b => b
            .AddTransition<SomeEvent>("state4")

            .AddInitialState("state3_1", b => b
                .AddTransition<SomeEvent>("state3_2")
            )
            .AddState("state3_2")
        )
        .AddState("state4", b => b
            .AddDefaultTransition("state5")
        )
        .AddState("state5", b => b
            .AddInternalTransition<ExampleRequest>(b => b
                .AddEffect(c =>
                {
                    var counter = c.SourceState.Values.GetOrDefault<int>("counter", 0);
                    c.SourceState.Values.Set("counter", counter + 1);
                })
            )
            .AddDefaultTransition("state2", b => b
                .AddGuard(c =>
                {
                    var counter = c.SourceState.Values.GetOrDefault<int>("counter", 0);
                    return counter > 2;
                })
            )
        )
    )
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
