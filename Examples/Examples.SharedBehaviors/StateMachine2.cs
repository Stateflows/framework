using Stateflows.Common;
using Stateflows.StateMachines;
using Stateflows.StateMachines.Attributes;

namespace Examples.SharedBehaviors
{
    [StateMachineBehavior(nameof(StateMachine2))]
    public class StateMachine2 : StateMachine
    {
        public override void Build(ITypedStateMachineBuilder builder)
        {
            builder
                .AddInitialState("state1", b => b
                    .AddInternalTransition<EveryOneMinute>(b => b
                        .AddEffect(c => Console.WriteLine($"    {DateTime.Now.Hour}:{DateTime.Now.Minute} default instance says: tick!"))
                    )
                )
                ;
        }
    }
}
