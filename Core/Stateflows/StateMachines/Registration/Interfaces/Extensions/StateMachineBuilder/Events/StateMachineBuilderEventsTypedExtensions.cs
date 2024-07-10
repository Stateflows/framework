using Stateflows.Common;

namespace Stateflows.StateMachines.Typed
{
    public static class StateMachineBuilderEventsTypedExtensions
    {
        public static IStateMachineBuilder AddDefaultInitializer<TInitializer>(this IStateMachineBuilder builder)
            where TInitializer : class, IDefaultInitializer
            => builder.AddDefaultInitializer(async c => true);

        public static IStateMachineBuilder AddInitializer<TInitializationEvent, TInitializer>(this IStateMachineBuilder builder)
            where TInitializationEvent : Event, new()
            where TInitializer : class, IInitializer<TInitializationEvent>
            => builder.AddInitializer<TInitializationEvent>(async c => true);

        public static IStateMachineBuilder AddFinalizer<TFinalizer>(this IStateMachineBuilder builder)
            where TFinalizer : class, IFinalizer
            => builder.AddFinalizer(async c => { });
    }
}
