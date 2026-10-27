using System.Diagnostics;
using System.Threading.Tasks;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Registration.Builders;

namespace Stateflows.Activities.Registration.Interfaces.Base
{
    /// <summary>Adds activity control and data nodes such as joins, forks, decisions, and data stores.</summary>
    /// <typeparam name="TReturn">Fluent builder type returned after this Activity element is configured.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-nodes" />
    public interface IActivitySpecials<out TReturn>
        where TReturn : class
    {
        #region AddJoin
        /// <summary>Adds a join node that synchronizes its incoming flows.</summary>
        /// <param name="joinNodeName">Registered name of this node in its containing Activity scope.</param>
        /// <param name="buildAction">Callback configuring the join's outgoing flows. Its builder is scoped to the node or
        /// flow, not the enclosing Activity.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-nodes" />
        [DebuggerHidden]
        public TReturn AddJoin(string joinNodeName, JoinBuildAction buildAction)
            => ((BaseActivityBuilder)this)
                .AddNode(
                    NodeType.Join,
                    joinNodeName,
                    c =>
                    {
                        c.PassAllTokensOn();
                        return Task.CompletedTask;
                    },
                    b => buildAction(b)
                ) as TReturn;

        /// <summary>Adds a join node that synchronizes its incoming flows.</summary>
        /// <param name="buildAction">Callback configuring the join's outgoing flows. Its builder is scoped to the node or
        /// flow, not the enclosing Activity.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-nodes" />
        [DebuggerHidden]
        public TReturn AddJoin(JoinBuildAction buildAction)
            => AddJoin(JoinNode.Name, buildAction);
        #endregion
        
        #region AddFork
        /// <summary>Adds a fork node that activates its eligible outgoing flows.</summary>
        /// <param name="forkNodeName">Registered name of this node in its containing Activity scope.</param>
        /// <param name="buildAction">Callback configuring the fork's outgoing flows. Its builder is scoped to the node or
        /// flow, not the enclosing Activity.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-nodes" />
        [DebuggerHidden]
        public TReturn AddFork(string forkNodeName, ForkBuildAction buildAction)
            => ((BaseActivityBuilder)this)
                .AddNode(
                    NodeType.Fork,
                    forkNodeName,
                    c =>
                    {
                        c.PassAllTokensOn();
                        return Task.CompletedTask;
                    },
                    b => buildAction(b)
                ) as TReturn;

        /// <summary>Adds a fork node that activates its eligible outgoing flows.</summary>
        /// <param name="buildAction">Callback configuring the fork's outgoing flows. Its builder is scoped to the node or
        /// flow, not the enclosing Activity.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-nodes" />
        [DebuggerHidden]
        public TReturn AddFork(ForkBuildAction buildAction)
            => AddFork(ForkNode.Name, buildAction);
        #endregion
        
        #region AddMerge
        /// <summary>Adds a merge node that forwards activation from alternative incoming flows.</summary>
        /// <param name="mergeNodeName">Registered name of this node in its containing Activity scope.</param>
        /// <param name="buildAction">Callback configuring the merge node's outgoing flows. Its builder is scoped to the
        /// node or flow, not the enclosing Activity.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-nodes" />
        [DebuggerHidden]
        public TReturn AddMerge(string mergeNodeName, MergeBuildAction buildAction)
            => ((BaseActivityBuilder)this)
                .AddNode(
                    NodeType.Merge,
                    mergeNodeName,
                    c =>
                    {
                        c.PassAllTokensOn();
                        return Task.CompletedTask;
                    },
                    b => buildAction(b.SetOptions(NodeOptions.None) as IMergeBuilder)
                ) as TReturn;

        /// <summary>Adds a merge node that forwards activation from alternative incoming flows.</summary>
        /// <param name="buildAction">Callback configuring the merge node's outgoing flows. Its builder is scoped to the
        /// node or flow, not the enclosing Activity.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-nodes" />
        [DebuggerHidden]
        public TReturn AddMerge(MergeBuildAction buildAction)
            => AddMerge(MergeNode.Name, buildAction);
        #endregion
        
        #region AddControlDecision
        /// <summary>Adds a control decision node that selects an eligible outgoing control flow.</summary>
        /// <param name="decisionNodeName">Registered name of this node in its containing Activity scope.</param>
        /// <param name="buildAction">Callback configuring the decision's outgoing guarded and fallback flows. Its builder
        /// is scoped to the node or flow, not the enclosing Activity.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-nodes" />
        [DebuggerHidden]
        public TReturn AddControlDecision(string decisionNodeName, DecisionBuildAction buildAction)
            => ((BaseActivityBuilder)this)
                .AddNode(
                    NodeType.Decision,
                    decisionNodeName,
                    c =>
                    {
                        c.PassAllTokensOn();
                        return Task.CompletedTask;
                    },
                    b => buildAction(b.SetOptions(NodeOptions.DecisionDefault) as IDecisionBuilder),
                    typeof(ControlToken)
                ) as TReturn;

        /// <summary>Adds a control decision node that selects an eligible outgoing control flow.</summary>
        /// <param name="buildAction">Callback configuring the decision's outgoing guarded and fallback flows. Its builder
        /// is scoped to the node or flow, not the enclosing Activity.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-nodes" />
        [DebuggerHidden]
        public TReturn AddControlDecision(DecisionBuildAction buildAction)
            => AddControlDecision(ControlDecisionNode.Name, buildAction);
        #endregion
        
        #region AddDecision
        /// <summary>Adds a decision node that selects an eligible outgoing object flow for the specified token type.
        /// </summary>
        /// <typeparam name="TToken">Token type carried by the object flow.</typeparam>
        /// <param name="decisionNodeName">Registered name of this node in its containing Activity scope.</param>
        /// <param name="decisionBuildAction">Callback configuring the decision's outgoing guarded and fallback flows. Its
        /// builder is scoped to the node or flow, not the enclosing Activity.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-nodes" />
        [DebuggerHidden]
        public TReturn AddDecision<TToken>(string decisionNodeName, DecisionBuildAction<TToken> decisionBuildAction)
            => ((BaseActivityBuilder)this)
                .AddNode(
                    NodeType.Decision,
                    decisionNodeName,
                    c =>
                    {
                        c.PassAllTokensOn();
                        return Task.CompletedTask;
                    },
                    b => decisionBuildAction(new DecisionBuilder<TToken>(b.SetOptions(NodeOptions.DecisionDefault) as NodeBuilder)),
                    typeof(TToken)
                ) as TReturn;

        /// <summary>Adds a decision node that selects an eligible outgoing object flow for the specified token type.
        /// </summary>
        /// <typeparam name="TToken">Token type carried by the object flow.</typeparam>
        /// <param name="buildAction">Callback configuring the decision's outgoing guarded and fallback flows. Its builder
        /// is scoped to the node or flow, not the enclosing Activity.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-nodes" />
        [DebuggerHidden]
        public TReturn AddDecision<TToken>(DecisionBuildAction<TToken> buildAction)
            => AddDecision(DecisionNode<TToken>.Name, buildAction);
        #endregion
        
        #region AddDataStore
        /// <summary>Adds a data-store node that retains tokens for later graph activations.</summary>
        /// <param name="dataStoreNodeName">Registered name of this node in its containing Activity scope.</param>
        /// <param name="buildAction">Callback configuring the data store's incoming and outgoing flows. Its builder is
        /// scoped to the node or flow, not the enclosing Activity.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-nodes" />
        [DebuggerHidden]
        public TReturn AddDataStore(string dataStoreNodeName, DataStoreBuildAction buildAction)
            => ((BaseActivityBuilder)this)
                .AddNode(
                    NodeType.DataStore,
                    dataStoreNodeName,
                    c =>
                    {
                        c.PassAllTokensOn();
                        return Task.CompletedTask;
                    },
                    b => buildAction(b.SetOptions(NodeOptions.DataStoreDefault) as IDataStoreBuilder)
                ) as TReturn;

        /// <summary>Adds a data-store node that retains tokens for later graph activations.</summary>
        /// <param name="buildAction">Callback configuring the data store's incoming and outgoing flows. Its builder is
        /// scoped to the node or flow, not the enclosing Activity.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-nodes" />
        [DebuggerHidden]
        public TReturn AddDataStore(DataStoreBuildAction buildAction)
            => AddDataStore(DataStoreNode.Name, buildAction);
        #endregion
    }
}
