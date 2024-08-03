using System.Diagnostics;
using System.Threading.Tasks;
using Stateflows.Activities.Registration;
using Stateflows.Activities.Registration.Builders;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities
{
    public static class StructuredActivityBuilderSpecialsExtensions
    {
        [DebuggerHidden]
        public static IStructuredActivityBuilder AddJoin(this IStructuredActivityBuilder builder, string joinNodeName, JoinBuildAction joinBuildAction)
            => (builder as BaseActivityBuilder)
                .AddNode(
                    NodeType.Join,
                    joinNodeName,
                    c =>
                    {
                        c.PassAllTokensOn();
                        return Task.CompletedTask;
                    },
                    b => joinBuildAction(b)
                ) as IStructuredActivityBuilder;

        [DebuggerHidden]
        public static IStructuredActivityBuilder AddJoin(this IStructuredActivityBuilder builder, JoinBuildAction joinBuildAction)
            => builder.AddJoin(ActivityNode<JoinNode>.Name, joinBuildAction);

        [DebuggerHidden]
        public static IStructuredActivityBuilder AddFork(this IStructuredActivityBuilder builder, string forkNodeName, ForkBuildAction forkBuildAction)
            => (builder as BaseActivityBuilder)
                .AddNode(
                    NodeType.Fork,
                    forkNodeName,
                    c =>
                    {
                        c.PassAllTokensOn();
                        return Task.CompletedTask;
                    },
                    b => forkBuildAction(b)
                ) as IStructuredActivityBuilder;

        [DebuggerHidden]
        public static IStructuredActivityBuilder AddFork(this IStructuredActivityBuilder builder, ForkBuildAction forkBuildAction)
            => builder.AddFork(ActivityNode<ForkNode>.Name, forkBuildAction);

        [DebuggerHidden]
        public static IStructuredActivityBuilder AddMerge(this IStructuredActivityBuilder builder, string mergeNodeName, MergeBuildAction mergeBuildAction)
            => (builder as BaseActivityBuilder)
                .AddNode(
                    NodeType.Merge,
                    mergeNodeName,
                    c =>
                    {
                        c.PassAllTokensOn();
                        return Task.CompletedTask;
                    },
                    b => mergeBuildAction(b.SetOptions(NodeOptions.None) as IMergeBuilder)
                ) as IStructuredActivityBuilder;

        [DebuggerHidden]
        public static IStructuredActivityBuilder AddMerge(this IStructuredActivityBuilder builder, MergeBuildAction mergeBuildAction)
            => builder.AddMerge(ActivityNode<MergeNode>.Name, mergeBuildAction);

        [DebuggerHidden]
        public static IStructuredActivityBuilder AddControlDecision(this IStructuredActivityBuilder builder, string decisionNodeName, DecisionBuildAction decisionBuildAction)
            => (builder as BaseActivityBuilder)
                .AddNode(
                    NodeType.Decision,
                    decisionNodeName,
                    c =>
                    {
                        c.PassAllTokensOn();
                        return Task.CompletedTask;
                    },
                    b => decisionBuildAction(b.SetOptions(NodeOptions.DecisionDefault) as IDecisionBuilder)
                ) as IStructuredActivityBuilder;

        [DebuggerHidden]
        public static IStructuredActivityBuilder AddControlDecision(this IStructuredActivityBuilder builder, DecisionBuildAction decisionBuildAction)
            => builder.AddControlDecision(ActivityNode<DecisionNode<ControlToken>>.Name, decisionBuildAction);

        [DebuggerHidden]
        public static IStructuredActivityBuilder AddDecision<TToken>(this IStructuredActivityBuilder builder, string decisionNodeName, DecisionBuildAction<TToken> decisionBuildAction)
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
                ) as IStructuredActivityBuilder;

        [DebuggerHidden]
        public static IStructuredActivityBuilder AddDecision<TToken>(this IStructuredActivityBuilder builder, DecisionBuildAction<TToken> decisionBuildAction)
            => builder.AddDecision(ActivityNode<DecisionNode<TToken>>.Name, decisionBuildAction);

        [DebuggerHidden]
        public static IStructuredActivityBuilder AddDataStore(this IStructuredActivityBuilder builder, string dataStoreNodeName, DataStoreBuildAction decisionBuildAction)
            => (builder as BaseActivityBuilder)
                .AddNode(
                    NodeType.DataStore,
                    dataStoreNodeName,
                    c =>
                    {
                        c.PassAllTokensOn();
                        return Task.CompletedTask;
                    },
                    b => decisionBuildAction(b.SetOptions(NodeOptions.DataStoreDefault) as IDataStoreBuilder)
                ) as IStructuredActivityBuilder;

        [DebuggerHidden]
        public static IStructuredActivityBuilder AddDataStore(this IStructuredActivityBuilder builder, DataStoreBuildAction buildAction)
            => builder.AddDataStore(DataStoreNode.Name, buildAction);
    }
}
