using Stateflows.Activities.Extensions;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Registration.Interfaces;
using System.Linq;
using System.Reflection;
using Stateflows.Activities.Attributes;
using Stateflows.Common.Extensions;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;
using System.ComponentModel;
using Stateflows.Activities.Collections;
using static System.Collections.Specialized.BitVector32;
using System.Text.Json;
using Stateflows.Activities.Registration;

namespace Stateflows.Activities
{
    public static class ActivityBuilderTypedExtensions
    {
        #region AddAction
        public static IActivityBuilder AddAction<TAction>(this IActivityBuilder builder, TypedActionBuilderAction buildAction = null)
            where TAction : Action
            => AddAction<TAction>(builder, ActivityNodeInfo<TAction>.Name, buildAction);

        public static IActivityBuilder AddAction<TAction>(this IActivityBuilder builder, string actionNodeName, TypedActionBuilderAction buildAction = null)
            where TAction : Action
        {
            (builder as IInternal).Services.RegisterAction<TAction>();

            return builder.AddAction(
                actionNodeName,
                c =>
                {
                    var action = (c as BaseContext).NodeScope.GetAction<TAction>(c);

                    InputTokensHolder.Tokens.Value = c.Input;
                    OutputTokensHolder.Tokens.Value = ((ActionContext)c).OutputTokens;

                    var result = action.ExecuteAsync();

                    return result;
                },
                b =>
                {
                    (b as NodeBuilder).Node.ScanForDeclaredTypes(typeof(TAction));
                    buildAction?.Invoke(b as ITypedActionBuilder);
                }
            );
        }
        #endregion

        //public static IActionBuilder AddControlFlow<TControlFlow>(this IActionBuilder builder, string targetNodeName)
        //    where TControlFlow : ControlFlow
        //{
        //    var self = builder as IActionBuilderInternal;
        //    self.Services.RegisterControlFlow<TControlFlow>();

        //    return builder.AddControlFlow(targetNodeName, b => b.AddControlFlowEvents<TControlFlow>());
        //}

        //#region AddSignalAction
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
        //#endregion

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
        public static IActivityBuilder AddStructuredActivity<TStructuredActivity>(this IActivityBuilder builder, StructuredActivityBuilderAction buildAction = null)
            where TStructuredActivity : StructuredActivity
            => AddStructuredActivity<TStructuredActivity>(builder, ActivityNodeInfo<TStructuredActivity>.Name, buildAction);

        public static IActivityBuilder AddStructuredActivity<TStructuredActivity>(this IActivityBuilder builder, string structuredActivityName, StructuredActivityBuilderAction buildAction = null)
            where TStructuredActivity : StructuredActivity
        {
            (builder as IInternal).Services.RegisterStructuredActivity<TStructuredActivity>();

            return builder.AddStructuredActivity(
                structuredActivityName,
                b =>
                {
                    b.AddStructuredActivityEvents<TStructuredActivity>();
                    (b as NodeBuilder).Node.ScanForDeclaredTypes(typeof(TStructuredActivity));
                    buildAction?.Invoke(b);
                }
            );
        }
        #endregion

        #region AddParallelActivity
        public static IActivityBuilder AddParallelActivity<TToken, TStructuredActivity>(this IActivityBuilder builder, StructuredActivityBuilderAction buildAction = null)
            where TToken : Token, new()
            where TStructuredActivity : StructuredActivity
            => AddParallelActivity<TToken, TStructuredActivity>(builder, ActivityNodeInfo<TStructuredActivity>.Name, buildAction);

        public static IActivityBuilder AddParallelActivity<TToken, TStructuredActivity>(this IActivityBuilder builder, string structuredActivityName, StructuredActivityBuilderAction buildAction = null)
            where TToken : Token, new()
            where TStructuredActivity : StructuredActivity
        {
            (builder as IInternal).Services.RegisterStructuredActivity<TStructuredActivity>();

            return builder.AddParallelActivity<TToken>(
                structuredActivityName,
                b =>
                {
                    b.AddStructuredActivityEvents<TStructuredActivity>();
                    (b as NodeBuilder).Node.ScanForDeclaredTypes(typeof(TStructuredActivity));
                    buildAction?.Invoke(b);
                }
            );
        }
        #endregion

        #region AddIterativeActivity
        public static IActivityBuilder AddIterativeActivity<TToken, TStructuredActivity>(this IActivityBuilder builder, StructuredActivityBuilderAction buildAction = null)
            where TToken : Token, new()
            where TStructuredActivity : StructuredActivity
            => AddIterativeActivity<TToken, TStructuredActivity>(builder, ActivityNodeInfo<TStructuredActivity>.Name, buildAction);

        public static IActivityBuilder AddIterativeActivity<TToken, TStructuredActivity>(this IActivityBuilder builder, string structuredActivityName, StructuredActivityBuilderAction buildAction = null)
            where TToken : Token, new()
            where TStructuredActivity : StructuredActivity
        {
            (builder as IInternal).Services.RegisterStructuredActivity<TStructuredActivity>();
            return builder.AddIterativeActivity<TToken>(
                structuredActivityName,
                b =>
                {
                    b.AddStructuredActivityEvents<TStructuredActivity>();
                    (b as NodeBuilder).Node.ScanForDeclaredTypes(typeof(TStructuredActivity));
                    buildAction?.Invoke(b);
                }
            );
        }
        #endregion
    }
}
