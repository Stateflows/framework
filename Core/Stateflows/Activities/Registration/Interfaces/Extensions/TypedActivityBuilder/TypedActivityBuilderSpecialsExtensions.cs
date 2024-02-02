using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Activities.Registration;
using Stateflows.Activities.Registration.Builders;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities
{
    public static class TypedActivityBuilderSpecialsExtensions
    {
        public static ITypedActivityBuilder AddJoin(this ITypedActivityBuilder builder, string joinNodeName, JoinBuildAction joinBuildAction)
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
                ) as ITypedActivityBuilder;

        public static ITypedActivityBuilder AddFork(this ITypedActivityBuilder builder, string forkNodeName, ForkBuildAction forkBuildAction)
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
                ) as ITypedActivityBuilder;

        public static ITypedActivityBuilder AddMerge(this ITypedActivityBuilder builder, string mergeNodeName, MergeBuildAction mergeBuildAction)
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
                ) as ITypedActivityBuilder;

        public static ITypedActivityBuilder AddControlDecision(this ITypedActivityBuilder builder, string decisionNodeName, DecisionBuildAction decisionBuildAction)
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
                ) as ITypedActivityBuilder;

        public static ITypedActivityBuilder AddTokenDecision<TToken>(this ITypedActivityBuilder builder, string decisionNodeName, DecisionBuildAction<TToken> decisionBuildAction)
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
                ) as ITypedActivityBuilder;

        public static ITypedActivityBuilder AddDataStore(this ITypedActivityBuilder builder, string dataStoreNodeName, DataStoreBuildAction decisionBuildAction)
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
                ) as ITypedActivityBuilder;

        public static ITypedActivityBuilder AddTimeEventAction<TTimeEvent>(this ITypedActivityBuilder builder, string actionNodeName, ActionDelegateAsync actionAsync, AcceptEventActionBuildAction buildAction)
            where TTimeEvent : TimeEvent, new()
            => builder.AddAcceptEventAction<TTimeEvent>(actionNodeName, c => actionAsync(c), buildAction);
    }
}
