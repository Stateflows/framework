using Stateflows.Common;
using Stateflows.StateMachines;

namespace Stateflows.Examples.OrderFulfillment;

public abstract class StopOrder(IBehaviorContext behavior, bool cancelled) : ITransitionEffect<IReasonable>
{
    public Task EffectAsync(IReasonable @event) =>
        OrderData.MutateAsync(behavior, new OrderStopped(cancelled, @event.Reason));
}

public sealed class CancelOrderTransition(IBehaviorContext behavior) : StopOrder(behavior, true);

public sealed class FailOrderTransition(IBehaviorContext behavior) : StopOrder(behavior, false);

public sealed class AppendReason(IBehaviorContext behavior) : ITransitionEffect<IReasonable>
{
    public Task EffectAsync(IReasonable @event) =>
        OrderData.MutateAsync(behavior, new AuditAppended(@event.Reason));
}

public abstract class ApplySuccessfulStep(IBehaviorContext behavior, Operation operation)
    : ITransitionGuard<IOperationEvent>, ITransitionEffect<IOperationEvent>
{
    public Task<bool> GuardAsync(IOperationEvent @event) =>
        Task.FromResult(@event.Operation == operation);

    public Task EffectAsync(IOperationEvent @event) =>
        OrderData.MutateAsync(behavior, new StepApplied(@event.Operation));
}

public sealed class PaymentAuthorized(IBehaviorContext behavior)
    : ApplySuccessfulStep(behavior, Operation.AuthorizePayment);

public sealed class StockReserved(IBehaviorContext behavior)
    : ApplySuccessfulStep(behavior, Operation.ReserveStock);

public sealed class ShipmentPrepared(IBehaviorContext behavior)
    : ApplySuccessfulStep(behavior, Operation.PrepareShipment);

public sealed class PaymentCaptured(IBehaviorContext behavior)
    : ApplySuccessfulStep(behavior, Operation.CapturePayment);

public sealed class ShipmentDispatched(IBehaviorContext behavior)
    : ApplySuccessfulStep(behavior, Operation.DispatchShipment);

public sealed class ShipmentDelivered(IBehaviorContext behavior)
    : ApplySuccessfulStep(behavior, Operation.DeliverShipment);

public sealed class ResourcesReady(IBehaviorContext behavior) : IDefaultTransitionGuard
{
    public async Task<bool> GuardAsync()
    {
        var snapshot = await OrderData.ReadAsync(behavior);
        return snapshot.Payment == PaymentState.Authorized && snapshot.Stock == StockState.Reserved;
    }
}

public sealed class CancellationRequested(IBehaviorContext behavior) : IDefaultTransitionGuard
{
    public async Task<bool> GuardAsync() =>
        (await OrderData.ReadAsync(behavior)).CancellationRequested;
}
