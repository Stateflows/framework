using Stateflows.StateMachines;
using Stateflows.StateMachines.Attributes;
using StateMachine.IntegrationTests.Classes.States;
using StateMachine.IntegrationTests.Classes.Transitions;
using Stateflows.StateMachines.Extensions;
using Stateflows.Common;

namespace StateMachine.IntegrationTests.Classes.StateMachines
{
    internal class EverythingInitializer : IDefaultInitializer
    {
        public Task<bool> OnInitializeAsync()
        {
            return Task.FromResult(true);
        }
    }
    internal class SomeInitializer : IInitializer<SomeEvent>
    {
        public Task<bool> OnInitializeAsync(SomeEvent initializationEvent)
        {
            return Task.FromResult(true);
        }
    }

    internal class EverythingFinalizer : IFinalizer
    {
        public Task OnFinalizeAsync()
        {
            return Task.CompletedTask;
        }
    }

    [StateMachineBehavior]
    internal class EverythingStateMachine : IStateMachine
    {
        public void Build(IStateMachineBuilder builder)
        {
            builder
                .AddInitializer<SomeEvent, SomeInitializer>()
                .AddInitializer<SomeEvent>(async c => true)
                .AddInitializer<SomeEvent>(c => Task.FromResult(true))

                .AddDefaultInitializer<EverythingInitializer>()
                .AddDefaultInitializer(async c => true)
                .AddDefaultInitializer(c => Task.FromResult(true))

                .AddFinalizer<EverythingFinalizer>()
                .AddFinalizer(async c => { })
                .AddFinalizer(c => Task.CompletedTask)

                .AddInitialState<State1>(b => b
                    .AddTransition<SomeEvent, State2>(b => b
                        .AddGuard<SomeEventTransition>()
                        .AddEffect<SomeEventTransition>()
                    )
                    .AddElseTransition<SomeEvent, State2>(b => b
                        .AddEffect<SomeEventTransition>()
                    )

                    .AddInternalTransition<SomeEvent>(b => b
                        .AddGuard<SomeEventTransition>()
                        .AddEffect<SomeEventTransition>()
                    )
                    .AddElseInternalTransition<SomeEvent>(b => b
                        .AddEffect<SomeEventTransition>()
                    )

                    .AddDefaultTransition<State2>(b => b
                        .AddGuard<GuardedTransition>()
                    )
                    .AddElseDefaultTransition<State2>(b => b
                        .AddEffect<EffectedTransition>()
                    )

                    .AddTransition<SomeEvent>(State<State2>.Name, b => b
                        .AddGuard<SomeEventTransition>()
                        .AddEffect<SomeEventTransition>()
                    )
                    .AddElseTransition<SomeEvent>(State<State2>.Name, b => b
                        .AddEffect<SomeEventTransition>()
                    )

                    .AddDefaultTransition(State<State2>.Name, b => b
                        .AddGuard<GuardedTransition>()
                    )
                    .AddElseDefaultTransition(State<State2>.Name, b => b
                        .AddEffect<EffectedTransition>()
                    )
                )

                .AddState("state1", b => b
                    .AddOnEntry(c => { })
                    .AddOnEntry(async c => { })

                    .AddOnExit(c => { })
                    .AddOnExit(async c => { })

                    .AddTransition<SomeEvent, State2>(b => b
                        .AddGuard<SomeEventTransition>()
                        .AddEffect<SomeEventTransition>()
                    )
                    .AddElseTransition<SomeEvent, State2>(b => b
                        .AddEffect<SomeEventTransition>()
                    )

                    .AddInternalTransition<SomeEvent>(b => b
                        .AddGuard<SomeEventTransition>()
                        .AddEffect<SomeEventTransition>()
                    )
                    .AddElseInternalTransition<SomeEvent>(b => b
                        .AddEffect<SomeEventTransition>()
                    )

                    .AddDefaultTransition<State2>(b => b
                        .AddGuard<GuardedTransition>()
                    )
                    .AddElseDefaultTransition<State2>(b => b
                        .AddEffect<EffectedTransition>()
                    )

                    .AddTransition<SomeEvent>(State<State2>.Name, b => b
                        .AddGuard<SomeEventTransition>()
                        .AddEffect<SomeEventTransition>()
                    )
                    .AddElseTransition<SomeEvent>(State<State2>.Name, b => b
                        .AddEffect<SomeEventTransition>()
                    )

                    .AddDefaultTransition(State<State2>.Name, b => b
                        .AddGuard<GuardedTransition>()
                    )
                    .AddElseDefaultTransition(State<State2>.Name, b => b
                        .AddEffect<EffectedTransition>()
                    )

                    .AddTransition<SomeEvent, State2>(b => b
                        .AddGuard(c => true)
                        .AddGuard(c => Task.FromResult(true))

                        .AddEffect(c => { })
                        .AddEffect(c => Task.CompletedTask)
                    )

                    .AddElseTransition<SomeEvent, State2>(b => b
                        .AddEffect(c => { })
                        .AddEffect(c => Task.CompletedTask)
                    )

                    .AddInternalTransition<SomeEvent>(b => b
                        .AddGuard(c => true)
                        .AddGuard(c => Task.FromResult(true))

                        .AddEffect(c => { })
                        .AddEffect(c => Task.CompletedTask)
                    )

                    .AddElseInternalTransition<SomeEvent>(b => b
                        .AddEffect(c => { })
                        .AddEffect(c => Task.CompletedTask)
                    )

                    .AddDefaultTransition<State2>(b => b
                        .AddGuard(c => true)
                        .AddGuard(c => Task.FromResult(true))

                        .AddEffect(c => { })
                        .AddEffect(c => Task.CompletedTask)
                    )

                    .AddElseDefaultTransition<State2>(b => b
                        .AddEffect(c => { })
                        .AddEffect(c => Task.CompletedTask)
                    )
                )

                .AddState<State2>(b => b
                    .AddTransition<SomeEvent, FinalState>()
                    .AddElseTransition<SomeEvent, FinalState>()

                    .AddTransition<SomeEvent, State2>(b => b
                        .AddGuard<SomeEventTransition>()
                        .AddEffect<SomeEventTransition>()
                    )
                    .AddElseTransition<SomeEvent, State2>(b => b
                        .AddEffect<SomeEventTransition>()
                    )

                    .AddInternalTransition<SomeEvent>(b => b
                        .AddGuard<SomeEventTransition>()
                        .AddEffect<SomeEventTransition>()
                    )
                    .AddElseInternalTransition<SomeEvent>(b => b
                        .AddEffect<SomeEventTransition>()
                    )

                    .AddDefaultTransition<State2>(b => b
                        .AddGuard<GuardedTransition>()
                    )
                    .AddElseDefaultTransition<State2>(b => b
                        .AddEffect<EffectedTransition>()
                    )

                    .AddTransition<SomeEvent>(State<State2>.Name, b => b
                        .AddGuard<SomeEventTransition>()
                        .AddEffect<SomeEventTransition>()
                    )
                    .AddElseTransition<SomeEvent>(State<State2>.Name, b => b
                        .AddEffect<SomeEventTransition>()
                    )

                    .AddDefaultTransition(State<State2>.Name, b => b
                        .AddGuard<GuardedTransition>()
                    )
                    .AddElseDefaultTransition(State<State2>.Name, b => b
                        .AddEffect<EffectedTransition>()
                    )

                    .AddTransition<SomeEvent, State2>(b => b
                        .AddGuard(c => true)
                        .AddGuard(c => Task.FromResult(true))

                        .AddEffect(c => { })
                        .AddEffect(c => Task.CompletedTask)
                    )

                    .AddElseTransition<SomeEvent, State2>(b => b
                        .AddEffect(c => { })
                        .AddEffect(c => Task.CompletedTask)
                    )

                    .AddInternalTransition<SomeEvent>(b => b
                        .AddGuard(c => true)
                        .AddGuard(c => Task.FromResult(true))

                        .AddEffect(c => { })
                        .AddEffect(c => Task.CompletedTask)
                    )

                    .AddElseInternalTransition<SomeEvent>(b => b
                        .AddEffect(c => { })
                        .AddEffect(c => Task.CompletedTask)
                    )

                    .AddDefaultTransition<State2>(b => b
                        .AddGuard(c => true)
                        .AddGuard(c => Task.FromResult(true))

                        .AddEffect(c => { })
                        .AddEffect(c => Task.CompletedTask)
                    )

                    .AddElseDefaultTransition<State2>(b => b
                        .AddEffect(c => { })
                        .AddEffect(c => Task.CompletedTask)
                    )
                )

                .AddCompositeState<State3>(b => b
                    .AddTransition<SomeEvent, FinalState>()
                    .AddElseTransition<SomeEvent, FinalState>()

                    .AddTransition<SomeEvent, State2>(b => b
                        .AddGuard<SomeEventTransition>()
                        .AddEffect<SomeEventTransition>()
                    )
                    .AddElseTransition<SomeEvent, State2>(b => b
                        .AddEffect<SomeEventTransition>()
                    )

                    .AddInternalTransition<SomeEvent>(b => b
                        .AddGuard<SomeEventTransition>()
                        .AddEffect<SomeEventTransition>()
                    )
                    .AddElseInternalTransition<SomeEvent>(b => b
                        .AddEffect<SomeEventTransition>()
                    )

                    .AddDefaultTransition<State2>(b => b
                        .AddGuard<GuardedTransition>()
                    )
                    .AddElseDefaultTransition<State2>(b => b
                        .AddEffect<EffectedTransition>()
                    )

                    .AddTransition<SomeEvent>(State<State2>.Name, b => b
                        .AddGuard<SomeEventTransition>()
                        .AddEffect<SomeEventTransition>()
                    )
                    .AddElseTransition<SomeEvent>(State<State2>.Name, b => b
                        .AddEffect<SomeEventTransition>()
                    )

                    .AddDefaultTransition(State<State2>.Name, b => b
                        .AddGuard<GuardedTransition>()
                    )
                    .AddElseDefaultTransition(State<State2>.Name, b => b
                        .AddEffect<EffectedTransition>()
                    )

                    .AddTransition<SomeEvent, State2>(b => b
                        .AddGuard(c => true)
                        .AddGuard(c => Task.FromResult(true))

                        .AddEffect(c => { })
                        .AddEffect(c => Task.CompletedTask)
                    )

                    .AddElseTransition<SomeEvent, State2>(b => b
                        .AddEffect(c => { })
                        .AddEffect(c => Task.CompletedTask)
                    )

                    .AddInternalTransition<SomeEvent>(b => b
                        .AddGuard(c => true)
                        .AddGuard(c => Task.FromResult(true))

                        .AddEffect(c => { })
                        .AddEffect(c => Task.CompletedTask)
                    )

                    .AddElseInternalTransition<SomeEvent>(b => b
                        .AddEffect(c => { })
                        .AddEffect(c => Task.CompletedTask)
                    )

                    .AddDefaultTransition<State2>(b => b
                        .AddGuard(c => true)
                        .AddGuard(c => Task.FromResult(true))

                        .AddEffect(c => { })
                        .AddEffect(c => Task.CompletedTask)
                    )

                    .AddElseDefaultTransition<State2>(b => b
                        .AddEffect(c => { })
                        .AddEffect(c => Task.CompletedTask)
                    )

                    .AddInitialState<State2>(b => b
                        .AddTransition<SomeEvent, FinalState>()
                        .AddElseTransition<SomeEvent, FinalState>()

                        //.AddInternalTransition<SomeEvent, SomeEventTransition>()
                        //.AddElseInternalTransition<SomeEvent, SomeEventTransition>()

                        //.AddDefaultTransition<GuardedTransition, FinalState>()
                        //.AddElseDefaultTransition<EffectedTransition, FinalState>()

                        //.AddTransition<SomeEvent, SomeEventTransition>(State<FinalState>.Name)
                        //.AddElseTransition<SomeEvent, SomeEventTransition>(State<FinalState>.Name)

                        //.AddDefaultTransition<GuardedTransition>(State<FinalState>.Name)
                        //.AddElseDefaultTransition<EffectedTransition>(State<FinalState>.Name)
                    )

                    .AddTransition<SomeEvent, FinalState>()
                    .AddElseTransition<SomeEvent, FinalState>()

                    //.AddInternalTransition<SomeEvent, SomeEventTransition>()
                    //.AddElseInternalTransition<SomeEvent, SomeEventTransition>()

                    //.AddDefaultTransition<GuardedTransition, FinalState>()
                    //.AddElseDefaultTransition<EffectedTransition, FinalState>()

                    //.AddTransition<SomeEvent, SomeEventTransition>(State<FinalState>.Name)
                    //.AddElseTransition<SomeEvent, SomeEventTransition>(State<FinalState>.Name)

                    //.AddDefaultTransition<GuardedTransition>(State<FinalState>.Name)
                    //.AddElseDefaultTransition<EffectedTransition>(State<FinalState>.Name)

                    .AddTransition<SomeEvent, State2>(b => b
                        .AddGuard(c => true)
                        .AddGuard(c => Task.FromResult(true))

                        .AddEffect(c => { })
                        .AddEffect(c => Task.CompletedTask)
                    )

                    .AddElseTransition<SomeEvent, State2>(b => b
                        .AddEffect(c => { })
                        .AddEffect(c => Task.CompletedTask)
                    )

                    .AddInternalTransition<SomeEvent>(b => b
                        .AddGuard(c => true)
                        .AddGuard(c => Task.FromResult(true))

                        .AddEffect(c => { })
                        .AddEffect(c => Task.CompletedTask)
                    )

                    .AddElseInternalTransition<SomeEvent>(b => b
                        .AddEffect(c => { })
                        .AddEffect(c => Task.CompletedTask)
                    )

                    .AddDefaultTransition<State2>(b => b
                        .AddGuard(c => true)
                        .AddGuard(c => Task.FromResult(true))

                        .AddEffect(c => { })
                        .AddEffect(c => Task.CompletedTask)
                    )

                    .AddElseDefaultTransition<State2>(b => b
                        .AddEffect(c => { })
                        .AddEffect(c => Task.CompletedTask)
                    )

                    .AddFinalState()

                    .AddTransition<SomeEvent, FinalState>()
                    .AddElseTransition<SomeEvent, FinalState>()

                    //.AddInternalTransition<SomeEvent, SomeEventTransition>()
                    //.AddElseInternalTransition<SomeEvent, SomeEventTransition>()

                    //.AddDefaultTransition<GuardedTransition, FinalState>()
                    //.AddElseDefaultTransition<EffectedTransition, FinalState>()

                    //.AddTransition<SomeEvent, SomeEventTransition>(State<FinalState>.Name)
                    //.AddElseTransition<SomeEvent, SomeEventTransition>(State<FinalState>.Name)

                    //.AddDefaultTransition<GuardedTransition>(State<FinalState>.Name)
                    //.AddElseDefaultTransition<EffectedTransition>(State<FinalState>.Name)

                    .AddTransition<SomeEvent, State2>(b => b
                        .AddGuard(c => true)
                        .AddGuard(c => Task.FromResult(true))

                        .AddEffect(c => { })
                        .AddEffect(c => Task.CompletedTask)
                    )

                    .AddElseTransition<SomeEvent, State2>(b => b
                        .AddEffect(c => { })
                        .AddEffect(c => Task.CompletedTask)
                    )

                    .AddInternalTransition<SomeEvent>(b => b
                        .AddGuard(c => true)
                        .AddGuard(c => Task.FromResult(true))

                        .AddEffect(c => { })
                        .AddEffect(c => Task.CompletedTask)
                    )

                    .AddElseInternalTransition<SomeEvent>(b => b
                        .AddEffect(c => { })
                        .AddEffect(c => Task.CompletedTask)
                    )

                    .AddDefaultTransition<State2>(b => b
                        .AddGuard(c => true)
                        .AddGuard(c => Task.FromResult(true))

                        .AddEffect(c => { })
                        .AddEffect(c => Task.CompletedTask)
                    )

                    .AddElseDefaultTransition<State2>(b => b
                        .AddEffect(c => { })
                        .AddEffect(c => Task.CompletedTask)
                    )
                )

                .AddCompositeState("composite", b => b
                    .AddOnEntry(c => { })
                    .AddOnEntry(async c => { })

                    .AddOnExit(c => { })
                    .AddOnExit(async c => { })

                    .AddOnInitialize(c => { })
                    .AddOnInitialize(async c => { })

                    .AddOnFinalize(c => { })
                    .AddOnFinalize(async c => { })

                    .AddTransition<SomeEvent, FinalState>()
                    .AddElseTransition<SomeEvent, FinalState>()

                    //.AddInternalTransition<SomeEvent, SomeEventTransition>()
                    //.AddElseInternalTransition<SomeEvent, SomeEventTransition>()

                    //.AddDefaultTransition<GuardedTransition, FinalState>()
                    //.AddElseDefaultTransition<EffectedTransition, FinalState>()

                    //.AddTransition<SomeEvent, SomeEventTransition>(State<FinalState>.Name)
                    //.AddElseTransition<SomeEvent, SomeEventTransition>(State<FinalState>.Name)

                    //.AddDefaultTransition<GuardedTransition>(State<FinalState>.Name)
                    //.AddElseDefaultTransition<EffectedTransition>(State<FinalState>.Name)

                    .AddTransition<SomeEvent, State2>(b => b
                        .AddGuard(c => true)
                        .AddGuard(c => Task.FromResult(true))

                        .AddEffect(c => { })
                        .AddEffect(c => Task.CompletedTask)
                    )

                    .AddElseTransition<SomeEvent, State2>(b => b
                        .AddEffect(c => { })
                        .AddEffect(c => Task.CompletedTask)
                    )

                    .AddInternalTransition<SomeEvent>(b => b
                        .AddGuard(c => true)
                        .AddGuard(c => Task.FromResult(true))

                        .AddEffect(c => { })
                        .AddEffect(c => Task.CompletedTask)
                    )

                    .AddElseInternalTransition<SomeEvent>(b => b
                        .AddEffect(c => { })
                        .AddEffect(c => Task.CompletedTask)
                    )

                    .AddDefaultTransition<State2>(b => b
                        .AddGuard(c => true)
                        .AddGuard(c => Task.FromResult(true))

                        .AddEffect(c => { })
                        .AddEffect(c => Task.CompletedTask)
                    )

                    .AddElseDefaultTransition<State2>(b => b
                        .AddEffect(c => { })
                        .AddEffect(c => Task.CompletedTask)
                    )

                    .AddInitialState<State2>(b => b
                        .AddTransition<SomeEvent, FinalState>()
                        .AddElseTransition<SomeEvent, FinalState>()

                        //.AddInternalTransition<SomeEvent, SomeEventTransition>()
                        //.AddElseInternalTransition<SomeEvent, SomeEventTransition>()

                        //.AddDefaultTransition<GuardedTransition, FinalState>()
                        //.AddElseDefaultTransition<EffectedTransition, FinalState>()

                        //.AddTransition<SomeEvent, SomeEventTransition>(State<FinalState>.Name)
                        //.AddElseTransition<SomeEvent, SomeEventTransition>(State<FinalState>.Name)

                        //.AddDefaultTransition<GuardedTransition>(State<FinalState>.Name)
                        //.AddElseDefaultTransition<EffectedTransition>(State<FinalState>.Name)
                    )

                    .AddTransition<SomeEvent, FinalState>()
                    .AddElseTransition<SomeEvent, FinalState>()

                    //.AddInternalTransition<SomeEvent, SomeEventTransition>()
                    //.AddElseInternalTransition<SomeEvent, SomeEventTransition>()

                    //.AddDefaultTransition<GuardedTransition, FinalState>()
                    //.AddElseDefaultTransition<EffectedTransition, FinalState>()

                    //.AddTransition<SomeEvent, SomeEventTransition>(State<FinalState>.Name)
                    //.AddElseTransition<SomeEvent, SomeEventTransition>(State<FinalState>.Name)

                    //.AddDefaultTransition<GuardedTransition>(State<FinalState>.Name)
                    //.AddElseDefaultTransition<EffectedTransition>(State<FinalState>.Name)

                    .AddTransition<SomeEvent, State2>(b => b
                        .AddGuard(c => true)
                        .AddGuard(c => Task.FromResult(true))

                        .AddEffect(c => { })
                        .AddEffect(c => Task.CompletedTask)
                    )

                    .AddElseTransition<SomeEvent, State2>(b => b
                        .AddEffect(c => { })
                        .AddEffect(c => Task.CompletedTask)
                    )

                    .AddInternalTransition<SomeEvent>(b => b
                        .AddGuard(c => true)
                        .AddGuard(c => Task.FromResult(true))

                        .AddEffect(c => { })
                        .AddEffect(c => Task.CompletedTask)
                    )

                    .AddElseInternalTransition<SomeEvent>(b => b
                        .AddEffect(c => { })
                        .AddEffect(c => Task.CompletedTask)
                    )

                    .AddDefaultTransition<State2>(b => b
                        .AddGuard(c => true)
                        .AddGuard(c => Task.FromResult(true))

                        .AddEffect(c => { })
                        .AddEffect(c => Task.CompletedTask)
                    )

                    .AddElseDefaultTransition<State2>(b => b
                        .AddEffect(c => { })
                        .AddEffect(c => Task.CompletedTask)
                    )

                    .AddFinalState()

                    .AddTransition<SomeEvent, FinalState>()
                    .AddElseTransition<SomeEvent, FinalState>()

                    //.AddInternalTransition<SomeEvent, SomeEventTransition>()
                    //.AddElseInternalTransition<SomeEvent, SomeEventTransition>()

                    //.AddDefaultTransition<GuardedTransition, FinalState>()
                    //.AddElseDefaultTransition<EffectedTransition, FinalState>()

                    //.AddTransition<SomeEvent, SomeEventTransition>(State<FinalState>.Name)
                    //.AddElseTransition<SomeEvent, SomeEventTransition>(State<FinalState>.Name)

                    //.AddDefaultTransition<GuardedTransition>(State<FinalState>.Name)
                    //.AddElseDefaultTransition<EffectedTransition>(State<FinalState>.Name)

                    .AddTransition<SomeEvent, State2>(b => b
                        .AddGuard(c => true)
                        .AddGuard(c => Task.FromResult(true))

                        .AddEffect(c => { })
                        .AddEffect(c => Task.CompletedTask)
                    )

                    .AddElseTransition<SomeEvent, State2>(b => b
                        .AddEffect(c => { })
                        .AddEffect(c => Task.CompletedTask)
                    )

                    .AddInternalTransition<SomeEvent>(b => b
                        .AddGuard(c => true)
                        .AddGuard(c => Task.FromResult(true))

                        .AddEffect(c => { })
                        .AddEffect(c => Task.CompletedTask)
                    )

                    .AddElseInternalTransition<SomeEvent>(b => b
                        .AddEffect(c => { })
                        .AddEffect(c => Task.CompletedTask)
                    )

                    .AddDefaultTransition<State2>(b => b
                        .AddGuard(c => true)
                        .AddGuard(c => Task.FromResult(true))

                        .AddEffect(c => { })
                        .AddEffect(c => Task.CompletedTask)
                    )

                    .AddElseDefaultTransition<State2>(b => b
                        .AddEffect(c => { })
                        .AddEffect(c => Task.CompletedTask)
                    )
                )
                .AddState<FinalState>();
        }
    }
}
