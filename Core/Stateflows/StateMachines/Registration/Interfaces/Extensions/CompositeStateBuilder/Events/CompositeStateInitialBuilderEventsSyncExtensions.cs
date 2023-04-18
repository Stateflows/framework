using Stateflows.StateMachines.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines
{
    public static class CompositeStateInitialBuilderEventsSyncExtensions
    {
        public static ICompositeStateInitialBuilder AddOnInitialize(this ICompositeStateInitialBuilder builder, StateActionDelegate stateAction)
            => builder.AddOnInitialize(stateAction.ToAsync());

        public static ICompositeStateInitialBuilder AddOnEntry(this ICompositeStateInitialBuilder builder, StateActionDelegate stateAction)
            => builder.AddOnEntry(stateAction.ToAsync());

        public static ICompositeStateInitialBuilder AddOnExit(this ICompositeStateInitialBuilder builder, StateActionDelegate stateAction)
            => builder.AddOnExit(stateAction.ToAsync());
    }
}
