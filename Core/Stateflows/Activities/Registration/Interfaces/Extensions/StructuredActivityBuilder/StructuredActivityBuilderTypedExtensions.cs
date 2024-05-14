using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Registration;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Activities.Registration.Builders;

namespace Stateflows.Activities.Typed
{
    public static class StructuredActivityBuilderTypedExtensions
    {
        #region AddAction
        public static IStructuredActivityBuilder AddAction<TAction>(this IStructuredActivityBuilder builder, TypedActionBuildAction buildAction = null)
            where TAction : ActionNode
            => AddAction<TAction>(builder, ActivityNodeInfo<TAction>.Name, buildAction);

        public static IStructuredActivityBuilder AddAction<TAction>(this IStructuredActivityBuilder builder, string actionNodeName, TypedActionBuildAction buildAction = null)
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
            where TSendEventAction : SendEventActionNode<TEvent>
        {
            var action = (context as BaseContext).NodeScope.GetSendEventAction<TEvent, TSendEventAction>(context);

            InputTokensHolder.Tokens.Value = ((ActionContext)context).InputTokens;
            OutputTokensHolder.Tokens.Value = ((ActionContext)context).OutputTokens;

            return await callback(action);
        }

        public static IStructuredActivityBuilder AddSendEventAction<TEvent, TSendEventAction>(this IStructuredActivityBuilder builder, SendEventActionBuildAction buildAction = null)
            where TEvent : Event, new()
            where TSendEventAction : SendEventActionNode<TEvent>
            => builder.AddSendEventAction<TEvent, TSendEventAction>(ActivityNodeInfo<TSendEventAction>.Name, buildAction);

        public static IStructuredActivityBuilder AddSendEventAction<TEvent, TSendEventAction>(this IStructuredActivityBuilder builder, string actionNodeName, SendEventActionBuildAction buildAction = null)
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

        #region AddReactiveStructuredActivity
        public static IReactiveStructuredActivityBuilder AddStructuredActivity<TStructuredActivity>(this IReactiveStructuredActivityBuilder builder, ReactiveStructuredActivityBuildAction buildAction = null)
            where TStructuredActivity : StructuredActivityNode
            => AddStructuredActivity<TStructuredActivity>(builder, ActivityNodeInfo<TStructuredActivity>.Name, buildAction);

        public static IReactiveStructuredActivityBuilder AddStructuredActivity<TStructuredActivity>(this IReactiveStructuredActivityBuilder builder, string structuredActivityName, ReactiveStructuredActivityBuildAction buildAction = null)
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

        #region AddStructuredActivity
        public static IStructuredActivityBuilder AddStructuredActivity<TStructuredActivity>(this IStructuredActivityBuilder builder, StructuredActivityBuildAction buildAction = null)
            where TStructuredActivity : StructuredActivityNode
            => AddStructuredActivity<TStructuredActivity>(builder, ActivityNodeInfo<TStructuredActivity>.Name, buildAction);

        public static IStructuredActivityBuilder AddStructuredActivity<TStructuredActivity>(this IStructuredActivityBuilder builder, string structuredActivityName, StructuredActivityBuildAction buildAction = null)
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
        public static IStructuredActivityBuilder AddParallelActivity<TParallelizationToken, TStructuredActivity>(this IStructuredActivityBuilder builder, ParallelActivityBuildAction buildAction = null)
            where TParallelizationToken : TokenHolder, new()
            where TStructuredActivity : ParallelActivityNode<TParallelizationToken>
            => AddParallelActivity<TParallelizationToken, TStructuredActivity>(builder, ActivityNodeInfo<TStructuredActivity>.Name, buildAction);

        public static IStructuredActivityBuilder AddParallelActivity<TParallelizationToken, TStructuredActivity>(this IStructuredActivityBuilder builder, string structuredActivityName, ParallelActivityBuildAction buildAction = null)
            where TParallelizationToken : TokenHolder, new()
            where TStructuredActivity : ParallelActivityNode<TParallelizationToken>
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
        public static IStructuredActivityBuilder AddIterativeActivity<TIterationToken, TStructuredActivity>(this IStructuredActivityBuilder builder, IterativeActivityBuildAction buildAction = null)
            where TIterationToken : TokenHolder, new()
            where TStructuredActivity : IterativeActivityNode<TIterationToken>
            => AddIterativeActivity<TIterationToken, TStructuredActivity>(builder, ActivityNodeInfo<TStructuredActivity>.Name, buildAction);

        public static IStructuredActivityBuilder AddIterativeActivity<TIterationToken, TStructuredActivity>(this IStructuredActivityBuilder builder, string structuredActivityName, IterativeActivityBuildAction buildAction = null)
            where TIterationToken : TokenHolder, new()
            where TStructuredActivity : IterativeActivityNode<TIterationToken>
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
