using Stateflows.StateMachines.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines
{
    public static class StateBuilderEventsSyncExtensions
    {
        public static IStateBuilder AddOnEntry(this IStateBuilder builder, StateActionDelegate stateAction)
            => builder.AddOnEntry(stateAction.ToAsync());

        public static IStateBuilder AddOnExit(this IStateBuilder builder, StateActionDelegate stateAction)
            => builder.AddOnExit(stateAction.ToAsync());
    }
}
