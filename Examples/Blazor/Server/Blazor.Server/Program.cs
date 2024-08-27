using System.Diagnostics;
using Blazor.Server.Data;
using Examples.Common;
using Examples.Storage;
using Stateflows;
using Stateflows.Utils;
using Stateflows.Common;
using Stateflows.Common.Data;
using Stateflows.StateMachines;
using Stateflows.StateMachines.Typed;
using Stateflows.StateMachines.Sync;
using Stateflows.Activities;
using Stateflows.Activities.Typed;
using Stateflows.Activities.Attributes;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.StateMachines.Attributes;
using X;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Blazor.Server;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.Common.Exceptions;
using Stateflows.Common.Utilities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();

builder.Services.AddSignalR();

builder.Services.AddStateflows(b => b
    .AddPlantUml()

    //.AddStorage()

    .AddDefaultInstance(StateMachine<Default>.ToClass())

    .AddStateMachines(b => b
        .AddStateMachine<Default>()
        .AddStateMachine("stateMachine1", b => b
            //.AddExceptionHandler<Handler>()
            //.AddInterceptor<Handler>()
            //.AddDefaultInitializer(async c => throw new CustomException("test"))
            .AddInitialState("state1", b => b
                .AddOnEntry(async c =>
                {
                    //throw new CustomException("test");

                    //var e = new CustomException("test");
                    //var x = new Exception("boo", e);
                    //var z = StateflowsJsonConverter.SerializePolymorphicObject(x, false, Newtonsoft.Json.Formatting.Indented);
                    //Debug.WriteLine(z);

                    //var s = "{\"$type\":\"System.Exception, System.Private.CoreLib\",\"ClassName\":\"System.Exception\",\"Message\":\"boo\",\"Data\":null,\"InnerException\":{\"$type\":\"Blazor.Server.CustomExceptionX, Blazor.Server\",\"Message\":\"test\",\"Data\":{\"$type\":\"System.Collections.ListDictionaryInternal, System.Private.CoreLib\"},\"InnerException\":null,\"HelpLink\":null,\"Source\":null,\"HResult\":-2146233088,\"StackTrace\":null},\"HelpURL\":null,\"StackTraceString\":null,\"RemoteStackTraceString\":null,\"RemoteStackIndex\":0,\"ExceptionMethod\":null,\"HResult\":-2146233088,\"Source\":null,\"WatsonBuckets\":null}\r\n";
                    //var x = StateflowsJsonConverter.DeserializeObject(s);

                    throw new Exception("test");

                    Debug.WriteLine("x");
                })
                .AddTransition<SomeEvent>("state2")
                .AddTransition<Startup>("state3")
                .AddInternalTransition<ExampleRequest>(b => b
                    .AddEffect(c =>
                    {
                        c.Event.Respond(new ExampleResponse() { ResponseData = "Example response data" });
                    })
                    .AddGuard(c => throw new Exception("test"))
                )
                .AddDoActivity<Activity3>()
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

    .AddActivities(b => b
        .AddActivity<ClearingActivity>()
        .AddActivity("activity1", b => b
            //.AddAcceptEventAction<Startup>(b => b
            //    .AddControlFlow<AcceptEventActionNode<AfterOneMinute>>()
            //)
            .AddAcceptEventAction<SomeEvent>(async c =>
            {
                Debug.WriteLine("Yuppi!");
                throw new Exception("test");
            })
        )
        .AddActivity("activity2", b => b
            .AddInitializer<InitializationRequest1>(async c =>
            {
                Debug.WriteLine(c.InitializationEvent.Foo);

                return true;
            })
            .AddInitial(b => b
                .AddControlFlow("main")
            )
            .AddStructuredActivity("main", b => b
                .AddExceptionHandler<Exception>(async c =>
                {
                    Debug.WriteLine(c.Exception.Message);
                    c.Output(666);
                })
                .AddInitial(b => b
                    .AddControlFlow("action1")
                    .AddControlFlow("action3")
                )
                .AddAction(
                    "action1",
                    async c => c.Output(42),
                    b => b
                        .AddFlow<int, Flow1>("action2")
                )
                .AddAction(
                    "action2",
                    async c =>
                    {
                        Debug.WriteLine(c.GetTokensOfType<int>().First());
                        throw new Exception("test");
                    },
                    b => b.AddControlFlow("action3")
                )
                .AddAction(
                    "action3",
                     async c => { }
                )

                .AddFlow<int>("action4")
            )
            .AddAction("action4", async c => { })
        )

        .AddActivity<Activity3>("activity3")

        .AddActivity("activity4", b => b
            .AddInitializer<InitializationRequest1>(async c =>
            {
                Debug.WriteLine(c.InitializationEvent.Foo);

                return true;
            })

            .AddInitial(b => b
                .AddControlFlow("1")
            )
            .AddAction(
                "1",
                async c => c.OutputRange(Enumerable.Range(1, 10)),
                b => b.AddFlow<int>("2")
            )
            .AddAction(
                "2",
                async c => Debug.WriteLine($"{c.GetTokensOfType<int>().Count()}")
            )
            .AddAcceptEventAction<SomeEvent>(b => b
                .AddControlFlow("2")
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

namespace X
{
    [StateMachineBehavior(nameof(Default))]
    public class Default : IStateMachine
    {
        public void Build(IStateMachineBuilder builder)
            => builder
                .AddInitialState("1", b => b
                    .AddOnEntry(async c => Debug.WriteLine("dupa"))
                );
    }

    public class Structured : IStructuredActivityNodeInitialization
    {
        public Task OnInitializeAsync()
        {
            throw new NotImplementedException();
        }
    }

    public class InitializationRequest1 : Event
    {
        public string Foo { get; set; }
    }

    public class Handler : IStateMachineExceptionHandler, IStateMachineInterceptor
    {
        public Task<bool> OnStateEntryExceptionAsync(IStateActionContext context, Exception exception)
        {
            return Task.FromResult(true);
        }

        public Task AfterProcessEventAsync(IEventActionContext<Event> context)
        {
            Debug.WriteLine($">>>>>>>>>>> after processing {context.Event.Name}");

            return Task.CompletedTask;
        }

        public Task BeforeDehydrateAsync(IStateMachineActionContext context)
        {
            Debug.WriteLine($">>>>>>>>>>> dehydrate {context.ExecutionTrigger.Name}");

            return Task.CompletedTask;
        }
    }

    public class Activity3 : IActivity
    {
        public void Build(IActivityBuilder builder)
            => builder
                .AddDefaultInitializer(async c =>
                {
                    //Debug.WriteLine(c.InitializationEvent.Foo);

                    

                    return true;
                })
                .AddAcceptEventAction<SomeEvent>(async c => { }, b => b
                    .AddControlFlow("action1", b => b
                        .AddGuard(async c =>
                        {
                            throw new NotImplementedException();
                        })
                    )
                )
                .AddAction(
                    "action1",
                    async c =>
                    {
                        c.OutputRange(Enumerable.Range(0, 100));
                    },
                    b => b
                        .AddFlow<int>("chunked")
                        //.AddExceptionHandler<Exception>(async c => { })
                )
                .AddParallelActivity<int>(
                    "chunked",
                    b => b
                        .AddInput(b => b
                            .AddFlow<int>("main")
                        )
                        .AddAction(
                            "main",
                            async c =>
                            {
                                var tokens = c.GetTokensOfType<int>().Select(t => $"value: {t}");
                                c.OutputRange(tokens);
                                Debug.WriteLine($"{tokens.Count()}/{c.GetTokensOfType<int>().Count()} tokens: {string.Join(", ", c.GetTokensOfType<int>().Take(5))}...");
                                await Task.Delay(1000);
                            },
                            b => b.AddFlow<string>(OutputNode.Name)
                        )
                        .AddOutput()
                        .AddFlow<string>("action2", b => b
                            .AddGuard(async c =>
                            {
                                lock (c.Activity.LockHandle)
                                {
                                    if (c.Activity.Values.TryGet<int>("count", out var counter))
                                    {
                                        counter++;
                                        c.Activity.Values.Set<int>("count", counter);
                                    }
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
                        Debug.WriteLine(c.GetTokensOfType<string>().Count().ToString());
                        c.Activity.Values.TryGet<int>("count", out var counter);
                        Debug.WriteLine($"counter: {counter}");
                    }
                );
    }

    public class Action1 : IActionNode
    {
        public readonly Input<int> IntInput;

        private readonly GlobalValue<int> Foo = new("foo");

        private readonly StateValue<int> Bar = new("bar");

        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            if (!Foo.IsSet)
            {
                Foo.Set(42);
            }

            return Task.CompletedTask;
        }
    }

    public class Flow1 : IFlowGuard<int>
    {
        public async Task<bool> GuardAsync(int token)
        {
            return token > 40;
        }
    }









    public class PersonId { }

    public class ClearingRequest : Event
    {
        public PersonId[] PersonIds { get; set; } = new PersonId[0];
    }


    public class ClearingActivity : IActivity
    {
        public void Build(IActivityBuilder builder)
        {
            builder
                .AddAcceptEventAction<ClearingRequest>(
                    async c => c.OutputRange(c.Event.PersonIds),
                    b => b.AddFlow<PersonId>("clearing-loop")
                )
                .AddIterativeActivity<PersonId>(
                    "clearing-loop",
                    b => b
                        .AddInput(b => b
                            .AddFlow<PersonId>("clear")
                        )
                        //.AddInitial(b => b
                        //    .AddControlFlow<AcceptEventActionNode<AfterOneMinute>>()
                        //)
                        //.AddTimeEventAction<AfterOneMinute>(b => b
                        //    .AddControlFlow("clear")
                        //)
                        .AddAction("clear", async c => Debug.WriteLine($"cleared {c.GetTokensOfType<PersonId>().Count()} dossiers"))

                        .AddControlFlow("finish"),
                    10
                )
                .AddAction("finish", async c => Debug.WriteLine("finished"))
                ;
        }
    }







































}