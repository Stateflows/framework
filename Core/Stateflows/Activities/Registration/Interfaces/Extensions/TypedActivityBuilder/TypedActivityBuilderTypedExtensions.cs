﻿using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Registration;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Activities.Registration.Builders;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Typed
{
    public static class TypedActivityBuilderTypedExtensions
    {
        #region AddAction
        public static ITypedActivityBuilder AddAction<TAction>(this ITypedActivityBuilder builder, TypedActionBuildAction buildAction = null)
            where TAction : ActionNode
            => AddAction<TAction>(builder, ActivityNodeInfo<TAction>.Name, buildAction);

        public static ITypedActivityBuilder AddAction<TAction>(this ITypedActivityBuilder builder, string actionNodeName, TypedActionBuildAction buildAction = null)
            where TAction : ActionNode
        {
            (builder as IInternal).Services.RegisterAction<TAction>();

            return builder.AddAction(
                actionNodeName,
                (ActionDelegateAsync)(c =>
                {
                    var action = (c as BaseContext).NodeScope.GetAction<TAction>(c);

                    InputTokensHolder.Tokens.Value = ((ActionContext)c).InputTokens;
                    OutputTokensHolder.Tokens.Value = ((ActionContext)c).OutputTokens;

                    ActivityNodeContextAccessor.Context.Value = c;
                    var result = action.ExecuteAsync();
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
        public static ITypedActivityBuilder AddAcceptEventAction<TEvent, TAcceptEventAction>(this ITypedActivityBuilder builder, AcceptEventActionBuildAction buildAction = null)
            where TEvent : Event, new()
            where TAcceptEventAction : AcceptEventActionNode<TEvent>
            => builder.AddAcceptEventAction<TEvent, TAcceptEventAction>(ActivityNodeInfo<TAcceptEventAction>.Name, buildAction);

        public static ITypedActivityBuilder AddAcceptEventAction<TEvent, TAcceptEventAction>(this ITypedActivityBuilder builder, string actionNodeName, AcceptEventActionBuildAction buildAction = null)
            where TEvent : Event, new()
            where TAcceptEventAction : AcceptEventActionNode<TEvent>
            => builder.AddAcceptEventAction<TEvent>(
                actionNodeName,
                (AcceptEventActionDelegateAsync<TEvent>)(c =>
                {
                    var action = (c as BaseContext).NodeScope.GetAcceptEventAction<TEvent, TAcceptEventAction>(c);

                    InputTokensHolder.Tokens.Value = ((ActionContext)c).InputTokens;
                    OutputTokensHolder.Tokens.Value = ((ActionContext)c).OutputTokens;

                    ActivityNodeContextAccessor.Context.Value = c;
                    var result = action.ExecuteAsync();
                    ActivityNodeContextAccessor.Context.Value = null;

                    return result;
                }),
                buildAction
            );
        #endregion

        #region AddSendEventAction
        private static async Task<TResult> GetSendEventAction<TEvent, TSendEventAction, TResult>(this IActionContext context, Func<TSendEventAction, Task<TResult>> callback)
            where TEvent : Event, new()
            where TSendEventAction : SendEventActionNode<TEvent>
        {
            var action = (context as BaseContext).NodeScope.GetSendEventAction<TEvent, TSendEventAction>(context);

            InputTokensHolder.Tokens.Value = ((ActionContext)context).InputTokens;
            OutputTokensHolder.Tokens.Value = ((ActionContext)context).OutputTokens;

            ActivityNodeContextAccessor.Context.Value = context;
            var result = await callback(action);
            ActivityNodeContextAccessor.Context.Value = null;

            return result;
        }

        public static ITypedActivityBuilder AddSendEventAction<TEvent, TSendEventAction>(this ITypedActivityBuilder builder, SendEventActionBuildAction buildAction = null)
            where TEvent : Event, new()
            where TSendEventAction : SendEventActionNode<TEvent>
            => builder.AddSendEventAction<TEvent, TSendEventAction>(ActivityNodeInfo<TSendEventAction>.Name, buildAction);

        public static ITypedActivityBuilder AddSendEventAction<TEvent, TSendEventAction>(this ITypedActivityBuilder builder, string actionNodeName, SendEventActionBuildAction buildAction = null)
            where TEvent : Event, new()
            where TSendEventAction : SendEventActionNode<TEvent>
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
        public static ITypedActivityBuilder AddStructuredActivity<TStructuredActivity>(this ITypedActivityBuilder builder, ReactiveStructuredActivityBuildAction buildAction = null)
            where TStructuredActivity : StructuredActivityNode
            => AddStructuredActivity<TStructuredActivity>(builder, ActivityNodeInfo<TStructuredActivity>.Name, buildAction);

        public static ITypedActivityBuilder AddStructuredActivity<TStructuredActivity>(this ITypedActivityBuilder builder, string structuredActivityName, ReactiveStructuredActivityBuildAction buildAction = null)
            where TStructuredActivity : StructuredActivityNode
        {
            (builder as IInternal).Services.RegisterStructuredActivity<TStructuredActivity>();

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
        public static ITypedActivityBuilder AddParallelActivity<TParallelizationToken, TStructuredActivity>(this ITypedActivityBuilder builder, ParallelActivityBuildAction buildAction = null)
            where TStructuredActivity : StructuredActivityNode<TParallelizationToken>
            => AddParallelActivity<TParallelizationToken, TStructuredActivity>(builder, ActivityNodeInfo<TStructuredActivity>.Name, buildAction);

        public static ITypedActivityBuilder AddParallelActivity<TParallelizationToken, TStructuredActivity>(this ITypedActivityBuilder builder, string structuredActivityName, ParallelActivityBuildAction buildAction = null)
            where TStructuredActivity : StructuredActivityNode<TParallelizationToken>
        {
            (builder as IInternal).Services.RegisterStructuredActivity<TStructuredActivity>();

            return builder.AddParallelActivity<TParallelizationToken>(
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

        #region AddIterativeActivity
        public static ITypedActivityBuilder AddIterativeActivity<TIterationToken, TStructuredActivity>(this ITypedActivityBuilder builder, IterativeActivityBuildAction buildAction = null, int chunkSize = 1)
            where TStructuredActivity : StructuredActivityNode<TIterationToken>
            => AddIterativeActivity<TIterationToken, TStructuredActivity>(builder, ActivityNodeInfo<TStructuredActivity>.Name, buildAction, chunkSize);

        public static ITypedActivityBuilder AddIterativeActivity<TIterationToken, TStructuredActivity>(this ITypedActivityBuilder builder, string structuredActivityName, IterativeActivityBuildAction buildAction = null, int chunkSize = 1)
            where TStructuredActivity : StructuredActivityNode<TIterationToken>
        {
            (builder as IInternal).Services.RegisterStructuredActivity<TStructuredActivity>();
            return builder.AddIterativeActivity<TIterationToken>(
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
    }
}
