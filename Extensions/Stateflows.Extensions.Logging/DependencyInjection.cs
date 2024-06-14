using Stateflows.Activities;
using Stateflows.Common.Registration.Interfaces;
using Stateflows.Extensions.Logging;
using Stateflows.StateMachines;

namespace Stateflows
{
    public static class DependencyInjection
    {
        public static IStateflowsBuilder AddTraceLogging(this IStateflowsBuilder stateflowsBuilder)
        {
            return stateflowsBuilder
                .AddStateMachines(b => b.AddObserver<StateMachineTracer>())
                .AddActivities(b => b.AddObserver<ActivityTracer>());
        }
    }
}
