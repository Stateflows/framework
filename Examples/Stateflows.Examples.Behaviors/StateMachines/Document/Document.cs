using Stateflows.Common;
using Stateflows.Examples.Behaviors.StateMachines.Document.Effects;
using Stateflows.Examples.Behaviors.StateMachines.Document.Guards;
using Stateflows.Examples.Behaviors.StateMachines.Document.States;
using Stateflows.Examples.Common.Events;
using Stateflows.Extensions.MinimalAPIs;
using Stateflows.StateMachines;

namespace Stateflows.Examples.Behaviors.StateMachines.Document;

public class Document : IStateMachine
{
    public void Build(IStateMachineBuilder builder)
        => builder
            .AddInitialState<New>(b => b
                .AddTransition<Review, Reviewed>(b => b
                    .AddEffect<ReviewEffect>()
                )
                .AddTransition<AfterFiveMinutes, Rejected>()
            )
            .AddState<Reviewed>(b => b
                .AddTransition<Accept, Accepted>() 
                .AddTransition<Reject, Rejected>()
            )
            .AddState<Accepted>(b => b
                .AddTransition<Pay, PayGuard, Paid>() 
                .AddTransition<Reject, Rejected>()
            )
            .AddState<Paid>(b => b
                .AddEndpoints(b => b
                    .AddGet("/invoices", () => { })
                )
            )
            .AddState<Rejected>()
        ;
}