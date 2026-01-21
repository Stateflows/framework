using Stateflows.Actions;
using Stateflows.Common;
using Stateflows.Examples.Behaviors.Activities.Invoicing;
using Stateflows.Examples.Behaviors.StateMachines.Document.Effects;
using Stateflows.Examples.Behaviors.StateMachines.Document.Guards;
using Stateflows.Examples.Behaviors.StateMachines.Document.Interceptors;
using Stateflows.Examples.Behaviors.StateMachines.Document.States;
using Stateflows.Examples.Common.Events;
using Stateflows.StateMachines;
using Stateflows.Activities;
using Stateflows.StateMachines.Attributes;

namespace Stateflows.Examples.Behaviors.StateMachines.Document;

[StateMachineBehavior]
public class Document : IStateMachine
{
    public static void Build(IStateMachineBuilder builder) => builder
        .AddInterceptor<HttpContextInterceptor>()
        .AddInitialState<New>(b => b
            .AddTransition<Review, ApprovalPending>(b => b
                .AddEffect<ReviewEffect>()
            )
            .AddTransition<AfterOneMinute, ReportAutorejection, Rejected>()
        )
        .AddState<ApprovalPending>(b => b
            .AddTransition<Approve, Approved>()
            .AddTransition<Reject, ReportRejection, Rejected>()
        )
        .AddCompositeState<Approved>(b => b
            .AddInitialState<GeneratingInvoice>(b => b
                .AddDoActivity<Invoicing>(b => b
                    .AddFinalizedNotificationPolicy()
                )
                .AddTransition<DoActivityFinalized, InvoiceGenerated>()
            )
            .AddState<InvoiceGenerated>(b => b
                .AddTransition<PaymentBooked, VerifyPayment, Paid>()
            )
        )
        .AddState<Paid>(b => b
            .AddInternalTransition<Reject>(b => b
                .AddEffectAction(
                    async c =>
                    {
                        await Task.Delay(5000);
                        await c.Behavior.Values.UpdateAsync("counter", c => c + 1, 0);
                    },
                    b => b
                        .AddCompletionNotificationPolicy()
                        .SetResourceName("heavy-work")
                )
            )
            .AddDefaultTransition<Rejected>(b => b
                .AddGuard(async c => await c.Behavior.Values.GetOrDefaultAsync<int>("counter") >= 5)
            )
        )
        .AddState<Rejected>()
    ;
}