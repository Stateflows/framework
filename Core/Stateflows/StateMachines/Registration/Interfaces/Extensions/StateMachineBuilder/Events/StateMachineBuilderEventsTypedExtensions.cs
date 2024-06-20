using Stateflows.Common;

namespace Stateflows.StateMachines.Typed
{
    public static class StateMachineBuilderEventsTypedExtensions
    {
        public static IStateMachineBuilder AddDefaultInitializer<TInitializer>(this IStateMachineBuilder builder)
            where TInitializer : DefaultInitializer
            => builder.AddDefaultInitializer(async c => true);

        public static IStateMachineBuilder AddInitializer<TInitializationEvent, TInitializer>(this IStateMachineBuilder builder)
            where TInitializationEvent : Event, new()
            where TInitializer : Initializer<TInitializationEvent>
            => builder.AddInitializer<TInitializationEvent>(async c => true);

        public static IStateMachineBuilder AddFinalizer<TFinalizer>(this IStateMachineBuilder builder)
            where TFinalizer : Finalizer
            => builder.AddFinalizer(async c => { });
    }
}
