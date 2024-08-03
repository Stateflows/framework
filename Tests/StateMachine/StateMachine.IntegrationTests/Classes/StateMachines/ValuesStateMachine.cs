using Stateflows.StateMachines.Typed;
using StateMachine.IntegrationTests.Classes.States;
using StateMachine.IntegrationTests.Classes.Transitions;

namespace StateMachine.IntegrationTests.Classes.StateMachines
{
    public class ValuesStateMachine : IStateMachine
    {
        public void Build(IStateMachineBuilder builder)
            => builder
                .AddInitialState<ValueState1>(b => b
                    .AddDefaultTransition<ValueTransition, ValueState2>()
                )
                .AddState<ValueState2>(b => b
                    .AddInternalTransition<SomeEvent, InternalTransition>()
                    .AddDefaultTransition<GuardedTransition, FinalState>()
                )
                .AddFinalState()
            ;
    }
}
