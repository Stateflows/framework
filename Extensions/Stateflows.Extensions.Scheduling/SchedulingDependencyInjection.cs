using System.Diagnostics;
using Stateflows.Common.Registration.Interfaces;
using Stateflows.Events;
using Stateflows.Scheduler.StateMachine;
using Stateflows.StateMachines;

namespace Stateflows
{
    /// <summary>Registers the built-in event scheduling behavior.</summary>
    public static class SchedulingDependencyInjection
    {
        /// <summary>Adds cron validation, trigger-type mapping, and the scheduler state machine.</summary>
        /// <param name="stateflowsBuilder">The Stateflows builder to configure.</param>
        /// <returns>The same builder, for further Stateflows configuration.</returns>
        [DebuggerHidden]
        public static IStateflowsBuilder AddScheduling(this IStateflowsBuilder stateflowsBuilder)
        {
            stateflowsBuilder
                .AddValidator<CronEventValidator>()
                .AddOneOf()
                .AddStateMachines(b => b
                    .AddStateMachine<StateflowsScheduler>()
                );

            return stateflowsBuilder;
        }
    }
}
