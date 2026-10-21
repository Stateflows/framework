using Microsoft.Extensions.Logging;
using Stateflows.Activities;
using Stateflows.Activities.Json;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Common;
using Stateflows.Common.Json;

namespace Stateflows.Examples.Behaviors.Activities.JsonPurchaseProcessing;

public sealed class ProcessPurchase
{
    public string OrderId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public sealed class PurchaseReceipt
{
    public string OrderId { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public sealed class ReceivePurchase(IOutputTokens<ProcessPurchase> output) : IAcceptEventActionNode<ProcessPurchase>
{
    public Task ExecuteAsync(ProcessPurchase @event, CancellationToken cancellationToken)
    {
        output.Add(@event);
        return Task.CompletedTask;
    }
}

public sealed class CreateReceipt : IFlowTransformation<ProcessPurchase, PurchaseReceipt>, IConfigurable<string>
{
    public string Configuration { private get; set; } = string.Empty;

    public Task<PurchaseReceipt> TransformAsync(ProcessPurchase token)
        => Task.FromResult(new PurchaseReceipt
        {
            OrderId = token.OrderId,
            Reference = Configuration + token.OrderId,
            Amount = token.Amount
        });
}

public sealed class RecordReceipt(IInputTokens<PurchaseReceipt> input, ILogger<RecordReceipt> logger) :
    IActionNode, IConfigurable<string>
{
    public string Configuration { private get; set; } = string.Empty;

    public Task ExecuteAsync(CancellationToken cancellationToken)
    {
        foreach (var receipt in input)
            logger.LogInformation("{Message} OrderId={OrderId} Reference={Reference} Amount={Amount}",
                Configuration, receipt.OrderId, receipt.Reference, receipt.Amount);
        return Task.CompletedTask;
    }
}

public static class JsonPurchaseProcessingRegistration
{
    private const string ModelResourceName =
        "Stateflows.Examples.Behaviors.Activities.JsonPurchaseProcessing.model.json";

    public static IActivitiesBuilder AddJsonPurchaseProcessing(this IActivitiesBuilder builder)
    {
        using var stream = typeof(JsonPurchaseProcessingRegistration).Assembly.GetManifestResourceStream(ModelResourceName)
            ?? throw new InvalidOperationException($"Embedded JSON model '{ModelResourceName}' was not found.");
        using var reader = new StreamReader(stream);
        var options = new JsonOptions()
            .AddType<ProcessPurchase>("purchase.request")
            .AddType<PurchaseReceipt>("purchase.receipt")
            .AddType<ReceivePurchase>("purchase.receive")
            .AddType<CreateReceipt>("purchase.createReceipt")
            .AddType<RecordReceipt>("purchase.recordReceipt");

        return builder.AddActivityJson(reader.ReadToEnd(), options);
    }
}
