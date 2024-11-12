using Stateflows.Common;
using System.Diagnostics;
using Stateflows.Common.Extensions;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines
{
    public static class FinalizedOverridenStateMachineBuilderEventsTypedExtensions
    {
        [DebuggerHidden]
        public static IFinalizedOverridenStateMachineBuilder AddDefaultInitializer<TInitializer>(this IFinalizedOverridenStateMachineBuilder builder)
            where TInitializer : class, IDefaultInitializer
        {
            (builder as IInternal).Services.AddServiceType<TInitializer>();

            return builder.AddDefaultInitializer(c => (c as BaseContext).Context.Executor.GetDefaultInitializer<TInitializer>(c)?.OnInitializeAsync());
        }

        [DebuggerHidden]
        public static IFinalizedOverridenStateMachineBuilder AddInitializer<TInitializationEvent, TInitializer>(this IFinalizedOverridenStateMachineBuilder builder)
            where TInitializationEvent : EventHolder, new()
            where TInitializer : class, IInitializer<TInitializationEvent>
        {
            (builder as IInternal).Services.AddServiceType<TInitializer>();

            return builder.AddInitializer<TInitializationEvent>(c => (c as BaseContext).Context.Executor.GetInitializer<TInitializer, TInitializationEvent>(c)?.OnInitializeAsync(c.InitializationEvent));
        }

        [DebuggerHidden]
        public static IFinalizedOverridenStateMachineBuilder AddFinalizer<TFinalizer>(this IFinalizedOverridenStateMachineBuilder builder)
            where TFinalizer : class, IFinalizer
        {
            (builder as IInternal).Services.AddServiceType<TFinalizer>();

            return builder.AddFinalizer(c => (c as BaseContext).Context.Executor.GetFinalizer<TFinalizer>(c)?.OnFinalizeAsync());
        }
    }
}
