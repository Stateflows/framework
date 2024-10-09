﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Extensions;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Registration;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Activities.Registration.Builders;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities
{
    public static class ReactiveStructuredActivityBuilderTypedExtensions
    {
        #region AddAction
        [DebuggerHidden]
        public static IReactiveStructuredActivityBuilder AddAction<TAction>(this IReactiveStructuredActivityBuilder builder, TypedActionBuildAction buildAction = null)
            where TAction : class, IActionNode
            => AddAction<TAction>(builder, ActivityNode<TAction>.Name, buildAction);

        [DebuggerHidden]
        public static IReactiveStructuredActivityBuilder AddAction<TAction>(this IReactiveStructuredActivityBuilder builder, string actionNodeName, TypedActionBuildAction buildAction = null)
            where TAction : class, IActionNode
        {
            (builder as IInternal).Services.AddServiceType<TAction>();

            return builder.AddAction(
                actionNodeName,
                (ActionDelegateAsync)(c =>
                {
                    var action = (c as BaseContext).NodeScope.GetAction<TAction>(c);

                    InputTokens.TokensHolder.Value = ((ActionContext)c).InputTokens;
                    OutputTokens.TokensHolder.Value = ((ActionContext)c).OutputTokens;

                    ActivityNodeContextAccessor.Context.Value = c;
                    var result = action.ExecuteAsync(c.CancellationToken);
                    ActivityNodeContextAccessor.Context.Value = null;

                    return result;
                }),
                b =>
                {
                    (b as NodeBuilder).Node.ScanForDeclaredTypes(typeof(TAction));
                    buildAction?.Invoke(b as ITypedActionBuilder);
                }
            );
        }
        #endregion

        #region AddAcceptEventAction
        [DebuggerHidden]
        public static IReactiveStructuredActivityBuilder AddAcceptEventAction<TEvent, TAcceptEventAction>(this IReactiveStructuredActivityBuilder builder, AcceptEventActionBuildAction buildAction = null)
            where TAcceptEventAction : class, IAcceptEventActionNode<TEvent>
            => builder.AddAcceptEventAction<TEvent, TAcceptEventAction>(ActivityNode<TAcceptEventAction>.Name, buildAction);

        [DebuggerHidden]
        public static IReactiveStructuredActivityBuilder AddAcceptEventAction<TEvent, TAcceptEventAction>(this IReactiveStructuredActivityBuilder builder, string actionNodeName, AcceptEventActionBuildAction buildAction = null)
            where TAcceptEventAction : class, IAcceptEventActionNode<TEvent>
            => builder.AddAcceptEventAction<TEvent>(
                actionNodeName,
                (AcceptEventActionDelegateAsync<TEvent>)(c =>
                {
                    var action = (c as BaseContext).NodeScope.GetAcceptEventAction<TEvent, TAcceptEventAction>(c);

                    InputTokens.TokensHolder.Value = ((ActionContext)c).InputTokens;
                    OutputTokens.TokensHolder.Value = ((ActionContext)c).OutputTokens;

                    ActivityNodeContextAccessor.Context.Value = c;
                    var result = action.ExecuteAsync(c.Event, c.CancellationToken);
                    ActivityNodeContextAccessor.Context.Value = null;

                    return result;
                }),
                buildAction
            );
        #endregion

        #region AddTimeEventAction
        [DebuggerHidden]
        public static IReactiveStructuredActivityBuilder AddTimeEventAction<TTimeEvent, TTimeEventAction>(this IReactiveStructuredActivityBuilder builder, AcceptEventActionBuildAction buildAction = null)
            where TTimeEvent : TimeEvent, new()
            where TTimeEventAction : class, ITimeEventActionNode
            => builder.AddTimeEventAction<TTimeEvent, TTimeEventAction>(ActivityNode<TTimeEventAction>.Name, buildAction);

        [DebuggerHidden]
        public static IReactiveStructuredActivityBuilder AddTimeEventAction<TTimeEvent, TTimeEventAction>(this IReactiveStructuredActivityBuilder builder, string actionNodeName, AcceptEventActionBuildAction buildAction = null)
            where TTimeEvent : TimeEvent, new()
            where TTimeEventAction : class, ITimeEventActionNode
            => builder.AddTimeEventAction<TTimeEvent>(
                actionNodeName,
                c =>
                {
                    var action = (c as BaseContext).NodeScope.GetTimeEventAction<TTimeEventAction>(c);

                    InputTokens.TokensHolder.Value = ((ActionContext)c).InputTokens;
                    OutputTokens.TokensHolder.Value = ((ActionContext)c).OutputTokens;

                    ActivityNodeContextAccessor.Context.Value = c;
                    var result = action.ExecuteAsync(c.CancellationToken);
                    ActivityNodeContextAccessor.Context.Value = null;

                    return result;
                },
                buildAction
            );
        #endregion

        #region AddSendEventAction
        [DebuggerHidden]
        private static async Task<TResult> GetSendEventAction<TEvent, TSendEventAction, TResult>(this IActionContext context, Func<TSendEventAction, Task<TResult>> callback)
            where TSendEventAction : class, ISendEventActionNode<TEvent>
        {
            var action = (context as BaseContext).NodeScope.GetSendEventAction<TEvent, TSendEventAction>(context);

            InputTokens.TokensHolder.Value = ((ActionContext)context).InputTokens;
            OutputTokens.TokensHolder.Value = ((ActionContext)context).OutputTokens;


            ActivityNodeContextAccessor.Context.Value = context;
            var result = await callback(action);
            ActivityNodeContextAccessor.Context.Value = null;

            return result;
        }

        [DebuggerHidden]
        public static IReactiveStructuredActivityBuilder AddSendEventAction<TEvent, TSendEventAction>(this IReactiveStructuredActivityBuilder builder, SendEventActionBuildAction buildAction = null)
            where TSendEventAction : class, ISendEventActionNode<TEvent>
            => builder.AddSendEventAction<TEvent, TSendEventAction>(ActivityNode<TSendEventAction>.Name, buildAction);

        [DebuggerHidden]
        public static IReactiveStructuredActivityBuilder AddSendEventAction<TEvent, TSendEventAction>(this IReactiveStructuredActivityBuilder builder, string actionNodeName, SendEventActionBuildAction buildAction = null)
            where TSendEventAction : class, ISendEventActionNode<TEvent>
        {
            return builder.AddSendEventAction<TEvent>(
                actionNodeName,
                c => c.GetSendEventAction<TEvent, TSendEventAction, TEvent>(a => a.GenerateEventAsync()),
                c => c.GetSendEventAction<TEvent, TSendEventAction, BehaviorId>(a => a.SelectTargetAsync()),
                buildAction
            );
        }
        #endregion

        #region AddStructuredActivity
        [DebuggerHidden]
        public static IReactiveStructuredActivityBuilder AddStructuredActivity<TStructuredActivity>(this IReactiveStructuredActivityBuilder builder, ReactiveStructuredActivityBuildAction buildAction)
            where TStructuredActivity : class, IStructuredActivityNode
            => AddStructuredActivity<TStructuredActivity>(builder, ActivityNode<TStructuredActivity>.Name, buildAction);

        [DebuggerHidden]
        public static IReactiveStructuredActivityBuilder AddStructuredActivity<TStructuredActivity>(this IReactiveStructuredActivityBuilder builder, string structuredActivityName, ReactiveStructuredActivityBuildAction buildAction = null)
            where TStructuredActivity : class, IStructuredActivityNode
        {
            (builder as IInternal).Services.AddServiceType<TStructuredActivity>();

            return builder.AddStructuredActivity(
                structuredActivityName,
                b =>
                {
                    var builder = b as StructuredActivityBuilder;
                    builder.AddStructuredActivityEvents<TStructuredActivity>();
                    builder.Node.ScanForDeclaredTypes(typeof(TStructuredActivity));
                    buildAction?.Invoke(b);
                }
            );
        }
        #endregion

        #region AddParallelActivity
        [DebuggerHidden]
        public static IReactiveStructuredActivityBuilder AddParallelActivity<TToken, TStructuredActivity>(this IReactiveStructuredActivityBuilder builder, ParallelActivityBuildAction buildAction = null, int chunkSize = 1)
            where TStructuredActivity : class, IStructuredActivityNode
            => AddParallelActivity<TToken, TStructuredActivity>(builder, ActivityNode<TStructuredActivity>.Name, buildAction, chunkSize);

        [DebuggerHidden]
        public static IReactiveStructuredActivityBuilder AddParallelActivity<TParallelizationToken, TStructuredActivity>(this IReactiveStructuredActivityBuilder builder, string structuredActivityName, ParallelActivityBuildAction buildAction = null, int chunkSize = 1)
            where TStructuredActivity : class, IStructuredActivityNode
        {
            (builder as IInternal).Services.AddServiceType<TStructuredActivity>();

            return builder.AddParallelActivity<TParallelizationToken>(
                structuredActivityName,
                b =>
                {
                    var builder = b as StructuredActivityBuilder;
                    builder.AddStructuredActivityEvents<TStructuredActivity>();
                    builder.Node.ScanForDeclaredTypes(typeof(TStructuredActivity));
                    buildAction?.Invoke(b);
                },
                chunkSize
            );
        }
        #endregion

        #region AddIterativeActivity
        [DebuggerHidden]
        public static IReactiveStructuredActivityBuilder AddIterativeActivity<TIterationToken, TIterativeActivity>(this IReactiveStructuredActivityBuilder builder, IterativeActivityBuildAction buildAction = null, int chunkSize = 1)
            where TIterativeActivity : class, IStructuredActivityNode
            => AddIterativeActivity<TIterationToken, TIterativeActivity>(builder, ActivityNode<TIterativeActivity>.Name, buildAction, chunkSize);

        [DebuggerHidden]
        public static IReactiveStructuredActivityBuilder AddIterativeActivity<TIterationToken, TIterativeActivity>(this IReactiveStructuredActivityBuilder builder, string structuredActivityName, IterativeActivityBuildAction buildAction = null, int chunkSize = 1)
            where TIterativeActivity : class, IStructuredActivityNode
        {
            (builder as IInternal).Services.AddServiceType<TIterativeActivity>();
            return builder.AddIterativeActivity<TIterationToken>(
                structuredActivityName,
                b =>
                {
                    var builder = b as StructuredActivityBuilder;
                    builder.AddStructuredActivityEvents<TIterativeActivity>();
                    builder.Node.ScanForDeclaredTypes(typeof(TIterativeActivity));
                    buildAction?.Invoke(b);
                },
                chunkSize
            );
        }
        #endregion
    }
}
