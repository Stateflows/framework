using Stateflows;
using Stateflows.StateMachines;
using Stateflows.StateMachines.Sync;
using Cosmos.DependencyInjection;
using Examples.Common;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddMetaServiceInHttpContext();

builder.Services
    .AddStateflows(b => b
        .AddPlantUml()
        .AddStateMachines(b => b
            .AddStateMachine("stateMachine1", b => b
                .AddOnInitialize(c =>
                {
                    Console.WriteLine("Service2");
                    return false;
                })
                .AddInitialState("state1", b => b
                    .AddTransition<SomeEvent>("state2")
                    .AddInternalTransition<ExampleRequest>(b => b
                        .AddEffect(c => c.Event.Respond(new ExampleResponse() { ResponseData = "Example response data" }))
                    )
                )
                .AddState("state2", b => b
                    .AddTransition<SomeEvent>("state1")
                )
            )
            .AddStateMachine("stateMachine2", b => b
                .AddInitialState("state1", b => b
                    .AddTransition<SomeEvent>("state2")
                    .AddInternalTransition<ExampleRequest>(b => b
                        .AddEffect(c => c.Event.Respond(new ExampleResponse() { ResponseData = "Example response data" }))
                    )
                )
                .AddState("state2", b => b
                    .AddTransition<SomeEvent>("state1")
                )
            )
        )
    )
    .AddEventBus(b => b
        .AddDefault()
        .AddStateflowsTransport()
    );

var app = builder.Build();

app.MapControllers();

app.Run();
