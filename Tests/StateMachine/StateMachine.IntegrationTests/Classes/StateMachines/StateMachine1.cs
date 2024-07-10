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

        public override void Build(IStateMachineBuilder builder)
        {
            builder
                .AddDefaultInitializer(c =>
                {
                    InitializeFired = true;
                    return true;
                })
                .AddFinalizer(c => FinalizeFired = true)

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
