using Stateflows.Activities.Extensions;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities
{
    public static class StructuredActivityBuilderTypedExtensions
    {
        #region AddAction
        public static IStructuredActivityBuilder AddAction<TAction>(this IStructuredActivityBuilder builder, TypedActionBuilderAction buildAction = null)
            where TAction : Action
            => AddAction<TAction>(builder, ActivityNodeInfo<TAction>.Name, buildAction);

        public static IStructuredActivityBuilder AddAction<TAction>(this IStructuredActivityBuilder builder, string actionNodeName, TypedActionBuilderAction buildAction = null)
            where TAction : Action
        {
            (builder as IInternal).Services.RegisterAction<TAction>();

            return builder.AddAction(
                actionNodeName,
                c => (c as BaseContext).NodeScope.GetAction<TAction>(c).ExecuteAsync(),
                b =>
                {
                    //(b as NodeBuilder).Node.ScanForDeclaredTypes(typeof(TAction));
                    buildAction?.Invoke(b as ITypedActionBuilder);
                }
            );
        }
        #endregion

        #region AddSignalAction
        //public static IActivityBuilder AddSignalAction<TEvent, TSignalAction>(
        //    this IActivityBuilder builder,
        //    SignalActionBuilderAction signalActionBuildAction
        //)
        //    where TEvent : Event
        //    where TSignalAction : SignalAction<TEvent>
        //    => AddSignalAction<TEvent, TSignalAction>(builder, ActivityNodeInfo<TSignalAction>.Name, signalActionBuildAction);

        //public static IActivityBuilder AddSignalAction<TEvent, TSignalAction>(
        //    this IActivityBuilder builder,
        //    string signalActionNodeName,
        //    SignalActionBuilderAction signalActionBuildAction
        //)
        //    where TEvent : Event
        //    where TSignalAction : SignalAction<TEvent>
        //{
        //    var self = builder as IActivityBuilderInternal;
        //    self.Services.RegisterAction<TSignalAction>();

        //    return builder.AddSignalAction(
        //        signalActionNodeName,
        //        c => (c as BaseContext).Context.Executor.GetAction<TSignalAction>(c).GenerateEventAsync(),
        //        c => (c as BaseContext).Context.Executor.GetAction<TSignalAction>(c).SelectTargetAsync(),
        //        signalActionBuildAction
        //    );
        //}
        #endregion

        #region AddEventAction
        //public static IActivityBuilder AddEventAction<TEvent, TEventAction>(this IActivityBuilder builder, EventActionBuilderAction buildAction = null)
        //    where TEvent : Event
        //    where TEventAction : EventAction<TEvent>
        //{
        //    return builder;
        //}

        //public static IActivityBuilder AddEventAction<TEvent, TEventAction>(this IActivityBuilder builder, string actionNodeName, EventActionBuilderAction buildAction = null)
        //    where TEvent : Event
        //    where TEventAction : EventAction<TEvent>
        //{
        //    return builder;
        //}
        #endregion

        #region AddStructuredActivity
        public static IStructuredActivityBuilder AddStructuredActivity<TStructuredActivity>(this IStructuredActivityBuilder builder, StructuredActivityBuilderAction buildAction = null)
            where TStructuredActivity : StructuredActivity
            => AddStructuredActivity<TStructuredActivity>(builder, ActivityNodeInfo<TStructuredActivity>.Name, buildAction);

        public static IStructuredActivityBuilder AddStructuredActivity<TStructuredActivity>(this IStructuredActivityBuilder builder, string structuredActivityName, StructuredActivityBuilderAction buildAction = null)
            where TStructuredActivity : StructuredActivity
        {
            (builder as IInternal).Services.RegisterStructuredActivity<TStructuredActivity>();

            return builder.AddStructuredActivity(
                structuredActivityName,
                b =>
                {
                    b.AddStructuredActivityEvents<TStructuredActivity>();
                    //(b as NodeBuilder).Node.ScanForDeclaredTypes(typeof(TAction));
                    buildAction?.Invoke(b);
                }
            );
        }
        #endregion

        #region AddParallelActivity
        public static IStructuredActivityBuilder AddParallelActivity<TToken, TStructuredActivity>(this IStructuredActivityBuilder builder, StructuredActivityBuilderAction buildAction = null)
            where TToken : Token, new()
            where TStructuredActivity : StructuredActivity
            => AddParallelActivity<TToken, TStructuredActivity>(builder, ActivityNodeInfo<TStructuredActivity>.Name, buildAction);

        public static IStructuredActivityBuilder AddParallelActivity<TToken, TStructuredActivity>(this IStructuredActivityBuilder builder, string structuredActivityName, StructuredActivityBuilderAction buildAction = null)
            where TToken : Token, new()
            where TStructuredActivity : StructuredActivity
        {
            (builder as IInternal).Services.RegisterStructuredActivity<TStructuredActivity>();

            return builder.AddParallelActivity<TToken>(
                structuredActivityName,
                b =>
                {
                    b.AddStructuredActivityEvents<TStructuredActivity>();
                    //(b as NodeBuilder).Node.ScanForDeclaredTypes(typeof(TAction));
                    buildAction?.Invoke(b);
                }
            );
        }
        #endregion

        #region AddIterativeActivity
        public static IStructuredActivityBuilder AddIterativeActivity<TToken, TStructuredActivity>(this IStructuredActivityBuilder builder, StructuredActivityBuilderAction buildAction = null)
            where TToken : Token, new()
            where TStructuredActivity : StructuredActivity
            => AddIterativeActivity<TToken, TStructuredActivity>(builder, ActivityNodeInfo<TStructuredActivity>.Name, buildAction);

        public static IStructuredActivityBuilder AddIterativeActivity<TToken, TStructuredActivity>(this IStructuredActivityBuilder builder, string structuredActivityName, StructuredActivityBuilderAction buildAction = null)
            where TToken : Token, new()
            where TStructuredActivity : StructuredActivity
        {
            (builder as IInternal).Services.RegisterStructuredActivity<TStructuredActivity>();

            return builder.AddIterativeActivity<TToken>(
                structuredActivityName,
                b =>
                {
                    b.AddStructuredActivityEvents<TStructuredActivity>();
                    //(b as NodeBuilder).Node.ScanForDeclaredTypes(typeof(TAction));
                    buildAction?.Invoke(b);
                }
            );
        }
        #endregion
    }
}
