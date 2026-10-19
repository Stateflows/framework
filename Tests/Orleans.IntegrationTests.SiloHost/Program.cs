using System.Collections.Concurrent;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Hosting;
using Stateflows;
using Stateflows.Actions;
using Stateflows.Activities;
using Stateflows.Common;
using Stateflows.Common.Interfaces;
using Stateflows.StateMachines;

var arguments = ParseArguments(args);
var node = GetRequiredArgument(arguments, "node");
var siloPort = GetPort(arguments, "silo-port");
var gatewayPort = GetPort(arguments, "gateway-port");
var httpPort = GetPort(arguments, "http-port");
var primarySiloPort = GetOptionalPort(arguments, "primary-silo-port");
var clusterId = GetRequiredArgument(arguments, "cluster-id");
var serviceId = GetRequiredArgument(arguments, "service-id");
var effects = new ConcurrentDictionary<string, ExecutionEffect>();

var builder = WebApplication.CreateBuilder();
builder.Logging.SetMinimumLevel(LogLevel.Warning);
builder.WebHost.UseUrls($"http://127.0.0.1:{httpPort}");

builder.Services.AddStateflows(stateflows => stateflows
    .AddActions(actions => actions.AddAction(
        $"{node}-action",
        _ =>
        {
            RecordEffect($"{node}-action");
            return Task.CompletedTask;
        }))
    .AddActivities(activities => activities.AddActivity(
        $"{node}-activity",
        activity => activity
            .AddInitial(initial => initial.AddControlFlow("record-effect"))
            .AddAction(
                "record-effect",
                _ =>
                {
                    RecordEffect($"{node}-activity");
                    return Task.CompletedTask;
                },
                action => action.AddControlFlow<FinalNode>())
            .AddFinal()))
    .AddStateMachines(machines => machines.AddStateMachine(
        $"{node}-machine",
        machine => machine
            .AddInitialState("ready", state => state.AddTransition<CrossProcessEvent>(
                "complete",
                transition => transition.AddEffect(_ =>
                {
                    RecordEffect($"{node}-machine");
                    return Task.CompletedTask;
                })))
            .AddState("complete")))
);

builder.Host.UseOrleans(silo =>
{
    var primarySiloEndpoint = primarySiloPort is int port
        ? new IPEndPoint(IPAddress.Loopback, port)
        : null;

    silo.UseLocalhostClustering(siloPort, gatewayPort, primarySiloEndpoint, serviceId, clusterId)
        .AddMemoryGrainStorage("stateflows")
        .UseInMemoryReminderService()
        .AddStateflowsRuntime();
});

void RecordEffect(string name)
{
    effects.AddOrUpdate(
        name,
        _ => new ExecutionEffect(Environment.ProcessId, 1),
        (_, current) => new ExecutionEffect(Environment.ProcessId, current.Count + 1));
}

var app = builder.Build();
app.MapGet("/ready", () => Results.Ok(new ReadyResponse(Environment.ProcessId, node)));
app.MapGet("/discovery/{target}", (string target, IServiceScopeFactory scopeFactory) =>
{
    using var scope = scopeFactory.CreateScope();
    var services = scope.ServiceProvider;
    var discovered = services.GetRequiredService<IActionLocator>()
        .TryLocateAction(new ActionId($"{target}-action", "remote"), out _)
                     // && services.GetRequiredService<IActionLocator>()
                     //     .TryLocateAction(new ActionId($"{target}-action-x", "remote"), out _)
        && services.GetRequiredService<IActivityLocator>()
            .TryLocateActivity(new ActivityId($"{target}-activity", "remote"), out _)
        && services.GetRequiredService<IStateMachineLocator>()
            .TryLocateStateMachine(new StateMachineId($"{target}-machine", "remote"), out _);

    return Results.Ok(new DiscoveryResponse(discovered));
});
app.MapPost("/execute/{target}", async (string target, IServiceScopeFactory scopeFactory) =>
{
    using var scope = scopeFactory.CreateScope();
    var services = scope.ServiceProvider;

    if (!services.GetRequiredService<IActionLocator>()
            .TryLocateAction(new ActionId($"{target}-action", "remote"), out var action)
        || !services.GetRequiredService<IActivityLocator>()
            .TryLocateActivity(new ActivityId($"{target}-activity", "remote"), out var activity)
        || !services.GetRequiredService<IStateMachineLocator>()
            .TryLocateStateMachine(new StateMachineId($"{target}-machine", "remote"), out var stateMachine))
    {
        return Results.NotFound(new { message = $"Definitions for '{target}' are not discovered." });
    }

    using (action)
    using (activity)
    using (stateMachine)
    {
        await action.ExecuteAsync();
        var activityStatus = (await activity.SendAsync(new Initialize())).Status;
        var initializationStatus = (await stateMachine.SendAsync(new Initialize())).Status;
        var initialState = (await stateMachine.GetStatusAsync()).Response.CurrentStates.Value;
        var transitionStatus = (await stateMachine.SendAsync(new CrossProcessEvent())).Status;
        var currentState = (await stateMachine.GetStatusAsync()).Response.CurrentStates.Value;

        return Results.Ok(new ExecutionResponse(
            Environment.ProcessId,
            target,
            activityStatus.ToString(),
            initializationStatus.ToString(),
            initialState,
            transitionStatus.ToString(),
            currentState));
    }
});
app.MapGet("/effects", () => Results.Ok(new EffectsResponse(Environment.ProcessId, effects.ToDictionary())));
app.MapPost("/shutdown", (IHostApplicationLifetime lifetime) =>
{
    _ = Task.Run(async () =>
    {
        await Task.Delay(TimeSpan.FromMilliseconds(250));
        lifetime.StopApplication();
    });

    return Results.Accepted();
});

await app.RunAsync();

static Dictionary<string, string> ParseArguments(string[] args)
{
    var result = new Dictionary<string, string>(StringComparer.Ordinal);
    for (var index = 0; index < args.Length; index += 2)
    {
        if (index + 1 >= args.Length || !args[index].StartsWith("--", StringComparison.Ordinal))
        {
            throw new ArgumentException($"Expected --name value argument at position {index}.");
        }

        result.Add(args[index][2..], args[index + 1]);
    }

    return result;
}

static string GetRequiredArgument(IReadOnlyDictionary<string, string> arguments, string name)
    => arguments.TryGetValue(name, out var value)
        ? value
        : throw new ArgumentException($"Missing --{name} argument.");

static int GetPort(IReadOnlyDictionary<string, string> arguments, string name)
    => int.TryParse(GetRequiredArgument(arguments, name), out var port)
        ? port
        : throw new ArgumentException($"Invalid --{name} value.");

static int? GetOptionalPort(IReadOnlyDictionary<string, string> arguments, string name)
    => arguments.TryGetValue(name, out var value)
        ? int.TryParse(value, out var port)
            ? port
            : throw new ArgumentException($"Invalid --{name} value.")
        : null;

public sealed class CrossProcessEvent;

public sealed record ReadyResponse(int ProcessId, string Node);
public sealed record DiscoveryResponse(bool Discovered);
public sealed record ExecutionResponse(
    int SourceProcessId,
    string Target,
    string ActivityStatus,
    string InitializationStatus,
    string InitialState,
    string TransitionStatus,
    string CurrentState);
public sealed record ExecutionEffect(int ProcessId, int Count);
public sealed record EffectsResponse(int ProcessId, IReadOnlyDictionary<string, ExecutionEffect> Effects);
