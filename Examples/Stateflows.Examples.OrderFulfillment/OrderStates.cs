using Stateflows.Common;
using Stateflows.StateMachines;

namespace Stateflows.Examples.OrderFulfillment;

public sealed class Processing : ICompositeState;
public sealed class AuthorizingPayment : IState;
public sealed class PaymentReady : IState;
public sealed class ReservingStock : IState;
public sealed class StockReady : IState;
public sealed class CompensationOutcome : IVertex;

public abstract class OrderStage(IStateContext state, IBehaviorContext behavior) : IStateEntry
{
    public Task OnEntryAsync() => OrderData.MutateAsync(behavior, new StageChanged(state.Name));
}

public sealed class Checking(IStateContext state, IBehaviorContext behavior)
    : OrderStage(state, behavior), IOrthogonalState;

public sealed class PreparingShipment(IStateContext state, IBehaviorContext behavior)
    : OrderStage(state, behavior);

public sealed class CapturingPayment(IStateContext state, IBehaviorContext behavior)
    : OrderStage(state, behavior);

public sealed class DispatchingShipment(IStateContext state, IBehaviorContext behavior)
    : OrderStage(state, behavior);

public sealed class InTransit(IStateContext state, IBehaviorContext behavior)
    : OrderStage(state, behavior);

public sealed class Invoicing(IStateContext state, IBehaviorContext behavior)
    : OrderStage(state, behavior);

public sealed class NeedsAttention(IStateContext state, IBehaviorContext behavior)
    : OrderStage(state, behavior);

public sealed class Compensating(IStateContext state, IBehaviorContext behavior)
    : OrderStage(state, behavior);

public sealed class CompensationNeedsAttention(IStateContext state, IBehaviorContext behavior)
    : OrderStage(state, behavior);

public sealed class Completed(IStateContext state, IBehaviorContext behavior)
    : OrderStage(state, behavior);

public sealed class Cancelled(IStateContext state, IBehaviorContext behavior)
    : OrderStage(state, behavior);

public sealed class Failed(IStateContext state, IBehaviorContext behavior)
    : OrderStage(state, behavior);
