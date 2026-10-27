using System.Diagnostics;

namespace Stateflows.Activities.Registration.Interfaces.Base
{
    /// <summary>Overrides initial, input, join, fork, merge, decision, and data-store elements.</summary>
    /// <typeparam name="TReturn">Fluent builder type returned after this Activity element is configured.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-nodes" />
    public interface IActivitySpecialsOverrides<out TReturn>
    {
        /// <summary>Overrides the inherited initial node and its outgoing flow.</summary>
        /// <param name="buildAction">Callback configuring the selected inherited element's replacement behavior. Its
        /// builder is scoped to that element.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-nodes" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        TReturn UseInitial(OverridenInitialBuildAction buildAction);
        
        /// <summary>Overrides the inherited input node and its outgoing flows.</summary>
        /// <param name="buildAction">Callback configuring the selected inherited element's replacement behavior. Its
        /// builder is scoped to that element.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-nodes" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        TReturn UseInput(OverridenInputBuildAction buildAction);

        #region AddJoin
        /// <summary>Overrides a named join node and its outgoing flows.</summary>
        /// <param name="joinNodeName">Registered name of this node in its containing Activity scope.</param>
        /// <param name="buildAction">Callback configuring the selected inherited element's replacement behavior. Its
        /// builder is scoped to that element.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-nodes" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        [DebuggerHidden]
        public TReturn UseJoin(string joinNodeName, OverridenJoinBuildAction buildAction);

        /// <summary>Overrides the default-named join node.</summary>
        /// <param name="buildAction">Callback configuring the selected inherited element's replacement behavior. Its
        /// builder is scoped to that element.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-nodes" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        [DebuggerHidden]
        public TReturn UseJoin(OverridenJoinBuildAction buildAction)
            => UseJoin(JoinNode.Name, buildAction);
        #endregion
        
        /// <summary>Overrides a named fork node and its outgoing flows.</summary>
        /// <param name="forkNodeName">Registered name of this node in its containing Activity scope.</param>
        /// <param name="buildAction">Callback configuring the selected inherited element's replacement behavior. Its
        /// builder is scoped to that element.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-nodes" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        public TReturn UseFork(string forkNodeName, OverridenForkBuildAction buildAction);

        /// <summary>Overrides the default-named fork node.</summary>
        /// <param name="buildAction">Callback configuring the selected inherited element's replacement behavior. Its
        /// builder is scoped to that element.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-nodes" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        [DebuggerHidden]
        public TReturn UseFork(OverridenForkBuildAction buildAction)
            => UseFork(ForkNode.Name, buildAction);
        
        #region AddMerge
        /// <summary>Overrides a named merge node and its outgoing flows.</summary>
        /// <param name="mergeNodeName">Registered name of this node in its containing Activity scope.</param>
        /// <param name="buildAction">Callback configuring the selected inherited element's replacement behavior. Its
        /// builder is scoped to that element.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-nodes" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        [DebuggerHidden]
        public TReturn UseMerge(string mergeNodeName, OverridenMergeBuildAction buildAction);

        /// <summary>Overrides the default-named merge node.</summary>
        /// <param name="buildAction">Callback configuring the selected inherited element's replacement behavior. Its
        /// builder is scoped to that element.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-nodes" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        [DebuggerHidden]
        public TReturn UseMerge(OverridenMergeBuildAction buildAction)
            => UseMerge(MergeNode.Name, buildAction);
        #endregion
        
        #region AddControlDecision
        /// <summary>Selects and replaces the matching inherited flow from the current node.</summary>
        /// <param name="decisionNodeName">Registered name of this node in its containing Activity scope.</param>
        /// <param name="buildAction">Callback configuring the selected inherited element's replacement behavior. Its
        /// builder is scoped to that element.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-nodes" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        [DebuggerHidden]
        public TReturn UseControlDecision(string decisionNodeName, OverridenDecisionBuildAction buildAction);

        /// <summary>Selects and replaces the matching inherited flow from the current node.</summary>
        /// <param name="buildAction">Callback configuring the selected inherited element's replacement behavior. Its
        /// builder is scoped to that element.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-nodes" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        [DebuggerHidden]
        public TReturn UseControlDecision(OverridenDecisionBuildAction buildAction)
            => UseControlDecision(ControlDecisionNode.Name, buildAction);
        #endregion
        
        #region AddDecision
        /// <summary>Overrides a named typed decision node and its object flows.</summary>
        /// <typeparam name="TToken">Token type carried by the object flow.</typeparam>
        /// <param name="decisionNodeName">Registered name of this node in its containing Activity scope.</param>
        /// <param name="decisionBuildAction">Callback configuring the selected inherited element's replacement behavior.
        /// Its builder is scoped to that element.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-nodes" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        [DebuggerHidden]
        public TReturn UseDecision<TToken>(string decisionNodeName, OverridenDecisionBuildAction<TToken> decisionBuildAction);

        /// <summary>Overrides the default-named typed decision node.</summary>
        /// <typeparam name="TToken">Token type carried by the object flow.</typeparam>
        /// <param name="buildAction">Callback configuring the selected inherited element's replacement behavior. Its
        /// builder is scoped to that element.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-nodes" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        [DebuggerHidden]
        public TReturn UseDecision<TToken>(OverridenDecisionBuildAction<TToken> buildAction)
            => UseDecision(DecisionNode<TToken>.Name, buildAction);
        #endregion
        
        #region AddDataStore
        /// <summary>Overrides a named data-store node and its typed object flows.</summary>
        /// <param name="dataStoreNodeName">Registered name of this node in its containing Activity scope.</param>
        /// <param name="buildAction">Callback configuring the selected inherited element's replacement behavior. Its
        /// builder is scoped to that element.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-nodes" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        [DebuggerHidden]
        public TReturn UseDataStore(string dataStoreNodeName, OverridenDataStoreBuildAction buildAction);

        /// <summary>Overrides the default-named data-store node.</summary>
        /// <param name="buildAction">Callback configuring the selected inherited element's replacement behavior. Its
        /// builder is scoped to that element.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-nodes" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        [DebuggerHidden]
        public TReturn UseDataStore(OverridenDataStoreBuildAction buildAction)
            => UseDataStore(DataStoreNode.Name, buildAction);
        #endregion
    }
}
