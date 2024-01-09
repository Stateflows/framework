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

        public static IStructuredActivityBuilder AddSendEventAction<TEvent, TSendEventAction>(this IStructuredActivityBuilder builder, SendEventActionBuilderAction buildAction = null)
            where TEvent : Event, new()
            where TSendEventAction : SendEventAction<TEvent>
            => builder.AddSendEventAction<TEvent, TSendEventAction>(ActivityNodeInfo<TSendEventAction>.Name, buildAction);

        public static IStructuredActivityBuilder AddSendEventAction<TEvent, TSendEventAction>(this IStructuredActivityBuilder builder, string actionNodeName, SendEventActionBuilderAction buildAction = null)
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

        #region AddReactiveStructuredActivity
        public static IReactiveStructuredActivityBuilder AddStructuredActivity<TStructuredActivity>(this IReactiveStructuredActivityBuilder builder, ReactiveStructuredActivityBuilderAction buildAction = null)
            where TStructuredActivity : StructuredActivity
            => AddStructuredActivity<TStructuredActivity>(builder, ActivityNodeInfo<TStructuredActivity>.Name, buildAction);

        public static IReactiveStructuredActivityBuilder AddStructuredActivity<TStructuredActivity>(this IReactiveStructuredActivityBuilder builder, string structuredActivityName, ReactiveStructuredActivityBuilderAction buildAction = null)
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
                    var builder = b as StructuredActivityBuilder;
                    builder.AddStructuredActivityEvents<TStructuredActivity>();
                    builder.Node.ScanForDeclaredTypes(typeof(TStructuredActivity));
                    buildAction?.Invoke(b);
                }
            );
        }
        #endregion

        #region AddParallelActivity
        public static IStructuredActivityBuilder AddParallelActivity<TStructuredActivity, TParallelizationToken>(this IStructuredActivityBuilder builder, StructuredActivityBuilderAction buildAction = null)
            where TStructuredActivity : StructuredActivity
            where TParallelizationToken : Token, new()
            => AddParallelActivity<TStructuredActivity, TParallelizationToken>(builder, ActivityNodeInfo<TStructuredActivity>.Name, buildAction);

        public static IStructuredActivityBuilder AddParallelActivity<TStructuredActivity, TParallelizationToken>(this IStructuredActivityBuilder builder, string structuredActivityName, StructuredActivityBuilderAction buildAction = null)
            where TStructuredActivity : StructuredActivity
            where TParallelizationToken : Token, new()
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
        public static IStructuredActivityBuilder AddIterativeActivity<TStructuredActivity, TIterationToken>(this IStructuredActivityBuilder builder, StructuredActivityBuilderAction buildAction = null)
            where TStructuredActivity : StructuredActivity
            where TIterationToken : Token, new()
            => AddIterativeActivity<TStructuredActivity, TIterationToken>(builder, ActivityNodeInfo<TStructuredActivity>.Name, buildAction);

        public static IStructuredActivityBuilder AddIterativeActivity<TStructuredActivity, TIterationToken>(this IStructuredActivityBuilder builder, string structuredActivityName, StructuredActivityBuilderAction buildAction = null)
            where TStructuredActivity : StructuredActivity
            where TIterationToken : Token, new()
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
                }
            );
        }
        #endregion
    }
}
