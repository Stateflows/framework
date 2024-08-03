using Stateflows.Common;
using System.Diagnostics;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Registration.Interfaces.Internal;
using Stateflows.Common.Extensions;

namespace Stateflows.StateMachines.Typed
{
    public static class StateMachineBuilderEventsTypedExtensions
    {
        [DebuggerHidden]
        public static IStateMachineBuilder AddDefaultInitializer<TInitializer>(this IStateMachineBuilder builder)
            where TInitializer : class, IDefaultInitializer
        {
            (builder as IInternal).Services.AddServiceType<TInitializer>();

            return builder.AddDefaultInitializer(c => (c as BaseContext).Context.Executor.GetDefaultInitializer<TInitializer>(c)?.OnInitializeAsync());
        }

        [DebuggerHidden]
        public static IStateMachineBuilder AddInitializer<TInitializationEvent, TInitializer>(this IStateMachineBuilder builder)
            where TInitializationEvent : Event, new()
            where TInitializer : class, IInitializer<TInitializationEvent>
        {
            (builder as IInternal).Services.AddServiceType<TInitializer>();

            return builder.AddInitializer<TInitializationEvent>(c => (c as BaseContext).Context.Executor.GetInitializer<TInitializer, TInitializationEvent>(c)?.OnInitializeAsync(c.InitializationEvent));
        }

        [DebuggerHidden]
        public static IStateMachineBuilder AddFinalizer<TFinalizer>(this IStateMachineBuilder builder)
            where TFinalizer : class, IFinalizer
        {
            (builder as IInternal).Services.AddServiceType<TFinalizer>();

            return builder.AddFinalizer(c => (c as BaseContext).Context.Executor.GetFinalizer<TFinalizer>(c)?.OnFinalizeAsync());
        }
    }
}
