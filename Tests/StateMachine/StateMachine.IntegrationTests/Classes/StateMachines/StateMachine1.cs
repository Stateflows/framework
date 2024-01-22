using Stateflows.StateMachines.Typed;
using Stateflows.StateMachines.Attributes;
using StateMachine.IntegrationTests.Classes.States;
using StateMachine.IntegrationTests.Classes.Transitions;

namespace StateMachine.IntegrationTests.Classes.StateMachines
{
    [StateMachineBehavior]
    internal class StateMachine1 : Stateflows.StateMachines.StateMachine
    {
        public static bool InitializeFired = false;

        public static bool FinalizeFired = false;

        public static void Reset()
        {
            InitializeFired = false;
            FinalizeFired = false;
        }

        public override Task<bool> OnInitializeAsync()
        {
            InitializeFired = true;
            return Task.FromResult(true);
        }

        public override Task OnFinalizeAsync()
        {
            FinalizeFired = true;
            return Task.CompletedTask;
        }

        public override void Build(ITypedStateMachineBuilder builder)
        {
            builder
                .AddInitialState<State1>(b => b
                    .AddTransition<SomeEvent, SomeEventTransition, State2>()
                )
                .AddState<State2>(b => b
                    .AddTransition<SomeEvent, SomeEventTransition, FinalState>()
                )
                .AddState<FinalState>();
        }
    }
}
