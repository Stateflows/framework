using System.Reflection;
using MassTransit;
using Stateflows;
using Stateflows.StateMachines;
using Examples.Common;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services
    .AddStateflows(b => b
        .AddStateMachines(b => b
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
    );

builder.Services.AddMassTransit(c => c
    .AddStateflowsTransport(Assembly.GetEntryAssembly()?.GetName().Name)
    .UsingRabbitMq((context, config) =>
    {
        config.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        config.ConfigureEndpoints(context);
    })
);

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapControllers();

app.Run();
