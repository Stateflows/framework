using System.Diagnostics;
using Stateflows.Common;
using Stateflows.Common.Extensions;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities
{
    public static class ActivityBuilderEventsTypedExtensions
    {
        [DebuggerHidden]
        public static IActivityBuilder AddDefaultInitializer<TInitializer>(this IActivityBuilder builder)
            where TInitializer : class, IDefaultInitializer
        {
            (builder as IInternal).Services.AddServiceType<TInitializer>();

            return builder.AddDefaultInitializer(c => (c as BaseContext).NodeScope.GetDefaultInitializer<TInitializer>(c)?.OnInitializeAsync());
        }

        [DebuggerHidden]
        public static IActivityBuilder AddInitializer<TInitializationEvent, TInitializer>(this IActivityBuilder builder)
            where TInitializationEvent : EventHolder, new()
            where TInitializer : class, IInitializer<TInitializationEvent>
        {
            (builder as IInternal).Services.AddServiceType<TInitializer>();

            return builder.AddInitializer<TInitializationEvent>(c => (c as BaseContext).NodeScope.GetInitializer<TInitializer, TInitializationEvent>(c)?.OnInitializeAsync(c.InitializationEvent));
        }

        [DebuggerHidden]
        public static IActivityBuilder AddFinalizer<TFinalizer>(this IActivityBuilder builder)
            where TFinalizer : class, IFinalizer
        {
            (builder as IInternal).Services.AddServiceType<TFinalizer>();

            return builder.AddFinalizer(c => (c as BaseContext).NodeScope.GetFinalizer<TFinalizer>(c)?.OnFinalizeAsync());
        }
    }
}
