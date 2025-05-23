using Stateflows.Common;
using Stateflows.StateMachines.Attributes;
using StateMachine.IntegrationTests.Classes.Events;
using StateMachine.IntegrationTests.Classes.States;
using StateMachine.IntegrationTests.Classes.Transitions;

namespace StateMachine.IntegrationTests.Classes.StateMachines
{
    internal class Initializer1 : IDefaultInitializer
    {
        public Task<bool> OnInitializeAsync()
        {
            StateMachine1.InitializeFired = true;

            return Task.FromResult(true);
        }
    }
    internal class Finalizer1 : IFinalizer
    {
        public Task OnFinalizeAsync()
        {
            StateMachine1.FinalizeFired = true;

            return Task.CompletedTask;
        }
    }

    [StateMachineBehavior]
    internal class StateMachine1 : IStateMachine
    {
        public static bool InitializeFired = false;

        public static bool FinalizeFired = false;

        public static void Reset()
        {
            InitializeFired = false;
            FinalizeFired = false;
        }

        public void Build(IStateMachineBuilder builder)
        {
            builder
                .AddDefaultInitializer<Initializer1>()
                .AddFinalizer<Finalizer1>()
                .AddInitialState<State1>(b => b
                    .AddTransition<SomeEvent, State2>(b => b
                        .AddGuard<SomeEventTransition>()
                        .AddEffect<SomeEventTransition>()
                    )
                )
                .AddState<State2>(b => b
                    .AddTransition<SomeEvent, FinalState>(b => b
                        .AddGuard<SomeEventTransition>()
                        .AddEffect<SomeEventTransition>()
                    )
                )
                .AddState<FinalState>();
        }
    }
}
