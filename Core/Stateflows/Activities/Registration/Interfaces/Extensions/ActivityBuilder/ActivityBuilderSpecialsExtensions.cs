using System.Threading.Tasks;
using Stateflows.Activities.Registration.Builders;
using Stateflows.Activities.Registration;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Common;

namespace Stateflows.Activities
{
    public static class ActivityBuilderSpecialsExtensions
    {
        public static IActivityBuilder AddJoin(this IActivityBuilder builder, string joinNodeName, JoinBuilderAction joinBuildAction)
            => (builder as BaseActivityBuilder)
                .AddNode(
                    NodeType.Join,
                    joinNodeName,
                    c =>
                    {
                        c.PassAllTokens();
                        return Task.CompletedTask;
                    },
                    b => joinBuildAction(b)
                ) as IActivityBuilder;

        public static IActivityBuilder AddFork(this IActivityBuilder builder, string forkNodeName, ForkBuilderAction forkBuildAction)
            => (builder as BaseActivityBuilder)
                .AddNode(
                    NodeType.Fork,
                    forkNodeName,
                    c =>
                    {
                        c.PassAllTokens();
                        return Task.CompletedTask;
                    },
                    b => forkBuildAction(b)
                ) as IActivityBuilder;

        public static IActivityBuilder AddMerge(this IActivityBuilder builder, string mergeNodeName, MergeBuilderAction mergeBuildAction)
            => (builder as BaseActivityBuilder)
                .AddNode(
                    NodeType.Merge,
                    mergeNodeName,
                    c =>
                    {
                        c.PassAllTokens();
                        return Task.CompletedTask;
                    },
                    b => mergeBuildAction(b.SetOptions(NodeOptions.None) as IMergeBuilder)
                ) as IActivityBuilder;

        public static IActivityBuilder AddControlDecision(this IActivityBuilder builder, string decisionNodeName, DecisionBuilderAction decisionBuildAction)
            => (builder as BaseActivityBuilder)
                .AddNode(
                    NodeType.Decision,
                    decisionNodeName,
                    c =>
                    {
                        c.PassAllTokens();
                        return Task.CompletedTask;
                    },
                    b => decisionBuildAction(b.SetOptions(NodeOptions.DecisionDefault) as IDecisionBuilder)
                ) as IActivityBuilder;

        public static IActivityBuilder AddObjectDecision<TToken>(this IActivityBuilder builder, string decisionNodeName, DecisionBuilderAction<TToken> decisionBuildAction)
            where TToken : Token, new()
            => (builder as BaseActivityBuilder)
                .AddNode(
                    NodeType.Decision,
                    decisionNodeName,
                    c =>
                    {
                        c.PassAllTokens();
                        return Task.CompletedTask;
                    },
                    b => decisionBuildAction(new DecisionBuilder<TToken>(b.SetOptions(NodeOptions.DecisionDefault) as NodeBuilder))
                ) as IActivityBuilder;

        public static IActivityBuilder AddDataStore(this IActivityBuilder builder, string dataStoreNodeName, DataStoreBuilderAction decisionBuildAction)
            => (builder as BaseActivityBuilder)
                .AddNode(
                    NodeType.DataStore,
                    dataStoreNodeName,
                    c =>
                    {
                        c.PassAllTokens();
                        return Task.CompletedTask;
                    },
                    b => decisionBuildAction(b.SetOptions(NodeOptions.DataStoreDefault) as IDataStoreBuilder)
                ) as IActivityBuilder;

        public static IActivityBuilder AddTimeEventAction<TTimeEvent>(this IActivityBuilder builder, string actionNodeName, ActionDelegateAsync actionAsync, AcceptEventActionBuilderAction buildAction)
            where TTimeEvent : TimeEvent, new()
            => builder.AddAcceptEventAction<TTimeEvent>(actionNodeName, c => actionAsync(c), buildAction);
    }
}
