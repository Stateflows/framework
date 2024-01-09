using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Collections;
using Stateflows.Activities.Registration;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Activities.Registration.Builders;

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
                (ActionDelegateAsync)(                c =>
                {
                    var action = (c as BaseContext).NodeScope.GetAction<TAction>(c);

                    InputTokensHolder.Tokens.Value = c.InputTokens;
                    OutputTokensHolder.Tokens.Value = ((ActionContext)c).Output;

                    var result = action.ExecuteAsync();

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
        public static IActivityBuilder AddAcceptEventAction<TEvent, TAcceptEventAction>(this IActivityBuilder builder, AcceptEventActionBuilderAction buildAction = null)
            where TEvent : Event, new()
            where TAcceptEventAction : AcceptEventAction<TEvent>
            => builder.AddAcceptEventAction<TEvent, TAcceptEventAction>(ActivityNodeInfo<TAcceptEventAction>.Name, buildAction);

        public static IActivityBuilder AddAcceptEventAction<TEvent, TAcceptEventAction>(this IActivityBuilder builder, string actionNodeName, AcceptEventActionBuilderAction buildAction = null)
            where TEvent : Event, new()
            where TAcceptEventAction : AcceptEventAction<TEvent>
            => builder.AddAcceptEventAction<TEvent>(
                actionNodeName,
                (AcceptEventActionDelegateAsync<TEvent>)(c =>
                {
                    var action = (c as BaseContext).NodeScope.GetAcceptEventAction<TEvent, TAcceptEventAction>(c);

                    InputTokensHolder.Tokens.Value = c.InputTokens;
                    OutputTokensHolder.Tokens.Value = ((ActionContext)c).Output;

                    var result = action.ExecuteAsync();

                    return result;
                }),
                buildAction
            );
        #endregion

        #region AddSendEventAction
        private static async Task<TResult> GetSendEventAction<TEvent, TSendEventAction, TResult>(this IActionContext context, Func<TSendEventAction, Task<TResult>> callback)
            where TEvent : Event, new()
            where TSendEventAction : SendEventAction<TEvent>
        {
            var action = (context as BaseContext).NodeScope.GetSendEventAction<TEvent, TSendEventAction>(context);

            InputTokensHolder.Tokens.Value = context.InputTokens;
            OutputTokensHolder.Tokens.Value = ((ActionContext)context).Output;

            return await callback(action);
        }

        public static IActivityBuilder AddSendEventAction<TEvent, TSendEventAction>(this IActivityBuilder builder, SendEventActionBuilderAction buildAction = null)
            where TEvent : Event, new()
            where TSendEventAction : SendEventAction<TEvent>
            => builder.AddSendEventAction<TEvent, TSendEventAction>(ActivityNodeInfo<TSendEventAction>.Name, buildAction);

        public static IActivityBuilder AddSendEventAction<TEvent, TSendEventAction>(this IActivityBuilder builder, string actionNodeName, SendEventActionBuilderAction buildAction = null)
            where TEvent : Event, new()
            where TSendEventAction : SendEventAction<TEvent>
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
        public static IActivityBuilder AddStructuredActivity<TStructuredActivity>(this IActivityBuilder builder, ReactiveStructuredActivityBuilderAction buildAction = null)
            where TStructuredActivity : StructuredActivity
            => AddStructuredActivity<TStructuredActivity>(builder, ActivityNodeInfo<TStructuredActivity>.Name, buildAction);

        public static IActivityBuilder AddStructuredActivity<TStructuredActivity>(this IActivityBuilder builder, string structuredActivityName, ReactiveStructuredActivityBuilderAction buildAction = null)
            where TStructuredActivity : StructuredActivity
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
        public static IActivityBuilder AddParallelActivity<TToken, TStructuredActivity>(this IActivityBuilder builder, ParallelActivityBuilderAction buildAction = null)
            where TToken : Token, new()
            where TStructuredActivity : StructuredActivity
            => AddParallelActivity<TToken, TStructuredActivity>(builder, ActivityNodeInfo<TStructuredActivity>.Name, buildAction);

        public static IActivityBuilder AddParallelActivity<TToken, TStructuredActivity>(this IActivityBuilder builder, string structuredActivityName, ParallelActivityBuilderAction buildAction = null)
            where TToken : Token, new()
            where TStructuredActivity : StructuredActivity
        {
            (builder as IInternal).Services.RegisterStructuredActivity<TStructuredActivity>();

            return builder.AddParallelActivity<TToken>(
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
        public static IActivityBuilder AddIterativeActivity<TToken, TStructuredActivity>(this IActivityBuilder builder, IterativeActivityBuilderAction buildAction = null)
            where TToken : Token, new()
            where TStructuredActivity : StructuredActivity
            => AddIterativeActivity<TToken, TStructuredActivity>(builder, ActivityNodeInfo<TStructuredActivity>.Name, buildAction);

        public static IActivityBuilder AddIterativeActivity<TToken, TStructuredActivity>(this IActivityBuilder builder, string structuredActivityName, IterativeActivityBuilderAction buildAction = null)
            where TToken : Token, new()
            where TStructuredActivity : StructuredActivity
        {
            (builder as IInternal).Services.RegisterStructuredActivity<TStructuredActivity>();
            return builder.AddIterativeActivity<TToken>(
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
    }
}
