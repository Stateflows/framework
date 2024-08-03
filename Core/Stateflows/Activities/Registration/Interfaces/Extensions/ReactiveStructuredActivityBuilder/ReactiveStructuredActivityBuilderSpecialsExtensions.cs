using System.Diagnostics;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Activities.Registration;
using Stateflows.Activities.Registration.Builders;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities
{
    public static class ReactiveStructuredActivityBuilderSpecialsExtensions
    {
        [DebuggerHidden]
        public static IReactiveStructuredActivityBuilder AddJoin(this IReactiveStructuredActivityBuilder builder, string joinNodeName, JoinBuildAction buildAction)
            => (builder as BaseActivityBuilder)
                .AddNode(
                    NodeType.Join,
                    joinNodeName,
                    c =>
                    {
                        c.PassAllTokensOn();
                        return Task.CompletedTask;
                    },
                    b => buildAction(b)
                ) as IReactiveStructuredActivityBuilder;

        [DebuggerHidden]
        public static IReactiveStructuredActivityBuilder AddJoin(this IReactiveStructuredActivityBuilder builder, JoinBuildAction joinBuildAction)
            => builder.AddJoin(JoinNode.Name, joinBuildAction);

        [DebuggerHidden]
        public static IReactiveStructuredActivityBuilder AddFork(this IReactiveStructuredActivityBuilder builder, string forkNodeName, ForkBuildAction buildAction)
            => (builder as BaseActivityBuilder)
                .AddNode(
                    NodeType.Fork,
                    forkNodeName,
                    c =>
                    {
                        c.PassAllTokensOn();
                        return Task.CompletedTask;
                    },
                    b => buildAction(b)
                ) as IReactiveStructuredActivityBuilder;

        [DebuggerHidden]
        public static IReactiveStructuredActivityBuilder AddFork(this IReactiveStructuredActivityBuilder builder, ForkBuildAction forkBuildAction)
            => builder.AddFork(ForkNode.Name, forkBuildAction);

        [DebuggerHidden]
        public static IReactiveStructuredActivityBuilder AddMerge(this IReactiveStructuredActivityBuilder builder, string mergeNodeName, MergeBuildAction buildAction)
            => (builder as BaseActivityBuilder)
                .AddNode(
                    NodeType.Merge,
                    mergeNodeName,
                    c =>
                    {
                        c.PassAllTokensOn();
                        return Task.CompletedTask;
                    },
                    b => buildAction(b.SetOptions(NodeOptions.None) as IMergeBuilder)
                ) as IReactiveStructuredActivityBuilder;

        [DebuggerHidden]
        public static IReactiveStructuredActivityBuilder AddMerge(this IReactiveStructuredActivityBuilder builder, MergeBuildAction mergeBuildAction)
            => builder.AddMerge(MergeNode.Name, mergeBuildAction);

        [DebuggerHidden]
        public static IReactiveStructuredActivityBuilder AddControlDecision(this IReactiveStructuredActivityBuilder builder, string decisionNodeName, DecisionBuildAction buildAction)
            => (builder as BaseActivityBuilder)
                .AddNode(
                    NodeType.Decision,
                    decisionNodeName,
                    c =>
                    {
                        c.PassAllTokensOn();
                        return Task.CompletedTask;
                    },
                    b => buildAction(b.SetOptions(NodeOptions.DecisionDefault) as IDecisionBuilder)
                ) as IReactiveStructuredActivityBuilder;

        [DebuggerHidden]
        public static IReactiveStructuredActivityBuilder AddControlDecision(this IReactiveStructuredActivityBuilder builder, DecisionBuildAction decisionBuildAction)
            => builder.AddControlDecision(DecisionNode<ControlToken>.Name, decisionBuildAction);

        [DebuggerHidden]
        public static IReactiveStructuredActivityBuilder AddDecision<TToken>(this IReactiveStructuredActivityBuilder builder, string decisionNodeName, DecisionBuildAction<TToken> buildAction)
            => (builder as BaseActivityBuilder)
                .AddNode(
                    NodeType.Decision,
                    decisionNodeName,
                    c =>
                    {
                        c.PassAllTokensOn();
                        return Task.CompletedTask;
                    },
                    b => buildAction(new DecisionBuilder<TToken>(b.SetOptions(NodeOptions.DecisionDefault) as NodeBuilder))
                ) as IReactiveStructuredActivityBuilder;

        [DebuggerHidden]
        public static IReactiveStructuredActivityBuilder AddDecision<TToken>(this IReactiveStructuredActivityBuilder builder, DecisionBuildAction<TToken> decisionBuildAction)
            => builder.AddDecision(DecisionNode<TToken>.Name, decisionBuildAction);

        [DebuggerHidden]
        public static IReactiveStructuredActivityBuilder AddDataStore(this IReactiveStructuredActivityBuilder builder, string dataStoreNodeName, DataStoreBuildAction buildAction)
            => (builder as BaseActivityBuilder)
                .AddNode(
                    NodeType.DataStore,
                    dataStoreNodeName,
                    c =>
                    {
                        c.PassAllTokensOn();
                        return Task.CompletedTask;
                    },
                    b => buildAction(b.SetOptions(NodeOptions.DataStoreDefault) as IDataStoreBuilder)
                ) as IReactiveStructuredActivityBuilder;

        [DebuggerHidden]
        public static IReactiveStructuredActivityBuilder AddDataStore(this IReactiveStructuredActivityBuilder builder, DataStoreBuildAction buildAction)
            => builder.AddDataStore(DataStoreNode.Name, buildAction);

        #region AddAcceptEventAction
        [DebuggerHidden]
        public static IReactiveStructuredActivityBuilder AddAcceptEventAction<TEvent>(this IReactiveStructuredActivityBuilder builder, string actionNodeName, AcceptEventActionBuildAction buildAction)
            where TEvent : Event, new()
            => builder.AddAcceptEventAction<TEvent>(actionNodeName, c => Task.CompletedTask, buildAction);

        [DebuggerHidden]
        public static IReactiveStructuredActivityBuilder AddAcceptEventAction<TEvent>(this IReactiveStructuredActivityBuilder builder, AcceptEventActionBuildAction buildAction)
            where TEvent : Event, new()
            => builder.AddAcceptEventAction<TEvent>(AcceptEventActionNode<TEvent>.Name, c => Task.CompletedTask, buildAction);

        [DebuggerHidden]
        public static IReactiveStructuredActivityBuilder AddAcceptEventAction<TEvent>(this IReactiveStructuredActivityBuilder builder, AcceptEventActionDelegateAsync<TEvent> actionAsync, AcceptEventActionBuildAction buildAction = null)
            where TEvent : Event, new()
            => builder.AddAcceptEventAction<TEvent>(AcceptEventActionNode<TEvent>.Name, c => actionAsync(c), buildAction);
        #endregion

        #region AddTimeEventAction
        [DebuggerHidden]
        public static IReactiveStructuredActivityBuilder AddTimeEventAction<TTimeEvent>(this IReactiveStructuredActivityBuilder builder, string actionNodeName, AcceptEventActionBuildAction buildAction)
            where TTimeEvent : TimeEvent, new()
            => builder.AddTimeEventAction<TTimeEvent>(actionNodeName, c => Task.CompletedTask, buildAction);

        [DebuggerHidden]
        public static IReactiveStructuredActivityBuilder AddTimeEventAction<TTimeEvent>(this IReactiveStructuredActivityBuilder builder, string actionNodeName, ActionDelegateAsync actionAsync, AcceptEventActionBuildAction buildAction = null)
            where TTimeEvent : TimeEvent, new()
            => builder.AddTimeEventAction<TTimeEvent>(actionNodeName, c => actionAsync(c), buildAction);

        [DebuggerHidden]
        public static IReactiveStructuredActivityBuilder AddTimeEventAction<TTimeEvent>(this IReactiveStructuredActivityBuilder builder, ActionDelegateAsync actionAsync, AcceptEventActionBuildAction buildAction = null)
            where TTimeEvent : TimeEvent, new()
            => builder.AddTimeEventAction<TTimeEvent>(TimeEventActionNode<TTimeEvent>.Name, c => actionAsync(c), buildAction);

        [DebuggerHidden]
        public static IReactiveStructuredActivityBuilder AddTimeEventAction<TTimeEvent>(this IReactiveStructuredActivityBuilder builder, AcceptEventActionBuildAction buildAction)
            where TTimeEvent : TimeEvent, new()
            => builder.AddTimeEventAction<TTimeEvent>(TimeEventActionNode<TTimeEvent>.Name, c => Task.CompletedTask, buildAction);
        #endregion
    }
}
