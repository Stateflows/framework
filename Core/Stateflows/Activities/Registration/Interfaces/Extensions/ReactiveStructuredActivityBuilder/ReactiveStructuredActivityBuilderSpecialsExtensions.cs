using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Activities.Registration;
using Stateflows.Activities.Registration.Builders;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities
{
    public static class ReactiveStructuredActivityBuilderSpecialsExtensions
    {
        public static IReactiveStructuredActivityBuilder AddJoin(this IReactiveStructuredActivityBuilder builder, string joinNodeName, JoinBuilderAction buildAction)
            => (builder as BaseActivityBuilder)
                .AddNode(
                    NodeType.Join,
                    joinNodeName,
                    c =>
                    {
                        c.PassAllOn();
                        return Task.CompletedTask;
                    },
                    b => buildAction(b)
                ) as IReactiveStructuredActivityBuilder;

        public static IReactiveStructuredActivityBuilder AddFork(this IReactiveStructuredActivityBuilder builder, string forkNodeName, ForkBuilderAction buildAction)
            => (builder as BaseActivityBuilder)
                .AddNode(
                    NodeType.Fork,
                    forkNodeName,
                    c =>
                    {
                        c.PassAllOn();
                        return Task.CompletedTask;
                    },
                    b => buildAction(b)
                ) as IReactiveStructuredActivityBuilder;

        public static IReactiveStructuredActivityBuilder AddMerge(this IReactiveStructuredActivityBuilder builder, string mergeNodeName, MergeBuilderAction buildAction)
            => (builder as BaseActivityBuilder)
                .AddNode(
                    NodeType.Merge,
                    mergeNodeName,
                    c =>
                    {
                        c.PassAllOn();
                        return Task.CompletedTask;
                    },
                    b => buildAction(b.SetOptions(NodeOptions.None) as IMergeBuilder)
                ) as IReactiveStructuredActivityBuilder;

        public static IReactiveStructuredActivityBuilder AddControlDecision(this IReactiveStructuredActivityBuilder builder, string decisionNodeName, DecisionBuilderAction buildAction)
            => (builder as BaseActivityBuilder)
                .AddNode(
                    NodeType.Decision,
                    decisionNodeName,
                    c =>
                    {
                        c.PassAllOn();
                        return Task.CompletedTask;
                    },
                    b => buildAction(b.SetOptions(NodeOptions.DecisionDefault) as IDecisionBuilder)
                ) as IReactiveStructuredActivityBuilder;

        public static IReactiveStructuredActivityBuilder AddTokenDecision<TToken>(this IReactiveStructuredActivityBuilder builder, string decisionNodeName, DecisionBuilderAction<TToken> buildAction)
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
                    b => buildAction(new DecisionBuilder<TToken>(b.SetOptions(NodeOptions.DecisionDefault) as NodeBuilder))
                ) as IReactiveStructuredActivityBuilder;

        public static IReactiveStructuredActivityBuilder AddDataStore(this IReactiveStructuredActivityBuilder builder, string dataStoreNodeName, DataStoreBuilderAction buildAction)
            => (builder as BaseActivityBuilder)
                .AddNode(
                    NodeType.DataStore,
                    dataStoreNodeName,
                    c =>
                    {
                        c.PassAllOn();
                        return Task.CompletedTask;
                    },
                    b => buildAction(b.SetOptions(NodeOptions.DataStoreDefault) as IDataStoreBuilder)
                ) as IReactiveStructuredActivityBuilder;

        public static IReactiveStructuredActivityBuilder AddTimeEventAction<TTimeEvent>(this IReactiveStructuredActivityBuilder builder, string actionNodeName, ActionDelegateAsync actionAsync, AcceptEventActionBuilderAction buildAction)
            where TTimeEvent : TimeEvent, new()
            => builder.AddAcceptEventAction<TTimeEvent>(actionNodeName, c => actionAsync(c), buildAction);
    }
}
