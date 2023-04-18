using Stateflows.StateMachines.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines
{
    public static class CompositeStateBuilderEventsSyncExtensions
    {
        public static ICompositeStateBuilder AddOnInitialize(this ICompositeStateBuilder builder, StateActionDelegate stateAction)
            => builder.AddOnInitialize(stateAction.ToAsync());

        public static ICompositeStateBuilder AddOnEntry(this ICompositeStateBuilder builder, StateActionDelegate stateAction)
            => builder.AddOnEntry(stateAction.ToAsync());

        public static ICompositeStateBuilder AddOnExit(this ICompositeStateBuilder builder, StateActionDelegate stateAction)
            => builder.AddOnExit(stateAction.ToAsync());
    }
}
