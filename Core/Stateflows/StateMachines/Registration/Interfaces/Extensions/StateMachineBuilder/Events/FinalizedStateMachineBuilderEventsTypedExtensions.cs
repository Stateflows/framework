using Stateflows.Common;
using System.Diagnostics;
using Stateflows.Common.Extensions;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Typed
{
    public static class FinalizedStateMachineBuilderEventsTypedExtensions
    {
        [DebuggerHidden]
        public static IFinalizedStateMachineBuilder AddDefaultInitializer<TInitializer>(this IFinalizedStateMachineBuilder builder)
            where TInitializer : class, IDefaultInitializer
        {
            (builder as IInternal).Services.AddServiceType<TInitializer>();

            return builder.AddDefaultInitializer(c => (c as BaseContext).Context.Executor.GetDefaultInitializer<TInitializer>(c)?.OnInitializeAsync());
        }

        [DebuggerHidden]
        public static IFinalizedStateMachineBuilder AddInitializer<TInitializationEvent, TInitializer>(this IFinalizedStateMachineBuilder builder)
            where TInitializationEvent : Event, new()
            where TInitializer : class, IInitializer<TInitializationEvent>
        {
            (builder as IInternal).Services.AddServiceType<TInitializer>();

            return builder.AddInitializer<TInitializationEvent>(c => (c as BaseContext).Context.Executor.GetInitializer<TInitializer, TInitializationEvent>(c)?.OnInitializeAsync(c.InitializationEvent));
        }

        [DebuggerHidden]
        public static IFinalizedStateMachineBuilder AddFinalizer<TFinalizer>(this IFinalizedStateMachineBuilder builder)
            where TFinalizer : class, IFinalizer
        {
            (builder as IInternal).Services.AddServiceType<TFinalizer>();

            return builder.AddFinalizer(c => (c as BaseContext).Context.Executor.GetFinalizer<TFinalizer>(c)?.OnFinalizeAsync());
        }
    }
}
