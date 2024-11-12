using Stateflows.StateMachines;
using StateMachine.IntegrationTests.Classes.States;
using StateMachine.IntegrationTests.Classes.Transitions;

namespace StateMachine.IntegrationTests.Classes.StateMachines
{
    public class ValuesStateMachine : IStateMachine
    {
        public void Build(IStateMachineBuilder builder)
            => builder
                .AddInitialState<ValueState1>(b => b
                    .AddDefaultTransition<ValueState2>(b => b
                        .AddGuard<ValueTransition>()
                    )
                )
                .AddState<ValueState2>(b => b
                    .AddInternalTransition<SomeEvent>(b => b
                        .AddEffect<InternalTransition>()
                    )
                    .AddDefaultTransition<FinalState>(b => b
                        .AddGuard<GuardedTransition>()
                    )
                )
                .AddFinalState()
            ;
    }
}
