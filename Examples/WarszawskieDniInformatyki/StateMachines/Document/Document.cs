using Stateflows.StateMachines;
using Stateflows.Transport.REST;
using WarszawskieDniInformatyki.StateMachines.Document.Effects;
using WarszawskieDniInformatyki.StateMachines.Document.Events;
using WarszawskieDniInformatyki.StateMachines.Document.Guards;
using WarszawskieDniInformatyki.StateMachines.Document.States;

namespace WarszawskieDniInformatyki.StateMachines.Document;

public class Document : IStateMachine
{
    public void Build(IStateMachineBuilder builder)
        => builder
            .AddInitialState<New>(b => b
                .AddTransition<Review, Reviewed>(b => b
                    .AddEffect<ReviewEffect>()
                )
            )
            .AddState<Reviewed>(b => b
                .AddTransition<Accept, Accepted>() 
                .AddTransition<Reject, Rejected>()
            )
            .AddState<Accepted>(b => b
                .AddTransition<Pay, PayGuard, Paid>() 
                .AddTransition<Reject, Rejected>()
            )
            .AddState<Paid>() 
            .AddState<Rejected>()
        ;
}