using System;
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
    public static class ReactiveStructuredActivityBuilderTypedExtensions
    {
        #region AddAction
        public static IReactiveStructuredActivityBuilder AddAction<TAction>(this IReactiveStructuredActivityBuilder builder, TypedActionBuildAction buildAction = null)
            where TAction : ActionNode
            => AddAction<TAction>(builder, ActivityNodeInfo<TAction>.Name, buildAction);

        public static IReactiveStructuredActivityBuilder AddAction<TAction>(this IReactiveStructuredActivityBuilder builder, string actionNodeName, TypedActionBuildAction buildAction = null)
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

        #region AddAcceptEventAction
        public static IReactiveStructuredActivityBuilder AddAcceptEventAction<TEvent, TAcceptEventAction>(this IReactiveStructuredActivityBuilder builder, AcceptEventActionBuildAction buildAction = null)
            where TEvent : Event, new()
            where TAcceptEventAction : AcceptEventActionNode<TEvent>
            => builder.AddAcceptEventAction<TEvent, TAcceptEventAction>(ActivityNodeInfo<TAcceptEventAction>.Name, buildAction);

        public static IReactiveStructuredActivityBuilder AddAcceptEventAction<TEvent, TAcceptEventAction>(this IReactiveStructuredActivityBuilder builder, string actionNodeName, AcceptEventActionBuildAction buildAction = null)
            where TEvent : Event, new()
            where TAcceptEventAction : AcceptEventActionNode<TEvent>
            => builder.AddAcceptEventAction<TEvent>(
                actionNodeName,
                (AcceptEventActionDelegateAsync<TEvent>)(c =>
                {
                    var action = (c as BaseContext).NodeScope.GetAcceptEventAction<TEvent, TAcceptEventAction>(c);

                    InputTokensHolder.Tokens.Value = ((ActionContext)c).InputTokens;
                    OutputTokensHolder.Tokens.Value = ((ActionContext)c).OutputTokens;

                    var result = action.ExecuteAsync();

                    return result;
                }),
                buildAction
            );
        #endregion

        #region AddTimeEventAction
        public static IReactiveStructuredActivityBuilder AddTimeEventAction<TTimeEvent, TTimeEventAction>(this IReactiveStructuredActivityBuilder builder, AcceptEventActionBuildAction buildAction = null)
            where TTimeEvent : TimeEvent, new()
            where TTimeEventAction : AcceptEventActionNode<TTimeEvent>
            => builder.AddAcceptEventAction<TTimeEvent, TTimeEventAction>(buildAction);

        public static IReactiveStructuredActivityBuilder AddTimeEventAction<TTimeEvent, TTimeEventAction>(this IReactiveStructuredActivityBuilder builder, string actionNodeName, AcceptEventActionBuildAction buildAction = null)
            where TTimeEvent : TimeEvent, new()
            where TTimeEventAction : AcceptEventActionNode<TTimeEvent>
            => builder.AddAcceptEventAction<TTimeEvent, TTimeEventAction>(actionNodeName, buildAction);
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

        public static IReactiveStructuredActivityBuilder AddSendEventAction<TEvent, TSendEventAction>(this IReactiveStructuredActivityBuilder builder, SendEventActionBuildAction buildAction = null)
            where TEvent : Event, new()
            where TSendEventAction : SendEventActionNode<TEvent>
            => builder.AddSendEventAction<TEvent, TSendEventAction>(ActivityNodeInfo<TSendEventAction>.Name, buildAction);

        public static IReactiveStructuredActivityBuilder AddSendEventAction<TEvent, TSendEventAction>(this IReactiveStructuredActivityBuilder builder, string actionNodeName, SendEventActionBuildAction buildAction = null)
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

        #region AddParallelActivity
        public static IReactiveStructuredActivityBuilder AddParallelActivity<TToken, TStructuredActivity>(this IReactiveStructuredActivityBuilder builder, ParallelActivityBuildAction buildAction = null)
            where TStructuredActivity : ParallelActivityNode<TToken>
            => AddParallelActivity<TToken, TStructuredActivity>(builder, ActivityNodeInfo<TStructuredActivity>.Name, buildAction);

        public static IReactiveStructuredActivityBuilder AddParallelActivity<TParallelizationToken, TStructuredActivity>(this IReactiveStructuredActivityBuilder builder, string structuredActivityName, ParallelActivityBuildAction buildAction = null)
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
        public static IReactiveStructuredActivityBuilder AddIterativeActivity<TIterationToken, TStructuredActivity>(this IReactiveStructuredActivityBuilder builder, IterativeActivityBuildAction buildAction = null, int chunkSize = 1)
            where TStructuredActivity : IterativeActivityNode<TIterationToken>
            => AddIterativeActivity<TIterationToken, TStructuredActivity>(builder, ActivityNodeInfo<TStructuredActivity>.Name, buildAction, chunkSize);

        public static IReactiveStructuredActivityBuilder AddIterativeActivity<TIterationToken, TStructuredActivity>(this IReactiveStructuredActivityBuilder builder, string structuredActivityName, IterativeActivityBuildAction buildAction = null, int chunkSize = 1)
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
                },
                chunkSize
            );
        }
        #endregion
    }
}
