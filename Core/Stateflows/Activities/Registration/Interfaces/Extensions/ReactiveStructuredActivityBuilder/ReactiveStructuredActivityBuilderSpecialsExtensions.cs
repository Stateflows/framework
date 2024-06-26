﻿using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Activities.Registration;
using Stateflows.Activities.Registration.Builders;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities
{
    public static class ReactiveStructuredActivityBuilderSpecialsExtensions
    {
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

        public static IReactiveStructuredActivityBuilder AddJoin(this IReactiveStructuredActivityBuilder builder, JoinBuildAction joinBuildAction)
            => builder.AddJoin(ActivityNodeInfo<JoinNode>.Name, joinBuildAction);

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

        public static IReactiveStructuredActivityBuilder AddFork(this IReactiveStructuredActivityBuilder builder, ForkBuildAction forkBuildAction)
            => builder.AddFork(ActivityNodeInfo<ForkNode>.Name, forkBuildAction);

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

        public static IReactiveStructuredActivityBuilder AddMerge(this IReactiveStructuredActivityBuilder builder, MergeBuildAction mergeBuildAction)
            => builder.AddMerge(ActivityNodeInfo<MergeNode>.Name, mergeBuildAction);

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

        public static IReactiveStructuredActivityBuilder AddControlDecision(this IReactiveStructuredActivityBuilder builder, DecisionBuildAction decisionBuildAction)
            => builder.AddControlDecision(ActivityNodeInfo<DecisionNode<Control>>.Name, decisionBuildAction);

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

        public static IReactiveStructuredActivityBuilder AddDecision<TToken>(this IReactiveStructuredActivityBuilder builder, DecisionBuildAction<TToken> decisionBuildAction)
            => builder.AddDecision(ActivityNodeInfo<DecisionNode<TToken>>.Name, decisionBuildAction);

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

        #region AddAcceptEventAction
        public static IReactiveStructuredActivityBuilder AddAcceptEventAction<TEvent>(this IReactiveStructuredActivityBuilder builder, string actionNodeName, AcceptEventActionBuildAction buildAction)
            where TEvent : Event, new()
            => builder.AddAcceptEventAction<TEvent>(actionNodeName, c => Task.CompletedTask, buildAction);

        public static IReactiveStructuredActivityBuilder AddAcceptEventAction<TEvent>(this IReactiveStructuredActivityBuilder builder, AcceptEventActionBuildAction buildAction)
            where TEvent : Event, new()
            => builder.AddAcceptEventAction<TEvent>(ActivityNodeInfo<AcceptEventActionNode<TEvent>>.Name, c => Task.CompletedTask, buildAction);

        public static IReactiveStructuredActivityBuilder AddAcceptEventAction<TEvent>(this IReactiveStructuredActivityBuilder builder, ActionDelegateAsync actionAsync, AcceptEventActionBuildAction buildAction = null)
            where TEvent : Event, new()
            => builder.AddAcceptEventAction<TEvent>(ActivityNodeInfo<AcceptEventActionNode<TEvent>>.Name, c => actionAsync(c), buildAction);
        #endregion

        #region AddTimeEventAction
        public static IReactiveStructuredActivityBuilder AddTimeEventAction<TTimeEvent>(this IReactiveStructuredActivityBuilder builder, string actionNodeName, AcceptEventActionBuildAction buildAction)
            where TTimeEvent : TimeEvent, new()
            => builder.AddAcceptEventAction<TTimeEvent>(actionNodeName, c => Task.CompletedTask, buildAction);

        public static IReactiveStructuredActivityBuilder AddTimeEventAction<TTimeEvent>(this IReactiveStructuredActivityBuilder builder, string actionNodeName, ActionDelegateAsync actionAsync, AcceptEventActionBuildAction buildAction = null)
            where TTimeEvent : TimeEvent, new()
            => builder.AddAcceptEventAction<TTimeEvent>(actionNodeName, c => actionAsync(c), buildAction);

        public static IReactiveStructuredActivityBuilder AddTimeEventAction<TTimeEvent>(this IReactiveStructuredActivityBuilder builder, ActionDelegateAsync actionAsync, AcceptEventActionBuildAction buildAction = null)
            where TTimeEvent : TimeEvent, new()
            => builder.AddAcceptEventAction<TTimeEvent>(ActivityNodeInfo<AcceptEventActionNode<TTimeEvent>>.Name, c => actionAsync(c), buildAction);

        public static IReactiveStructuredActivityBuilder AddTimeEventAction<TTimeEvent>(this IReactiveStructuredActivityBuilder builder, AcceptEventActionBuildAction buildAction)
            where TTimeEvent : TimeEvent, new()
            => builder.AddAcceptEventAction<TTimeEvent>(ActivityNodeInfo<AcceptEventActionNode<TTimeEvent>>.Name, c => Task.CompletedTask, buildAction);
        #endregion
    }
}
