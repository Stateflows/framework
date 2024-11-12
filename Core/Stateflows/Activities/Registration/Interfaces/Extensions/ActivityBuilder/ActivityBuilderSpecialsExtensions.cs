using System.Diagnostics;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Activities.Registration;
using Stateflows.Activities.Registration.Builders;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities
{
    public static class ActivityBuilderSpecialsExtensions
    {
        [DebuggerHidden]
        public static IActivityBuilder AddJoin(this IActivityBuilder builder, string joinNodeName, JoinBuildAction buildAction)
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
                ) as IActivityBuilder;

        [DebuggerHidden]
        public static IActivityBuilder AddJoin(this IActivityBuilder builder, JoinBuildAction buildAction)
            => builder.AddJoin(JoinNode.Name, buildAction);

        [DebuggerHidden]
        public static IActivityBuilder AddFork(this IActivityBuilder builder, string forkNodeName, ForkBuildAction buildAction)
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
                ) as IActivityBuilder;

        [DebuggerHidden]
        public static IActivityBuilder AddFork(this IActivityBuilder builder, ForkBuildAction buildAction)
            => builder.AddFork(ForkNode.Name, buildAction);

        [DebuggerHidden]
        public static IActivityBuilder AddMerge(this IActivityBuilder builder, string mergeNodeName, MergeBuildAction buildAction)
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
                ) as IActivityBuilder;

        [DebuggerHidden]
        public static IActivityBuilder AddMerge(this IActivityBuilder builder, MergeBuildAction buildAction)
            => builder.AddMerge(MergeNode.Name, buildAction);

        [DebuggerHidden]
        public static IActivityBuilder AddControlDecision(this IActivityBuilder builder, string decisionNodeName, DecisionBuildAction buildAction)
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
                ) as IActivityBuilder;

        [DebuggerHidden]
        public static IActivityBuilder AddControlDecision(this IActivityBuilder builder, DecisionBuildAction buildAction)
            => builder.AddControlDecision(ControlDecisionNode.Name, buildAction);

        [DebuggerHidden]
        public static IActivityBuilder AddDecision<TToken>(this IActivityBuilder builder, string decisionNodeName, DecisionBuildAction<TToken> decisionBuildAction)
            => (builder as BaseActivityBuilder)
                .AddNode(
                    NodeType.Decision,
                    decisionNodeName,
                    c =>
                    {
                        c.PassAllTokensOn();
                        return Task.CompletedTask;
                    },
                    b => decisionBuildAction(new DecisionBuilder<TToken>(b.SetOptions(NodeOptions.DecisionDefault) as NodeBuilder))
                ) as IActivityBuilder;

        [DebuggerHidden]
        public static IActivityBuilder AddDecision<TToken>(this IActivityBuilder builder, DecisionBuildAction<TToken> buildAction)
            => builder.AddDecision(DecisionNode<TToken>.Name, buildAction);

        [DebuggerHidden]
        public static IActivityBuilder AddDataStore(this IActivityBuilder builder, string dataStoreNodeName, DataStoreBuildAction buildAction)
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
                ) as IActivityBuilder;

        [DebuggerHidden]
        public static IActivityBuilder AddDataStore(this IActivityBuilder builder, DataStoreBuildAction buildAction)
            => builder.AddDataStore(DataStoreNode.Name, buildAction);

        #region AddAcceptEventAction
        [DebuggerHidden]
        public static IActivityBuilder AddAcceptEventAction<TEvent>(this IActivityBuilder builder, string actionNodeName, AcceptEventActionBuildAction buildAction)
            => builder.AddAcceptEventAction<TEvent>(actionNodeName, c => Task.CompletedTask, buildAction);

        [DebuggerHidden]
        public static IActivityBuilder AddAcceptEventAction<TEvent>(this IActivityBuilder builder, AcceptEventActionBuildAction buildAction)
            => builder.AddAcceptEventAction<TEvent>(AcceptEventActionNode<TEvent>.Name, c => Task.CompletedTask, buildAction);

        [DebuggerHidden]
        public static IActivityBuilder AddAcceptEventAction<TEvent>(this IActivityBuilder builder, AcceptEventActionDelegateAsync<TEvent> actionAsync, AcceptEventActionBuildAction buildAction = null)
            => builder.AddAcceptEventAction<TEvent>(AcceptEventActionNode<TEvent>.Name, actionAsync, buildAction);
        #endregion

        #region AddTimeEventAction
        [DebuggerHidden]
        public static IActivityBuilder AddTimeEventAction<TTimeEvent>(this IActivityBuilder builder, string actionNodeName, AcceptEventActionBuildAction buildAction)
            where TTimeEvent : TimeEvent, new()
            => builder.AddTimeEventAction<TTimeEvent>(actionNodeName, c => Task.CompletedTask, buildAction);

        [DebuggerHidden]
        public static IActivityBuilder AddTimeEventAction<TTimeEvent>(this IActivityBuilder builder, string actionNodeName, ActionDelegateAsync actionAsync, AcceptEventActionBuildAction buildAction = null)
            where TTimeEvent : TimeEvent, new()
            => builder.AddTimeEventAction<TTimeEvent>(actionNodeName, c => actionAsync(c), buildAction);

        [DebuggerHidden]
        public static IActivityBuilder AddTimeEventAction<TTimeEvent>(this IActivityBuilder builder, ActionDelegateAsync actionAsync, AcceptEventActionBuildAction buildAction = null)
            where TTimeEvent : TimeEvent, new()
            => builder.AddTimeEventAction<TTimeEvent>(TimeEventActionNode<TTimeEvent>.Name, c => actionAsync(c), buildAction);

        [DebuggerHidden]
        public static IActivityBuilder AddTimeEventAction<TTimeEvent>(this IActivityBuilder builder, AcceptEventActionBuildAction buildAction)
            where TTimeEvent : TimeEvent, new()
            => builder.AddTimeEventAction<TTimeEvent>(TimeEventActionNode<TTimeEvent>.Name, c => Task.CompletedTask, buildAction);
        #endregion
    }
}
