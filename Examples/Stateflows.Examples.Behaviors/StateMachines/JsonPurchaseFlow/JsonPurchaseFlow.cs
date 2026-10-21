using System.Reflection;
using Microsoft.Extensions.Logging;
using Stateflows.Common;
using Stateflows.Common.Json;
using Stateflows.StateMachines;
using Stateflows.StateMachines.Json;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.Examples.Behaviors.StateMachines.JsonPurchaseFlow;

public sealed class PurchaseSubmitted
{
    public string OrderId { get; set; } = string.Empty;
}

public sealed class PurchaseCancelled
{
    public string OrderId { get; set; } = string.Empty;
}

public sealed class PurchaseStateHandler(ILogger<PurchaseStateHandler> logger) :
    IStateEntry, IStateExit, IConfigurable<string>
{
    public string Configuration { private get; set; } = string.Empty;

    public Task OnEntryAsync()
    {
        logger.LogInformation("{Message}", Configuration);
        return Task.CompletedTask;
    }

    public Task OnExitAsync()
    {
        logger.LogInformation("{Message}", Configuration);
        return Task.CompletedTask;
    }
}

public sealed class PurchaseEffect(ILogger<PurchaseEffect> logger) :
    ITransitionEffect<PurchaseSubmitted>,
    ITransitionEffect<PurchaseCancelled>,
    IConfigurable<string>
{
    public string Configuration { private get; set; } = string.Empty;

    public Task EffectAsync(PurchaseSubmitted @event)
    {
        logger.LogInformation("{Message} OrderId={OrderId}", Configuration, @event.OrderId);
        return Task.CompletedTask;
    }

    public Task EffectAsync(PurchaseCancelled @event)
    {
        logger.LogInformation("{Message} OrderId={OrderId}", Configuration, @event.OrderId);
        return Task.CompletedTask;
    }
}

public static class JsonPurchaseFlowRegistration
{
    private const string ModelResourceName =
        "Stateflows.Examples.Behaviors.StateMachines.JsonPurchaseFlow.model.json";

    public static IStateMachinesBuilder AddJsonPurchaseFlow(this IStateMachinesBuilder builder)
    {
        using var stream = typeof(JsonPurchaseFlowRegistration).Assembly.GetManifestResourceStream(ModelResourceName)
            ?? throw new InvalidOperationException($"Embedded JSON model '{ModelResourceName}' was not found.");
        using var reader = new StreamReader(stream);
        var json = reader.ReadToEnd();
        var options = new JsonOptions()
            .AddType<PurchaseSubmitted>("purchase.submitted")
            .AddType<PurchaseCancelled>("purchase.cancelled")
            .AddType<PurchaseStateHandler>("purchase.stateHandler")
            .AddType<PurchaseEffect>("purchase.effect");

        return builder.AddStateMachineJson(json, options);
    }
}
