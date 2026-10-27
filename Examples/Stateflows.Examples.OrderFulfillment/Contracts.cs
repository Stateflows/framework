using Stateflows.Extensions.MinimalAPIs.Attributes;

namespace Stateflows.Examples.OrderFulfillment;

public enum Scenario
{
    HappyPath,
    PaymentDeclined,
    OutOfStock,
    PreparationFailed,
    CaptureFailed,
    DispatchFailed,
    TransientFailures,
    DeliveryNeedsAttention,
    CompensationNeedsAttention
}

public enum Operation
{
    AuthorizePayment,
    ReserveStock,
    PrepareShipment,
    CapturePayment,
    DispatchShipment,
    DeliverShipment,
    VoidAuthorization,
    RefundPayment,
    ReleaseStock
}

public enum PaymentState { None, Authorized, Captured, Voided, Refunded }
public enum StockState { None, Reserved, Released, Dispatched }
public enum ShipmentState { None, Prepared, Dispatched, Delivered }

public sealed record OrderLine(string Sku, int Quantity, decimal UnitPrice);

public sealed record SubmitOrder(
    string CustomerEmail,
    OrderLine[] Lines,
    Scenario Scenario = Scenario.HappyPath,
    int CallbackDelayMilliseconds = 500);

public interface IReasonable
{
    string Reason { get; }
}

public interface IOperationEvent
{
    Operation Operation { get; }
}

public sealed record CancelOrder(string Reason) : IReasonable;
public sealed record RetryDelivery;
public sealed record RetryCompensation;

[NoApiMapping]
public sealed record StepSucceeded(Operation Operation) : IOperationEvent;

[NoApiMapping]
public sealed record StepFailed(Operation Operation, string Reason) : IOperationEvent, IReasonable;

[NoApiMapping]
public sealed record CompensationFinished;

[NoApiMapping]
public sealed record CompensationFailed(string Reason) : IReasonable;

[NoApiMapping]
public sealed record InvoiceReady;

public sealed record Attempt(Operation Operation, int Number, string Outcome);
public sealed record AuditEntry(DateTimeOffset At, string Message);
public sealed record Invoice(string Number, decimal Total);

public sealed record OrderSnapshot(
    SubmitOrder? Order,
    string Stage,
    PaymentState Payment,
    StockState Stock,
    ShipmentState Shipment,
    bool CancellationRequested,
    string? FailureReason,
    Attempt[] Attempts,
    AuditEntry[] Audit,
    Invoice? Invoice)
{
    public static OrderSnapshot Empty => new(
        null, "NotSubmitted", PaymentState.None, StockState.None, ShipmentState.None,
        false, null, [], [], null);

    public decimal Total => Order?.Lines.Sum(line => line.Quantity * line.UnitPrice) ?? 0m;
}

[NoApiMapping]
public sealed record OrderCreated(SubmitOrder Order);
[NoApiMapping]
public sealed record AttemptRecorded(Attempt Attempt);
[NoApiMapping]
public sealed record StepApplied(Operation Operation);
[NoApiMapping]
public sealed record StageChanged(string Stage);
[NoApiMapping]
public sealed record OrderStopped(bool Cancelled, string Reason);
[NoApiMapping]
public sealed record AuditAppended(string Message);
[NoApiMapping]
public sealed record InvoiceIssued(Invoice Invoice);
