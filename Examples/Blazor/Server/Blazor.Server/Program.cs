using System.Diagnostics;
using Blazor.Server.Data;
using Examples.Common;
using Stateflows;
using Stateflows.Common;
using Stateflows.StateMachines;
using Stateflows.Activities;
using Stateflows.Activities.Attributes;
using Stateflows.Activities.Collections;
using Stateflows.Activities.Registration.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();

builder.Services.AddSignalR();

builder.Services.AddStateflows(b => b
    .AddPlantUml()

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
                    c.OutputTokensRange((new[] { 1, 2, 3, 4, 5 }).Select(x => new ValueToken<int>() { Value = x }).ToArray());
                    c.OutputTokensRange((new[] { "1", "2", "3" }).Select(x => new ValueToken<string>() { Value = x }).ToArray());
                },
                b => b.AddObjectFlow<ValueToken<int>>("2")
            )
            .AddAction("2",
                async c =>
                {
                    Debug.WriteLine("action 2");
                }
            )
            .AddAcceptEventAction<SomeEvent>("event",
                async c => { },
                b => b
                    .AddControlFlow("decision")
            )
            .AddTimeEventAction<EveryOneMinute>("time",
                async c =>
                {
                    c.OutputToken(new ValueToken<int>() { Value = Random.Shared.Next(1, 5) });
                    c.OutputToken(new ValueToken<int>() { Value = Random.Shared.Next(1, 5) });
                    c.OutputToken(new ValueToken<int>() { Value = Random.Shared.Next(1, 5) });
                    c.OutputToken(new ValueToken<int>() { Value = Random.Shared.Next(1, 5) });
                    c.OutputToken(new ValueToken<int>() { Value = Random.Shared.Next(1, 5) });
                    c.OutputToken(new ValueToken<int>() { Value = Random.Shared.Next(1, 5) });
                    c.OutputToken(new ValueToken<int>() { Value = Random.Shared.Next(1, 5) });
                    c.OutputToken(new ValueToken<int>() { Value = Random.Shared.Next(1, 5) });
                    c.OutputToken(new ValueToken<int>() { Value = Random.Shared.Next(1, 5) });
                    c.OutputToken(new ValueToken<int>() { Value = Random.Shared.Next(1, 5) });
                },
                b => b
                    .AddObjectFlow<ValueToken<int>>("data")
            )
            .AddDataStore("data", b => b
                .AddObjectFlow<ValueToken<int>>("decision")
            )
            .AddObjectDecision<ValueToken<int>>("decision", b => b
                .AddObjectFlow("3", b => b
                    .AddGuard(async c => c.Token.Value < 3)
                    .AddTransformation(async c =>
                    {
                        Debug.WriteLine($"token going to 3: {c.Token.Value}");
                        return c.Token;
                    })
                )
                .AddObjectFlow("4", b => b
                    .AddGuard(async c => c.Token.Value < 6)
                    .AddTransformation(async c =>
                    {
                        Debug.WriteLine($"token going to 4: {c.Token.Value}");
                        return c.Token;
                    })
                )
                .AddElseObjectFlow("5"
                    , b => b.AddTransformation(async c =>
                    {
                        Debug.WriteLine($"token going to 5: {c.Token.Value}");
                        return c.Token;
                    })
                )
            )
            .AddAction("3",
                async c =>
                {
                    Debug.WriteLine($"finish 3! {c.InputTokens.OfType<ValueToken<int>>().Count()}");
                }
            )
            .AddAction("4",
                async c =>
                {
                    Debug.WriteLine($"finish 4! {c.InputTokens.OfType<ValueToken<int>>().Count()}");
                }
            )
            .AddAction("5",
                async c =>
                {
                    Debug.WriteLine($"finish 5! {c.InputTokens.OfType<ValueToken<int>>().Count()}");
                },
                b => b.AddControlFlow<Final>()
            )
            .AddFinal()
        )
    )

    .AddStateMachines(b => b

        //.AddStateMachine("dossier-cleanup", b => b
        //    .AddInitialState("selection", b => b
        //        .AddTransition<SelectDossiers>("gathering")
        //    )
        //    .AddState("dossiers-gathering", b => b
        //        .AddOnEntry(c =>
        //        {
        //            // send dossiers request
        //        })
        //        .AddInternalTransition<DossierObtained>(b => b
        //            .AddEffect(c => { /* save dossier data locally */ })
        //        )
        //        .AddDefaultTransition("validation", b => b
        //            .AddGuard(c => true /* all dossiers obtained */)
        //        )
        //    )
        //    .AddState("validation", b => b
        //        .AddTransition<ClearDossiers>("clearing")
        //    )
        //    .AddState("clearing", b => b
        //        .AddOnEntry(c =>
        //        {
        //            // send clearing request
        //        })
        //        .AddOnEntry(c =>
        //        {
        //            // run archiving
        //        })

        //        .AddInternalTransition<DossierCleared>(b => b
        //            .AddEffect(c => { /* save log entry */})
        //        )
        //        .AddDefaultTransition("cleared", b => b
        //            .AddGuard(c => true /* all dossiers cleared */)
        //        )

        //        .AddOnExit(c => { /* clear local dossiers data */ })
        //    )
        //    .AddState<State1>(b => b
        //        .AddOnDoActivity("")
        //    )
        //    .AddFinalState("cleared")
        //)


        .AddStateMachine("stateMachine1", b => b
            .AddInitialState("state1", b => b
                .AddTransition<OtherEvent>("state2")
                .AddInternalTransition<ExampleRequest>(b => b
                    .AddEffect(c => c.Event.Respond(new ExampleResponse()))
                )
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

public class Action1 : Stateflows.Activities.Action
{
    public readonly Input<ValueToken<int>> Ints;
    public readonly Input<ValueToken<string>> Strings;
    public readonly Output<ValueToken<int>> IntsOut;

    public override async Task ExecuteAsync()
    {
        await Task.Delay(1000);

        Debug.WriteLine("ints count: " + Ints.Count().ToString() ?? "input not working");

        IntsOut.AddRange(Ints);

        await Task.Delay(1000);
    }
}

public class Action2 : Stateflows.Activities.Action
{
    public readonly OptionalInput<ValueToken<string>> Strings;
    public readonly Output<ValueToken<string>> StringsOut;

    public override async Task ExecuteAsync()
    {
        await Task.Delay(1000);

        Debug.WriteLine("strings count: " + Strings.Count().ToString() ?? "input not working");

        StringsOut.AddRange(Strings);
    }
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