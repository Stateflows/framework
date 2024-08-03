using System;
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

namespace Stateflows.Activities.Typed
{
    public static class ActivityBuilderTypedExtensions
    {
        #region AddAction
        [DebuggerHidden]
        public static IActivityBuilder AddAction<TAction>(this IActivityBuilder builder, TypedActionBuildAction buildAction = null)
            where TAction : class, IActionNode
            => AddAction<TAction>(builder, ActivityNode<TAction>.Name, buildAction);

        [DebuggerHidden]
        public static IActivityBuilder AddAction<TAction>(this IActivityBuilder builder, string actionNodeName, TypedActionBuildAction buildAction = null)
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
        public static IActivityBuilder AddAcceptEventAction<TEvent, TAcceptEventAction>(this IActivityBuilder builder, AcceptEventActionBuildAction buildAction = null)
            where TEvent : Event, new()
            where TAcceptEventAction : class, IAcceptEventActionNode<TEvent>
            => builder.AddAcceptEventAction<TEvent, TAcceptEventAction>(ActivityNode<TAcceptEventAction>.Name, buildAction);

        [DebuggerHidden]
        public static IActivityBuilder AddAcceptEventAction<TEvent, TAcceptEventAction>(this IActivityBuilder builder, string actionNodeName, AcceptEventActionBuildAction buildAction = null)
            where TEvent : Event, new()
            where TAcceptEventAction : class, IAcceptEventActionNode<TEvent>
            => builder.AddAcceptEventAction<TEvent>(
                actionNodeName,
                c =>
                {
                    var action = (c as BaseContext).NodeScope.GetAcceptEventAction<TEvent, TAcceptEventAction>(c);

                    InputTokens.TokensHolder.Value = ((ActionContext)c).InputTokens;
                    OutputTokens.TokensHolder.Value = ((ActionContext)c).OutputTokens;

                    ActivityNodeContextAccessor.Context.Value = c;
                    var result = action.ExecuteAsync(c.Event, c.CancellationToken);
                    ActivityNodeContextAccessor.Context.Value = null;

                    return result;
                },
                buildAction
            );
        #endregion

        #region AddTimeEventAction
        [DebuggerHidden]
        public static IActivityBuilder AddTimeEventAction<TTimeEvent, TTimeEventAction>(this IActivityBuilder builder, AcceptEventActionBuildAction buildAction = null)
            where TTimeEvent : TimeEvent, new()
            where TTimeEventAction : class, ITimeEventActionNode
            => builder.AddTimeEventAction<TTimeEvent, TTimeEventAction>(ActivityNode<TTimeEventAction>.Name, buildAction);

        [DebuggerHidden]
        public static IActivityBuilder AddTimeEventAction<TTimeEvent, TTimeEventAction>(this IActivityBuilder builder, string actionNodeName, AcceptEventActionBuildAction buildAction = null)
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
        private static async Task<TResult> GetSendEventAction<TEvent, TSendEventAction, TResult>(this IActionContext context, Func<TSendEventAction, Task<TResult>> callback)
            where TEvent : Event, new()
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
        public static IActivityBuilder AddSendEventAction<TEvent, TSendEventAction>(this IActivityBuilder builder, SendEventActionBuildAction buildAction = null)
            where TEvent : Event, new()
            where TSendEventAction : class, ISendEventActionNode<TEvent>
            => builder.AddSendEventAction<TEvent, TSendEventAction>(ActivityNode<TSendEventAction>.Name, buildAction);

        [DebuggerHidden]
        public static IActivityBuilder AddSendEventAction<TEvent, TSendEventAction>(this IActivityBuilder builder, string actionNodeName, SendEventActionBuildAction buildAction = null)
            where TEvent : Event, new()
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
        public static IActivityBuilder AddStructuredActivity<TStructuredActivity>(this IActivityBuilder builder, ReactiveStructuredActivityBuildAction buildAction)
            where TStructuredActivity : class, IBaseStructuredActivityNode
            => AddStructuredActivity<TStructuredActivity>(builder, ActivityNode<TStructuredActivity>.Name, buildAction);

        [DebuggerHidden]
        public static IActivityBuilder AddStructuredActivity<TStructuredActivity>(this IActivityBuilder builder, string structuredActivityName, ReactiveStructuredActivityBuildAction buildAction = null)
            where TStructuredActivity : class, IBaseStructuredActivityNode
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
        public static IActivityBuilder AddParallelActivity<TParallelizationToken, TStructuredActivity>(this IActivityBuilder builder, ParallelActivityBuildAction buildAction = null, int chunkSize = 1)
            where TStructuredActivity : class, IBaseStructuredActivityNode
            => AddParallelActivity<TParallelizationToken, TStructuredActivity>(builder, ActivityNode<TStructuredActivity>.Name, buildAction, chunkSize);

        [DebuggerHidden]
        public static IActivityBuilder AddParallelActivity<TParallelizationToken, TParallelActivity>(this IActivityBuilder builder, string structuredActivityName, ParallelActivityBuildAction buildAction = null, int chunkSize = 1)
            where TParallelActivity : class, IBaseStructuredActivityNode
        {
            (builder as IInternal).Services.AddServiceType<TParallelActivity>();

            return builder.AddParallelActivity<TParallelizationToken>(
                structuredActivityName,
                b =>
                {
                    var builder = b as StructuredActivityBuilder;
                    builder.AddStructuredActivityEvents<TParallelActivity>();
                    builder.Node.ScanForDeclaredTypes(typeof(TParallelActivity));
                    buildAction?.Invoke(b);
                },
                chunkSize
            );
        }
        #endregion

        #region AddIterativeActivity
        [DebuggerHidden]
        public static IActivityBuilder AddIterativeActivity<TIterationToken, TStructuredActivity>(this IActivityBuilder builder, IterativeActivityBuildAction buildAction = null, int chunkSize = 1)
            where TStructuredActivity : class, IBaseStructuredActivityNode
            => AddIterativeActivity<TIterationToken, TStructuredActivity>(builder, ActivityNode<TStructuredActivity>.Name, buildAction, chunkSize);

        [DebuggerHidden]
        public static IActivityBuilder AddIterativeActivity<TIterationToken, TStructuredActivity>(this IActivityBuilder builder, string structuredActivityName, IterativeActivityBuildAction buildAction = null, int chunkSize = 1)
            where TStructuredActivity : class, IBaseStructuredActivityNode
        {
            (builder as IInternal).Services.AddServiceType<TStructuredActivity>();
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
