using System.Diagnostics;
using Blazor.Server.Data;
using Examples.Common;
using Stateflows;
using Stateflows.Utils;
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
using X;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();

builder.Services.AddSignalR();

builder.Services.AddStateflows(b => b
    .AddPlantUml()

    //.AddStorage()

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

        .AddStateMachine("stateMachine2", b => b
            .AddInitialState("state1", b => b
                .AddOnEntry(async c =>
                {
                    await c.StateMachine.SubscribeAsync<SomeNotification>(new StateMachineId("stateMachine1", "x"));
                })
                .AddTransition<SomeNotification>("state2")
            )
            .AddState("state2")
        )
    )

    .AddActivities(b => b
        .AddActivity("activity2", b => b
            .AddOnInitialize<InitializationRequest1>(async c =>
            {
                Debug.WriteLine(c.InitializationRequest.Foo);

                return true;
            })
            .AddInitial(b => b
                .AddControlFlow("main")
            )
            .AddStructuredActivity("main", b => b
                .AddExceptionHandler<Exception>(async c =>
                {
                    Debug.WriteLine(c.Exception.Message);
                    c.Output(new Token<int>() { Payload = 666 });
                })
                .AddInitial(b => b
                    .AddControlFlow("action1")
                    .AddControlFlow("action3")
                )
                .AddAction(
                    "action1",
                    async c => c.Output(new Token<int> { Payload = 42 }),
                    b => b
                        .AddFlow<Token<int>, Flow1>("action2")
                )
                .AddAction(
                    "action2",
                    async c =>
                    {
                        Debug.WriteLine(c.Input.OfType<Token<int>>().First().Payload);
                        throw new Exception("test");
                    },
                    b => b.AddControlFlow("action3")
                )
                .AddAction(
                    "action3",
                     async c => { }
                )

                .AddFlow<Token<int>>("action4")
            )
            .AddAction("action4", async c => { })
        )

        .AddActivity<Activity3>("activity3")
    )

    .AddAutoInitialization(new StateMachineClass("stateMachine1"))

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

namespace X
{
    public class InitializationRequest1 : InitializationRequest
    {
        public string Foo { get; set; }
    }

    public class Activity3 : Stateflows.Activities.Activity<InitializationRequest1>
    {
        private async Task MethodAsync()
        {
            return;
        }

        public override void Build(ITypedActivityBuilder builder)
            => builder
                .AddAcceptEventAction<SomeEvent>(async c => { }, b => b
                    .AddControlFlow("action1")
                )
                .AddAction(
                    "action1",
                    async c => c.OutputRange(Enumerable.Range(0, 100).ToTokens()),
                    b => b.AddFlow<Token<int>>("chunked")
                )

                .AddParallelActivity<Token<int>>(
                    "chunked",
                    b => b
                        .AddInput(b => b
                            .AddFlow<Token<int>>("main")
                        )
                        .AddAction(
                            "main",
                            async c =>
                            {
                                var tokens = c.Input.OfType<Token<int>>().Select(t => $"value: {t.Payload}").ToTokens();
                                c.OutputRange(tokens);
                                Debug.WriteLine($"{tokens.Count()}/{c.Input.Count()} tokens: {string.Join(", ", c.Input.OfType<Token<int>>().Take(5).Select(t => t.Payload))}...");
                                await Task.Delay(1000);
                            },
                            b => b.AddFlow<Token<string>>(OutputNode.Name)
                        )
                        .AddOutput()
                        .AddFlow<Token<string>>("action2", b => b
                            .AddGuard(async c =>
                            {
                                lock (c.Activity.LockHandle)
                                {
                                    var counter = 0;
                                    c.Activity.Values.TryGet<int>("count", out counter);

                                    counter++;
                                    c.Activity.Values.Set<int>("count", counter);
                                }

                                return true;
                            })
                        ),
                    17
                )
                .AddAction(
                    "action2",
                    async c =>
                    {
                        Debug.WriteLine(c.Input.OfType<Token<string>>().Count().ToString());
                        c.Activity.Values.TryGet<int>("count", out var counter);
                        Debug.WriteLine($"counter: {counter}");
                        //foreach (var token in c.Input.OfType<Token<string>>())
                        //{
                        //    Debug.WriteLine(token.Payload);
                        //}
                    }
                );

        public override async Task<bool> OnInitializeAsync(InitializationRequest1 initializationRequest)
        {
            Debug.WriteLine(initializationRequest.Foo);

            return true;
        }
    }

    public class Flow1 : Flow<Token<int>>
    {
        public override async Task<bool> GuardAsync()
        {
            return Context.Token.Payload > 40;
        }
    }
}