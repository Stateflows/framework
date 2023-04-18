using Stateflows.StateMachines.Interfaces;

namespace Stateflows.StateMachines
{
    public static class StateMachineBuilderEventsSyncExtensions
    {
        public static IStateMachineBuilder AddOnInitialize(this IStateMachineBuilder builder, StateMachineActionDelegate stateMachineAction)
            => builder.AddOnInitialize(stateMachineAction.ToAsync());
    }
}
