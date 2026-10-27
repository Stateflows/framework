using Stateflows.Entities.Attributes;
using Stateflows.Entities.Enums;

namespace Stateflows.Examples.OrderFulfillment;

public interface IOrderData
{
    [Field]
    OrderSnapshot Data { get; set; }

    [DefaultInitializer]
    void Initialize() => Data = OrderSnapshot.Empty;

    [Mutation]
    void Create(OrderCreated mutation)
    {
        if (Data.Order is not null)
            throw new InvalidOperationException("The order has already been submitted.");

        Data = Data with { Order = mutation.Order };
    }

    [Mutation]
    void Record(AttemptRecorded mutation) =>
        Data = Data with { Attempts = [.. Data.Attempts, mutation.Attempt] };

    [Mutation]
    void ChangeStage(StageChanged mutation) =>
        Data = Data with
        {
            Stage = mutation.Stage,
            Audit = [.. Data.Audit, new(DateTimeOffset.UtcNow, $"Entered {mutation.Stage}.")]
        };

    [Mutation]
    void Stop(OrderStopped mutation) =>
        Data = Data with
        {
            CancellationRequested = mutation.Cancelled,
            FailureReason = mutation.Reason,
            Audit = [.. Data.Audit, new(DateTimeOffset.UtcNow, mutation.Reason)]
        };

    [Mutation]
    void Append(AuditAppended mutation) =>
        Data = Data with { Audit = [.. Data.Audit, new(DateTimeOffset.UtcNow, mutation.Message)] };

    [Mutation]
    void Issue(InvoiceIssued mutation) => Data = Data with { Invoice = mutation.Invoice };

    [Mutation]
    void Apply(StepApplied mutation)
    {
        Data = mutation.Operation switch
        {
            Operation.AuthorizePayment when Data.Payment == PaymentState.None =>
                Data with { Payment = PaymentState.Authorized },
            Operation.ReserveStock when Data.Stock == StockState.None =>
                Data with { Stock = StockState.Reserved },
            Operation.PrepareShipment when Data.Stock == StockState.Reserved =>
                Data with { Shipment = ShipmentState.Prepared },
            Operation.CapturePayment when Data.Payment == PaymentState.Authorized =>
                Data with { Payment = PaymentState.Captured },
            Operation.DispatchShipment when Data.Payment == PaymentState.Captured &&
                Data.Shipment == ShipmentState.Prepared && Data.Stock == StockState.Reserved =>
                Data with { Shipment = ShipmentState.Dispatched, Stock = StockState.Dispatched },
            Operation.DeliverShipment when Data.Shipment == ShipmentState.Dispatched =>
                Data with { Shipment = ShipmentState.Delivered },
            Operation.VoidAuthorization when Data.Payment == PaymentState.Authorized =>
                Data with { Payment = PaymentState.Voided },
            Operation.RefundPayment when Data.Payment == PaymentState.Captured &&
                Data.Shipment is ShipmentState.None or ShipmentState.Prepared =>
                Data with { Payment = PaymentState.Refunded },
            Operation.ReleaseStock when Data.Stock == StockState.Reserved =>
                Data with { Stock = StockState.Released },
            _ => throw new InvalidOperationException($"Cannot apply {mutation.Operation} to the current order.")
        };
        Data = Data with
        {
            Audit = [.. Data.Audit, new(DateTimeOffset.UtcNow, $"Applied {mutation.Operation}.")]
        };
    }

    [Projection(PublishScope.Owner)]
    OrderSnapshot View => Data;
}
