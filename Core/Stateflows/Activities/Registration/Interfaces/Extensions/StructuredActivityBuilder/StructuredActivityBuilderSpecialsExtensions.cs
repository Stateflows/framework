using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Activities.Registration;
using Stateflows.Activities.Registration.Builders;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities
{
    public static class StructuredActivityBuilderSpecialsExtensions
    {
        public static IStructuredActivityBuilder AddJoin(this IStructuredActivityBuilder builder, string joinNodeName, JoinBuildAction joinBuildAction)
            => (builder as BaseActivityBuilder)
                .AddNode(
                    NodeType.Join,
                    joinNodeName,
                    c =>
                    {
                        c.PassAllOn();
                        return Task.CompletedTask;
                    },
                    b => joinBuildAction(b)
                ) as IStructuredActivityBuilder;

        public static IStructuredActivityBuilder AddFork(this IStructuredActivityBuilder builder, string forkNodeName, ForkBuildAction forkBuildAction)
            => (builder as BaseActivityBuilder)
                .AddNode(
                    NodeType.Fork,
                    forkNodeName,
                    c =>
                    {
                        c.PassAllOn();
                        return Task.CompletedTask;
                    },
                    b => forkBuildAction(b)
                ) as IStructuredActivityBuilder;

        public static IStructuredActivityBuilder AddMerge(this IStructuredActivityBuilder builder, string mergeNodeName, MergeBuildAction mergeBuildAction)
            => (builder as BaseActivityBuilder)
                .AddNode(
                    NodeType.Merge,
                    mergeNodeName,
                    c =>
                    {
                        c.PassAllOn();
                        return Task.CompletedTask;
                    },
                    b => mergeBuildAction(b.SetOptions(NodeOptions.None) as IMergeBuilder)
                ) as IStructuredActivityBuilder;

        public static IStructuredActivityBuilder AddControlDecision(this IStructuredActivityBuilder builder, string decisionNodeName, DecisionBuildAction decisionBuildAction)
            => (builder as BaseActivityBuilder)
                .AddNode(
                    NodeType.Decision,
                    decisionNodeName,
                    c =>
                    {
                        c.PassAllOn();
                        return Task.CompletedTask;
                    },
                    b => decisionBuildAction(b.SetOptions(NodeOptions.DecisionDefault) as IDecisionBuilder)
                ) as IStructuredActivityBuilder;

        public static IStructuredActivityBuilder AddTokenDecision<TToken>(this IStructuredActivityBuilder builder, string decisionNodeName, DecisionBuildAction<TToken> decisionBuildAction)
            where TToken : Token, new()
            => (builder as BaseActivityBuilder)
                .AddNode(
                    NodeType.Decision,
                    decisionNodeName,
                    c =>
                    {
                        c.PassAllOn();
                        return Task.CompletedTask;
                    },
                    b => decisionBuildAction(new DecisionBuilder<TToken>(b.SetOptions(NodeOptions.DecisionDefault) as NodeBuilder))
                ) as IStructuredActivityBuilder;

        public static IStructuredActivityBuilder AddDataStore(this IStructuredActivityBuilder builder, string dataStoreNodeName, DataStoreBuildAction decisionBuildAction)
            => (builder as BaseActivityBuilder)
                .AddNode(
                    NodeType.DataStore,
                    dataStoreNodeName,
                    c =>
                    {
                        c.PassAllOn();
                        return Task.CompletedTask;
                    },
                    b => decisionBuildAction(b.SetOptions(NodeOptions.DataStoreDefault) as IDataStoreBuilder)
                ) as IStructuredActivityBuilder;
    }
}
