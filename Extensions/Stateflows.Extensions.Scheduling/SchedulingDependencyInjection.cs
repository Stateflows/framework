using System.Diagnostics;
using Stateflows.Common.Registration.Interfaces;
using Stateflows.Events;
using Stateflows.Scheduler.StateMachine;
using Stateflows.StateMachines;

namespace Stateflows
{
    public static class SchedulingDependencyInjection
    {
        [DebuggerHidden]
        public static IStateflowsBuilder AddScheduling(this IStateflowsBuilder stateflowsBuilder)
        {
            stateflowsBuilder
                .AddValidator<CronEventValidator>()
                .AddStateMachines(b => b
                    .AddStateMachine<StateflowsScheduler>()
                );

            return stateflowsBuilder;
        }
    }
}
