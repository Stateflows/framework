using Blazor.Server.Data;
using Examples.Common;

using Stateflows;
using Stateflows.Common;
using Stateflows.StateMachines;
using Stateflows.Activities;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();

builder.Services.AddSignalR();

builder.Services.AddStateflows(b => b
    .AddPlantUml()

    .AddActivity("activity1", b => b
        .AddInitial(b => b
            .AddControlFlow("1")
        )
        .AddAction("1", async c =>
        {
            c.OutputRange((new[] { 1, 2, 3, 4, 5 }).Select(x => new ValueToken<int>() { Value = x }));
        }, b => b
            .AddObjectFlow<ValueToken<int>>("2")
        )
        .AddParallelActivity<ValueToken<int>>("2", b => b
            .AddInitial(b => b
                .AddControlFlow("2.1")
                .AddControlFlow<Final>()
            )
            .AddAction("2.1", async c =>
            {
                await Task.Delay(1000);
                Debug.WriteLine("executing thread 1");
            }, b => b
                .AddControlFlow("2.2")
            )
            .AddAction("2.2", async c =>
            {
                await Task.Delay(1000);
                Debug.WriteLine("executing thread 2");
                c.Output(new ValueToken<int>() { Value = Random.Shared.Next(0, 100) });
            }, b => b
                .AddObjectFlow<ValueToken<int>, Output>()
            )
            .AddOutput()
            .AddFinal()

            .AddObjectFlow<ValueToken<int>>("3", b => b.SetWeight(0))
            //.AddControlFlow("3")
        )
        .AddAction("3", async c =>
        {
            Debug.WriteLine($"{c.Input.Count()}");
        })
    )

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
            .AddTransition<AfterOneMinute>("state4")

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

    .AddStateMachine("stateMachine1", 2, b => b
        .AddInitialState("state1", b => b
            .AddTransition<SomeEvent>("state2")
            .AddInternalTransition<ExampleRequest>(b => b
                .AddEffect(c => c.Event.Respond(new ExampleResponse() { ResponseData = "Example response data" }))
            )
        )
        .AddState("state2", b => b
            .AddTransition<OtherEvent>("state4", b => b
                .AddGuard(c => c.Event.AnswerToLifeUniverseAndEverything == 42)
            )
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