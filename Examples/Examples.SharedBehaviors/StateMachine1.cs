﻿using Examples.SharedBehaviors.Events;
using Stateflows.Common;
using Stateflows.StateMachines;
using Stateflows.StateMachines.Attributes;
using System.Diagnostics;

namespace Examples.SharedBehaviors
{
    [StateMachine(nameof(StateMachine1))]
    public class StateMachine1 : StateMachine
    {
        public override void Build(ITypedStateMachineBuilder builder)
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
