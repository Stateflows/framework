﻿using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Activities.Registration;
using Stateflows.Activities.Registration.Builders;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.StateMachines.Engine;

namespace Stateflows.Activities
{
    public static class ActivityBuilderSpecialsExtensions
    {
        public static IActivityBuilder AddJoin(this IActivityBuilder builder, string joinNodeName, JoinBuildAction joinBuildAction)
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
                ) as IActivityBuilder;

        public static IActivityBuilder AddFork(this IActivityBuilder builder, string forkNodeName, ForkBuildAction forkBuildAction)
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
                ) as IActivityBuilder;

        public static IActivityBuilder AddMerge(this IActivityBuilder builder, string mergeNodeName, MergeBuildAction mergeBuildAction)
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
                ) as IActivityBuilder;

        public static IActivityBuilder AddControlDecision(this IActivityBuilder builder, string decisionNodeName, DecisionBuildAction decisionBuildAction)
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
                ) as IActivityBuilder;

        public static IActivityBuilder AddTokenDecision<TToken>(this IActivityBuilder builder, string decisionNodeName, DecisionBuildAction<TToken> decisionBuildAction)
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
                ) as IActivityBuilder;

        public static IActivityBuilder AddDataStore(this IActivityBuilder builder, string dataStoreNodeName, DataStoreBuildAction decisionBuildAction)
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
                ) as IActivityBuilder;

        #region AddAcceptEventAction
        public static IActivityBuilder AddAcceptEventAction<TEvent>(this IActivityBuilder builder, string actionNodeName, AcceptEventActionBuildAction buildAction)
            where TEvent : Event, new()
            => builder.AddAcceptEventAction<TEvent>(actionNodeName, c => Task.CompletedTask, buildAction);

        public static IActivityBuilder AddAcceptEventAction<TEvent>(this IActivityBuilder builder, AcceptEventActionBuildAction buildAction)
            where TEvent : Event, new()
            => builder.AddAcceptEventAction<TEvent>(ActivityNodeInfo<AcceptEventActionNode<TEvent>>.Name, c => Task.CompletedTask, buildAction);

        public static IActivityBuilder AddAcceptEventAction<TEvent>(this IActivityBuilder builder, ActionDelegateAsync actionAsync, AcceptEventActionBuildAction buildAction = null)
            where TEvent : Event, new()
            => builder.AddAcceptEventAction<TEvent>(ActivityNodeInfo<AcceptEventActionNode<TEvent>>.Name, c => actionAsync(c), buildAction);
        #endregion

        #region AddTimeEventAction
        public static IActivityBuilder AddTimeEventAction<TTimeEvent>(this IActivityBuilder builder, string actionNodeName, AcceptEventActionBuildAction buildAction)
            where TTimeEvent : TimeEvent, new()
            => builder.AddAcceptEventAction<TTimeEvent>(actionNodeName, c => Task.CompletedTask, buildAction);

        public static IActivityBuilder AddTimeEventAction<TTimeEvent>(this IActivityBuilder builder, string actionNodeName, ActionDelegateAsync actionAsync, AcceptEventActionBuildAction buildAction = null)
            where TTimeEvent : TimeEvent, new()
            => builder.AddAcceptEventAction<TTimeEvent>(actionNodeName, c => actionAsync(c), buildAction);

        public static IActivityBuilder AddTimeEventAction<TTimeEvent>(this IActivityBuilder builder, ActionDelegateAsync actionAsync, AcceptEventActionBuildAction buildAction = null)
            where TTimeEvent : TimeEvent, new()
            => builder.AddAcceptEventAction<TTimeEvent>(ActivityNodeInfo<AcceptEventActionNode<TTimeEvent>>.Name, c => actionAsync(c), buildAction);

        public static IActivityBuilder AddTimeEventAction<TTimeEvent>(this IActivityBuilder builder, AcceptEventActionBuildAction buildAction)
            where TTimeEvent : TimeEvent, new()
            => builder.AddAcceptEventAction<TTimeEvent>(ActivityNodeInfo<AcceptEventActionNode<TTimeEvent>>.Name, c => Task.CompletedTask, buildAction);
        #endregion
    }
}
