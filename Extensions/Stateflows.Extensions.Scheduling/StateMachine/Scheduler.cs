using OneOf;
using Stateflows.Common;
using Stateflows.Scheduler.StateMachine.Effects;
using Stateflows.Scheduler.StateMachine.Events;
using Stateflows.Scheduler.StateMachine.Guards;
using Stateflows.Scheduler.StateMachine.States;
using Stateflows.StateMachines;

namespace Stateflows.Scheduler.StateMachine
{
    public class StateflowsScheduler : IStateMachine
    {
        public static void Build(IStateMachineBuilder builder) => builder
            .AddInitialCompositeState("main", b => b
                .AddInternalTransition<OneOf<ScheduleCron, ScheduleDelay, ScheduleInterval, ScheduleTime>, ScheduleEffect>()
                .AddInternalTransition<Unschedule, UnscheduleEffect>()

                .AddInitialState<Idle>(b => b
                    .AddDefaultTransition<Running>(b => b
                        .AddGuard<AnySchedules>()
                    )
                )
                .AddState<Running>(b => b
                    .AddDefaultTransition<Idle>(b => b
                        .AddNegatedGuard<AnySchedules>()
                    )
                    .AddInternalTransition<EveryOneMinute, PublishEffect>()
                )
            )
        ;
    }
}