using Stateflows.StateMachines.Interfaces;

namespace Stateflows.StateMachines
{
    public static class StateMachineInitialBuilderEventsSyncExtensions
    {
        public static IStateMachineInitialBuilder AddOnInitialize(this IStateMachineInitialBuilder builder, StateMachineActionDelegate stateMachineAction)
            => builder.AddOnInitialize(stateMachineAction.ToAsync());
    }
}
