using Examples.SharedBehaviors;
using Examples.Storage;
using Medallion.Threading.SqlServer;
using Stateflows;
using Stateflows.StateMachines;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddStateflows(b => b
    .AddStateMachines(b => b
        .AddStateMachine<StateMachine1>()
        .AddStateMachine<StateMachine2>()
    )
    .AddDefaultInstance<StateMachine2>()
    .AddStorage()
    .AddDistributedLock(async (serviceProvider, key) =>
    {
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var connectionString = configuration.GetConnectionString("StateflowsDatabase");
        return new SqlDistributedLock(key, connectionString);
    })
    .SetEnvironment(
        builder.Environment.IsDevelopment()
            ? $"{StateflowsEnvironments.Development}.{Environment.MachineName}"
            : StateflowsEnvironments.Production
    )
);

var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.MapControllers();
app.Run();
