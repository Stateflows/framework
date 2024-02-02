using System.Diagnostics;
using Blazor.Server.Data;
using Examples.Common;
using Stateflows;
using Stateflows.Common;
using Stateflows.Common.Data;
using Stateflows.StateMachines;
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

    .AddActivities(b => b
        .AddActivity("sync0", b => b
            .AddInitial(b => b
                .AddControlFlow("1")
            )
            .AddStructuredActivity("1", b => b
                .AddAcceptEventAction<EveryOneMinute>("time1", async c => c.Output(c.Event), b => b
                    .AddTokenFlow<EveryOneMinute, FinalNode>()
                )
                .AddFinal()
                .AddControlFlow("2")
            )
            .AddStructuredActivity("2", b => b
                .AddTimeEventAction<EveryOneMinute>("time2", async c => { })                
            )
            .AddTimeEventAction<EveryOneMinute>("time3", async c => { })

        //.AddTimeEventAction<EveryOneMinute>("time", async c => { }, b => b
        //    .AddControlFlow("merge")
        //)
        //.AddMerge("merge", b => b
        //    .AddControlFlow("sync_table_1")
        //)
        //.AddAction("sync_table_1", async c =>
        //{
        //    /* implementation */
        //    Debug.WriteLine("table 1 synced");
        //})
        )
    )

    .AddDefaultInstance(BehaviorType.Activity.ToClass("sync0"))

    .AddStorage()

    //.AddTracing()

    .AddActivities(b => b
        .AddActivity("activity1", b => b
            .AddOnInitialize(async c =>
            {
                Debug.WriteLine("initialize");
                return true;
            })


            .AddInitial(b => b
                .AddControlFlow("1")
            )
            .AddAction("1",
                async c =>
                {
                    c.OutputRange((new[] { 1, 2, 3, 4, 5 }).ToTokens());
                    c.OutputRange((new[] { "1", "2", "3" }).ToTokens());
                },
                b => b.AddDataFlow<int>("2")
            )
            .AddAction("2",
                async c =>
                {
                    Debug.WriteLine("action 2");
                }
            )
            .AddAcceptEventAction<SomeEvent>("event",
                async c =>
                {
                    Debug.WriteLine(c.Event.TheresSomethingHappeningHere);
                },
                b => b
                    .AddControlFlow("decision")
            )
            .AddTimeEventAction<EveryOneMinute>("time",
                async c =>
                {
                    c.Output(Random.Shared.Next(1, 5));
                    c.Output(Random.Shared.Next(1, 5));
                    c.Output(Random.Shared.Next(1, 5));
                    c.Output(Random.Shared.Next(1, 5));
                    c.Output(Random.Shared.Next(1, 5));
                    c.Output(Random.Shared.Next(1, 5));
                    c.Output(Random.Shared.Next(1, 5));
                    c.Output(Random.Shared.Next(1, 5));
                    c.Output(Random.Shared.Next(1, 5));
                    c.Output(Random.Shared.Next(1, 5));
                },
                b => b
                    .AddDataFlow<int>("data")
            )
            .AddDataStore("data", b => b
                .AddDataFlow<int>("decision")
            )
            .AddDataDecision<int>("decision", b => b
                .AddFlow("3", b => b
                    .AddGuard(async c => c.Token.Payload < 3)
                    .AddTransformation(async c =>
                    {
                        Debug.WriteLine($"token going to 3: {c.Token.Payload}");
                        return c.Token;
                    })
                )
                .AddFlow("4", b => b
                    .AddGuard(async c => c.Token.Payload < 6)
                    .AddTransformation(async c =>
                    {
                        Debug.WriteLine($"token going to 4: {c.Token.Payload}");
                        return c.Token;
                    })
                )
                .AddElseFlow("5"
                    , b => b.AddTransformation(async c =>
                    {
                        Debug.WriteLine($"token going to 5: {c.Token.Payload}");
                        return c.Token;
                    })
                )
            )
            .AddAction("3",
                async c =>
                {
                    Debug.WriteLine($"finish 3! {c.Input.OfType<Token<int>>().Count()}");
                }
            )
            .AddAction("4",
                async c =>
                {
                    Debug.WriteLine($"finish 4! {c.Input.OfType<Token<int>>().Count()}");
                }
            )
            .AddAction("5",
                async c =>
                {
                    Debug.WriteLine($"finish 5! {c.Input.OfType<Token<int>>().Count()}");
                },
                b => b.AddControlFlow<FinalNode>()
            )
            .AddFinal()
        )
    )

    .AddStateMachines(b => b
        .AddStateMachine("stateMachine1", b => b
            .AddInitialState("state1", b => b
                .AddTransition<TestNamespace.MyClass>("state3", b => { }
                //.AddEffect(c => Console.Write(c.Event.Payload.Prop))
                )
                .AddTransition<OtherEvent>("state2")
                .AddTransition<EveryOneMinute>("state2", b => b
                    .AddGuard(c => false)
                )
                //.AddElseTransition<EveryOneMinute>("state2")
                .AddTransition<EveryFiveMinutes>("state2", b => b
                    .AddEffect(c => throw new Exception("test"))
                )
                .AddInternalTransition<ExampleRequest>(b => b
                    .AddEffect(c =>
                    {
                        c.Event.Respond(new ExampleResponse());
                        c.StateMachine.Send(new TestNamespace.MyClass().ToEvent());
                    })
                )
            )
            .AddState("state2", b => b
                .AddTransition<OtherEvent>("state3")
                .AddOnDoActivity("activity1")
            )
            .AddState("state3", b => b
                .AddTransition<OtherEvent>("state1", b => b
                    .AddGuard(c => Random.Shared.Next(1, 10) % 2 == 0)
                //.AddEffect(c => Debug.WriteLine("Even, going to state1"))
                )
                .AddElseTransition<OtherEvent>("state2", b => { }
                //.AddEffect(c => Debug.WriteLine("Odd, going to state2"))
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

app.MapStateflowsTransportHub();
app.MapStateflowsEndpoints();

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

public class Action1 : Stateflows.Activities.ActionNode
{
    public readonly Input<Token<int>> Ints;
    public readonly Input<Token<string>> Strings;
    public readonly Output<Token<int>> IntsOut;

    public override async Task ExecuteAsync()
    {
        await Task.Delay(1000);

        Debug.WriteLine("ints count: " + Ints.Count().ToString() ?? "input not working");

        IntsOut.AddRange(Ints);

        await Task.Delay(1000);
    }
}

public class Action2 : Stateflows.Activities.ActionNode
{
    public readonly OptionalInput<Token<string>> Strings;
    public readonly Output<Token<string>> StringsOut;

    public override async Task ExecuteAsync()
    {
        await Task.Delay(1000);

        Debug.WriteLine("strings count: " + Strings.Count().ToString() ?? "input not working");

        StringsOut.AddRange(Strings);
    }
}

namespace TestNamespace
{
    public class MyClass
    {
        public string Prop { get; set; }
    }

    [ActivityBehavior]
    public class Activity1 : Stateflows.Activities.Activity
    {
        public override void Build(ITypedActivityBuilder builder)
        {
            builder
                .AddAction("", async c => { }, b => b
                    .AddControlFlow("")
                )
                .AddAction("", async c => { })
                ;
        }
    }

    [StateMachineBehavior]
    public class StateMachine1 : Stateflows.StateMachines.StateMachine
    {
        public override void Build(ITypedStateMachineBuilder builder)
        {
            builder
                .AddInitialState("1", b => b
                    .AddDefaultTransition("2")
                )
                .AddState("2")
                ;
        }
    }
}

public class SynchronizeDatabase : ActionNode
{
    public override Task ExecuteAsync()
    {
        Debug.WriteLine("działam");

        return Task.CompletedTask;
    }
}