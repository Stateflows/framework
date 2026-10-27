using Stateflows.Actions;
using Stateflows.Activities;
using Stateflows.Common;
using Stateflows.Common.Interfaces;

namespace Stateflows.Examples.OrderFulfillment;

public static class OrderData
{
    public static async Task<OrderSnapshot> ReadAsync<T>(IEntityOperations<T> context)
    {
        var (success, snapshot) = await context.TryGetProjectionAsync<IOrderData, OrderSnapshot>();
        return success && snapshot is not null
            ? snapshot
            : throw new InvalidOperationException("The group's order projection could not be read.");
    }

    public static async Task MutateAsync<T, TMutation>(IEntityOperations<T> context, TMutation mutation)
    {
        if (!await context.TryMutateAsync<IOrderData, TMutation>(mutation))
            throw new InvalidOperationException($"Order mutation {typeof(TMutation).Name} was not consumed.");
    }
}

public interface IOrderSimulator
{
    Task<string?> SimulateAsync(
        IOwnerBehaviorContext owner, Operation operation, CancellationToken cancellationToken);
}

public sealed class OrderSimulator : IOrderSimulator
{
    public const int MaximumAttempts = 3;

    public async Task<string?> SimulateAsync(
        IOwnerBehaviorContext owner, Operation operation, CancellationToken cancellationToken)
    {
        for (var localAttempt = 1; localAttempt <= MaximumAttempts; localAttempt++)
        {
            var snapshot = await OrderData.ReadAsync(owner);
            var order = snapshot.Order ?? throw new InvalidOperationException("No submitted order exists.");
            var number = snapshot.Attempts.Count(attempt => attempt.Operation == operation) + 1;
            await Task.Delay(order.CallbackDelayMilliseconds, cancellationToken);
            var outcome = Outcome(order.Scenario, operation, number);
            await OrderData.MutateAsync(owner, new AttemptRecorded(new(operation, number, outcome)));

            if (outcome == "Succeeded") return null;
            if (outcome == "PermanentFailure" || localAttempt == MaximumAttempts)
                return $"{operation} failed ({outcome}, attempt {number}).";

            await Task.Delay(order.CallbackDelayMilliseconds * localAttempt, cancellationToken);
        }

        throw new InvalidOperationException("The retry loop exited without an outcome.");
    }

    private static string Outcome(Scenario scenario, Operation operation, int attempt)
    {
        if (scenario == Scenario.TransientFailures && attempt == 1)
            return "TransientFailure";
        if (scenario == Scenario.DeliveryNeedsAttention &&
            operation == Operation.DeliverShipment && attempt <= MaximumAttempts)
            return "TransientFailure";
        if (scenario == Scenario.CompensationNeedsAttention &&
            operation == Operation.RefundPayment && attempt <= MaximumAttempts)
            return "TransientFailure";

        return (scenario, operation) switch
        {
            (Scenario.PaymentDeclined, Operation.AuthorizePayment) => "PermanentFailure",
            (Scenario.OutOfStock, Operation.ReserveStock) => "PermanentFailure",
            (Scenario.PreparationFailed, Operation.PrepareShipment) => "PermanentFailure",
            (Scenario.CaptureFailed, Operation.CapturePayment) => "TransientFailure",
            (Scenario.DispatchFailed or Scenario.CompensationNeedsAttention, Operation.DispatchShipment) =>
                "PermanentFailure",
            _ => "Succeeded"
        };
    }
}

public abstract class OperationWorker(
    IOwnerBehaviorContext owner, IOrderSimulator simulator, Operation operation)
{
    protected IOwnerBehaviorContext Owner { get; } = owner;

    public virtual async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var failure = await simulator.SimulateAsync(Owner, operation, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        if (failure is null)
            Owner.Send(new StepSucceeded(operation));
        else
            Owner.Send(new StepFailed(operation, failure));
    }
}

public sealed class AuthorizePaymentAction(IOwnerBehaviorContext owner, IOrderSimulator simulator)
    : OperationWorker(owner, simulator, Operation.AuthorizePayment), IAction;

public sealed class CapturePaymentAction(IOwnerBehaviorContext owner, IOrderSimulator simulator)
    : OperationWorker(owner, simulator, Operation.CapturePayment), IAction;

public sealed class DispatchShipmentAction(IOwnerBehaviorContext owner, IOrderSimulator simulator)
    : OperationWorker(owner, simulator, Operation.DispatchShipment), IAction;

public sealed class DeliverShipmentAction(IOwnerBehaviorContext owner, IOrderSimulator simulator)
    : OperationWorker(owner, simulator, Operation.DeliverShipment), IAction;

public sealed class CompensateOrderAction(IOwnerBehaviorContext owner, IOrderSimulator simulator) : IAction
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var snapshot = await OrderData.ReadAsync(owner);
        var operations = new List<Operation>();
        if (snapshot.Payment == PaymentState.Captured) operations.Add(Operation.RefundPayment);
        if (snapshot.Payment == PaymentState.Authorized) operations.Add(Operation.VoidAuthorization);
        if (snapshot.Stock == StockState.Reserved) operations.Add(Operation.ReleaseStock);

        foreach (var operation in operations)
        {
            var failure = await simulator.SimulateAsync(owner, operation, cancellationToken);
            if (failure is not null)
            {
                owner.Send(new CompensationFailed(failure));
                return;
            }

            await OrderData.MutateAsync(owner, new StepApplied(operation));
        }

        owner.Send(new CompensationFinished());
    }
}

public abstract class AppendAuditAction(IOwnerBehaviorContext owner, string message) : IAction
{
    public Task ExecuteAsync(CancellationToken cancellationToken) =>
        OrderData.MutateAsync(owner, new AuditAppended(message));
}

public sealed class RecordCancellationAction(IOwnerBehaviorContext owner)
    : AppendAuditAction(owner, "Cancellation accepted before dispatch.");

public sealed class RecordCompletionAction(IOwnerBehaviorContext owner)
    : AppendAuditAction(owner, "Delivery receipt recorded and invoice issued.");

public sealed class CheckInventory(IOwnerBehaviorContext owner) : IActionNode
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var snapshot = await OrderData.ReadAsync(owner);
        await OrderData.MutateAsync(owner,
            new AuditAppended($"Checking {snapshot.Order!.Lines.Length} inventory lines."));
    }
}

public sealed class Reserve(IOwnerBehaviorContext owner, IOrderSimulator simulator)
    : OperationWorker(owner, simulator, Operation.ReserveStock), IActionNode;

public sealed class BuildPackingList(IOwnerBehaviorContext owner, IOutputTokens<int> units) : IActionNode
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var snapshot = await OrderData.ReadAsync(owner);
        units.Add(snapshot.Order!.Lines.Sum(line => line.Quantity));
    }
}

public sealed class PackAndLabel(
    IOwnerBehaviorContext owner, IOrderSimulator simulator, IInputTokens<int> units)
    : OperationWorker(owner, simulator, Operation.PrepareShipment), IActionNode
{
    public override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await OrderData.MutateAsync(Owner,
            new AuditAppended($"Packing and labelling {units.Single()} units."));
        await base.ExecuteAsync(cancellationToken);
    }
}

public sealed class CalculateInvoice(IOwnerBehaviorContext owner, IOutputTokens<Invoice> invoices) : IActionNode
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var snapshot = await OrderData.ReadAsync(owner);
        invoices.Add(new Invoice($"INV-{owner.Id.Instance}", snapshot.Total));
    }
}

public sealed class StoreInvoice(IOwnerBehaviorContext owner, IInputTokens<Invoice> invoices) : IActionNode
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await OrderData.MutateAsync(owner, new InvoiceIssued(invoices.Single()));
        owner.Send(new InvoiceReady());
    }
}

public sealed class ReserveStockActivity : IActivity
{
    public static void Build(IActivityBuilder activity) => activity
        .AddInitial(node => node.AddControlFlow<CheckInventory>())
        .AddAction<CheckInventory>(node => node.AddControlFlow<Reserve>())
        .AddAction<Reserve>(node => node.AddControlFlow<FinalNode>())
        .AddFinal();
}

public sealed class PrepareShipmentActivity : IActivity
{
    public static void Build(IActivityBuilder activity) => activity
        .AddInitial(node => node.AddControlFlow<BuildPackingList>())
        .AddAction<BuildPackingList>(node => node.AddFlow<int, PackAndLabel>())
        .AddAction<PackAndLabel>(node => node.AddControlFlow<FinalNode>())
        .AddFinal();
}

public sealed class IssueInvoiceActivity : IActivity
{
    public static void Build(IActivityBuilder activity) => activity
        .AddInitial(node => node.AddControlFlow<CalculateInvoice>())
        .AddAction<CalculateInvoice>(node => node.AddFlow<Invoice, StoreInvoice>())
        .AddAction<StoreInvoice>(node => node.AddControlFlow<FinalNode>())
        .AddFinal();
}
