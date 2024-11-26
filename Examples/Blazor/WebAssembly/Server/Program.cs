using Examples.Common;

using Stateflows;
using Stateflows.Common;
using Stateflows.StateMachines;
using Stateflows.StateMachines.Sync;
using Stateflows.Transport.Http;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();

builder.Services.AddStateflows(b => b
    .AddPlantUml()

    .AddStateMachines(b => b
        .AddStateMachine("zemanowa-maszyna", b => b
            .AddInitialState("draft", b => b
                .AddTransition<SomeEvent>("active")
            )
            .AddCompositeState("active", b => b
                .AddInitialState("normal", b => b
                    .AddTransition<OtherEvent>("special")
                )
                .AddState("special", b => b
                    .AddTransition<OtherEvent>("normal")
                    .AddInternalTransition<SomeEvent>(b => b
                        .AddEffect(async c => { })
                    )
                )
            )
        )

        .AddStateMachine("stateMachine1", b => b
            .AddInitialState("state1", b => b
                .AddTransition<SomeEvent>("state2")
                .AddInternalTransition<ExampleRequest>(b => b
                    .AddEffect(c => c.Event.Respond(new ExampleResponse() { ResponseData = "Example response data" }))
                )
            )
            .AddState("state2", b => b
                .AddOnEntry(async c =>
                {
                    // logic
                })
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
                .AddTransition<AfterOneMinute>("state2")
                .AddDefaultTransition("state2", b => b
                    .AddGuard(c =>
                    {
                        var counter = c.SourceState.Values.GetOrDefault<int>("counter", 0);
                        return counter > 2;
                    })
                )
            )
        )
    )
);

// Add services to the container.

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

//app.MapStateflowsSignalRTransport();
app.MapStateflowsHttpTransport();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();


app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
