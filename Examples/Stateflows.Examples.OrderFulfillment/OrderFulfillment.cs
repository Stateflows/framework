using Stateflows.Extensions.MinimalAPIs;
using Stateflows.Extensions.MinimalAPIs.Interfaces;
using Stateflows.StateMachines;

namespace Stateflows.Examples.OrderFulfillment;

public sealed class OrderFulfillment : IStateMachine
{
    public static readonly string Name = StateMachine<OrderFulfillment>.Name;

    public static void Build(IStateMachineBuilder machine) => machine
        .AddEntity<IOrderData>()
        .AddInitializer<SubmitOrder>(async context =>
        {
            await OrderData.MutateAsync(context.Behavior, new OrderCreated(context.InitializationEvent));
            return true;
        })
        .AddEndpoints(endpoints =>
        {
            endpoints.AddGet("/summary", async (IStateMachineEndpointContext context) =>
                Results.Ok(await OrderData.ReadAsync(context.Behavior)));
            endpoints.AddGet("/diagram", (IStateMachineEndpointContext context) =>
                OrderEndpoints.DiagramAsync(context));
            endpoints.AddGet("/diagrams/reservation", (IStateMachineEndpointContext context) =>
                OrderEndpoints.DiagramAsync(context, State<ReservingStock>.Name));
            endpoints.AddGet("/diagrams/preparation", (IStateMachineEndpointContext context) =>
                OrderEndpoints.DiagramAsync(context, State<PreparingShipment>.Name));
        })
        .AddInitialCompositeState<Processing>(processing => processing
            .AddTransition<CancelOrder, CancelOrderTransition, Compensating>(transition => transition
                .AddEffectAction<RecordCancellationAction>())
            .AddTransition<StepFailed, FailOrderTransition, Compensating>()
            .AddInitialOrthogonalState<Checking>(checks => checks
                .AddDefaultTransition<ResourcesReady, PreparingShipment>()
                .AddRegion(payment => payment
                    .AddInitialState<AuthorizingPayment>(state => state
                        .AddDoAction<AuthorizePaymentAction>()
                        .AddTransition<StepSucceeded, PaymentAuthorized, PaymentReady>())
                    .AddState<PaymentReady>())
                .AddRegion(stock => stock
                    .AddInitialState<ReservingStock>(state => state
                        .AddDoActivity<ReserveStockActivity>()
                        .AddTransition<StepSucceeded, StockReserved, StockReady>())
                    .AddState<StockReady>()))
            .AddState<PreparingShipment>(state => state
                .AddDoActivity<PrepareShipmentActivity>()
                .AddTransition<StepSucceeded, ShipmentPrepared, CapturingPayment>())
            .AddState<CapturingPayment>(state => state
                .AddDoAction<CapturePaymentAction>()
                .AddTransition<StepSucceeded, PaymentCaptured, DispatchingShipment>())
            .AddState<DispatchingShipment>(state => state
                .AddDoAction<DispatchShipmentAction>()
                .AddTransition<StepSucceeded, ShipmentDispatched, InTransit>()))
        .AddState<InTransit>(state => state
            .AddDoAction<DeliverShipmentAction>()
            .AddTransition<StepSucceeded, ShipmentDelivered, Invoicing>(transition => transition
                .AddEffectActivity<IssueInvoiceActivity>())
            .AddTransition<StepFailed, AppendReason, NeedsAttention>())
        .AddState<Invoicing>(state => state
            .AddTransition<InvoiceReady, Completed>(transition => transition
                .AddEffectAction<RecordCompletionAction>()))
        .AddState<NeedsAttention>(state => state
            .AddTransition<RetryDelivery, InTransit>())
        .AddState<Compensating>(state => state
            .AddDoAction<CompensateOrderAction>()
            .AddTransition<CompensationFinished, CompensationOutcome>()
            .AddTransition<CompensationFailed, AppendReason, CompensationNeedsAttention>())
        .AddState<CompensationNeedsAttention>(state => state
            .AddTransition<RetryCompensation, Compensating>())
        .AddChoice(State<CompensationOutcome>.Name, choice => choice
            .AddTransition<CancellationRequested, Cancelled>()
            .AddElseTransition<Failed>())
        .AddState<Completed>()
        .AddState<Cancelled>()
        .AddState<Failed>();
}
