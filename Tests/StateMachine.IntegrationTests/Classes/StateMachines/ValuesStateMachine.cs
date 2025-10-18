using StateMachine.IntegrationTests.Classes.Events;
using StateMachine.IntegrationTests.Classes.States;
using StateMachine.IntegrationTests.Classes.Transitions;

namespace StateMachine.IntegrationTests.Classes.StateMachines
{
    public class ValuesStateMachine : IStateMachine
    {
        public static void Build(IStateMachineBuilder builder)
            => builder
                .AddInitialState<ValueState1>(b => b
                    .AddOnEntry(Actions.State.Value("nullable").Update(x => x + 3, 0))
                    .AddOnEntry(Actions.Global.Namespace("set").Value("set").Set(42))
                    .AddDefaultTransition<FinalState>(b => b
                        .AddGuard(Guards.Deny)
                    )
                    .AddDefaultTransition<ValueState2>(b => b
                        .AddGuard(
                            Guards.Allow,
                            Guards.Source.Namespace("counter").Value("x").IsNotSet,
                            Guards.Source.Value("counter").IsEqualTo(1),
                            Guards.Source.Value("nullable").IsEqualTo(3),
                            Guards.Source.Value("nulled").IsEqualTo<int?>(null),
                            Guards.Global.Namespace("set").Value("set").IsSet
                        )
                        .AddGuard<ValueTransition>()
                        .AddGuard<InState<ValueState1>>()
                        .AddGuard(Guards.InState<ValueState1>)
                    )
                )
                .AddState<ValueState2>(b => b
                    .AddOnEntry(Actions.Global.Namespace("set").Clear)
                    .AddInternalTransition<SomeEvent>(b => b
                        .AddEffect<InternalTransition>()
                    )
                    .AddDefaultTransition<GuardedTransition, FinalState>(b => b
                        // .AddGuard(Guards.Global.Value("counter").IsEqualTo(1))
                        // .AddGuard<GuardedTransition>()
                        .AddGuard(Guards.Global.Namespace("set").Value("set").IsNotSet)
                        // .AddGuard(Guards.Global.Namespace("x").Namespace("y").Value("z").IsEqualTo(42))
                    )
                )
                .AddFinalState()
            ;
    }
}
