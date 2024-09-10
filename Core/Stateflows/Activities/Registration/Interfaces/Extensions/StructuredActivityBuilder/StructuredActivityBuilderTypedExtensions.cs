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
    public static class StructuredActivityBuilderTypedExtensions
    {
        #region AddAction
        [DebuggerHidden]
        public static IStructuredActivityBuilder AddAction<TAction>(this IStructuredActivityBuilder builder, TypedActionBuildAction buildAction = null)
            where TAction : class, IActionNode
            => AddAction<TAction>(builder, ActivityNode<TAction>.Name, buildAction);

        [DebuggerHidden]
        public static IStructuredActivityBuilder AddAction<TAction>(this IStructuredActivityBuilder builder, string actionNodeName, TypedActionBuildAction buildAction = null)
            where TAction : class, IActionNode
        {
            (builder as IInternal).Services.AddServiceType<TAction>();

            return builder.AddAction(
                actionNodeName,
                (c =>
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
        public static IStructuredActivityBuilder AddSendEventAction<TEvent, TSendEventAction>(this IStructuredActivityBuilder builder, SendEventActionBuildAction buildAction = null)
            where TSendEventAction : class, ISendEventActionNode<TEvent>
            => builder.AddSendEventAction<TEvent, TSendEventAction>(ActivityNode<TSendEventAction>.Name, buildAction);

        [DebuggerHidden]
        public static IStructuredActivityBuilder AddSendEventAction<TEvent, TSendEventAction>(this IStructuredActivityBuilder builder, string actionNodeName, SendEventActionBuildAction buildAction = null)
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
        public static IStructuredActivityBuilder AddStructuredActivity<TStructuredActivity>(this IStructuredActivityBuilder builder, StructuredActivityBuildAction buildAction = null)
            where TStructuredActivity : class, IStructuredActivityNode
            => AddStructuredActivity<TStructuredActivity>(builder, ActivityNode<TStructuredActivity>.Name, buildAction);

        [DebuggerHidden]
        public static IStructuredActivityBuilder AddStructuredActivity<TStructuredActivity>(this IStructuredActivityBuilder builder, string structuredActivityName, StructuredActivityBuildAction buildAction = null)
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
        public static IStructuredActivityBuilder AddParallelActivity<TParallelizationToken, TStructuredActivity>(this IStructuredActivityBuilder builder, ParallelActivityBuildAction buildAction = null, int chunkSize = 1)
            where TStructuredActivity : class, IStructuredActivityNode
            => AddParallelActivity<TParallelizationToken, TStructuredActivity>(builder, ActivityNode<TStructuredActivity>.Name, buildAction, chunkSize);

        [DebuggerHidden]
        public static IStructuredActivityBuilder AddParallelActivity<TParallelizationToken, TStructuredActivity>(this IStructuredActivityBuilder builder, string structuredActivityName, ParallelActivityBuildAction buildAction = null, int chunkSize = 1)
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
        public static IStructuredActivityBuilder AddIterativeActivity<TIterationToken, TStructuredActivity>(this IStructuredActivityBuilder builder, IterativeActivityBuildAction buildAction = null, int chunkSize = 1)
            where TStructuredActivity : class, IStructuredActivityNode
            => AddIterativeActivity<TIterationToken, TStructuredActivity>(builder, ActivityNode<TStructuredActivity>.Name, buildAction, chunkSize);

        [DebuggerHidden]
        public static IStructuredActivityBuilder AddIterativeActivity<TIterationToken, TStructuredActivity>(this IStructuredActivityBuilder builder, string structuredActivityName, IterativeActivityBuildAction buildAction = null, int chunkSize = 1)
            where TStructuredActivity : class, IStructuredActivityNode
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
