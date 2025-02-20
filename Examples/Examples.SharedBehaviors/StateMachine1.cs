using Examples.SharedBehaviors.Events;
using Stateflows.Common;
using Stateflows.StateMachines;
using Stateflows.StateMachines.Attributes;

namespace Examples.SharedBehaviors
{
    [StateMachineBehavior(nameof(StateMachine1))]
    public class StateMachine1 : IStateMachine
    {
        public void Build(IStateMachineBuilder builder)
        {
            builder
                .AddInitialState("state1", b => b
                    .AddTransition<Event1>("state2")
                )
                .AddState("state2", b => b
                    .AddTransition<Event2>("state3")
                    .AddInternalTransition<EveryOneMinute>(b => b
                        .AddEffect(c => Console.WriteLine($"    {DateTime.Now.Hour}:{DateTime.Now.Minute} instance {c.StateMachine.Id.Instance} says: tick!"))
                    )
                )
                .AddState("state3", b => b
                    .AddTransition<Event1>("state1")
                )
                ;
        }
    }
}
