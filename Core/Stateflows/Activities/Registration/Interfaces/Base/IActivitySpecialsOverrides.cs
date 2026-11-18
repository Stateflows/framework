using System.Collections.Generic;
using System.Diagnostics;
using Stateflows.Activities.Registration.Builders;
using Stateflows.Common;
using Stateflows.Payloads;

namespace Stateflows.Activities.Registration.Interfaces.Base
{
    /// <summary>Overrides initial, input, join, fork, merge, decision, and data-store elements.</summary>
    /// <typeparam name="TReturn">Fluent builder type returned after this Activity element is configured.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.1.-Nodes" />
    public interface IActivitySpecialsOverrides<out TReturn>
    {
        /// <summary>Overrides the inherited initial node and its outgoing flow.</summary>
        /// <param name="buildAction">Callback configuring the selected inherited element's replacement behavior. Its
        /// builder is scoped to that element.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.1.-Nodes" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
        TReturn UseInitial(OverridenInitialBuildAction buildAction);
        
        /// <summary>Overrides the inherited input pin for the specified token type and its outgoing flows.</summary>
        /// <param name="buildAction">Callback configuring the selected inherited element's replacement behavior. Its
        /// builder is scoped to that element.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.1.-Nodes" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
        TReturn UseInputPin<TToken>(System.Action<IOverridenInputPinBuilder<TToken>> buildAction);

        /// <summary>Overrides the input pin for payload tokens with the specified names.</summary>
        /// <param name="tokenNames">Payload token names admitted by the pin.</param>
        /// <param name="buildAction">Callback configuring the pin's outgoing flows.</param>
        TReturn UseInputPin(IReadOnlyList<string> tokenNames, System.Action<IOverridenInputPinBuilder<Payload>> buildAction)
            => PayloadFlowScope.With(tokenNames, nameof(tokenNames), () => UseInputPin<Payload>(buildAction));

        /// <summary>Overrides the input pin for payload tokens with the specified name.</summary>
        /// <param name="tokenName">Payload token name admitted by the pin.</param>
        /// <param name="buildAction">Callback configuring the pin's outgoing flows.</param>
        TReturn UseInputPin(string tokenName, System.Action<IOverridenInputPinBuilder<Payload>> buildAction)
            => UseInputPin(new[] { tokenName }, buildAction);

        #region AddJoin
        /// <summary>Overrides a named join node and its outgoing flows.</summary>
        /// <param name="joinNodeName">Registered name of this node in its containing Activity scope.</param>
        /// <param name="buildAction">Callback configuring the selected inherited element's replacement behavior. Its
        /// builder is scoped to that element.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.1.-Nodes" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
        [DebuggerHidden]
        public TReturn UseJoin(string joinNodeName, OverridenJoinBuildAction buildAction);

        /// <summary>Overrides the default-named join node.</summary>
        /// <param name="buildAction">Callback configuring the selected inherited element's replacement behavior. Its
        /// builder is scoped to that element.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.1.-Nodes" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
        [DebuggerHidden]
        public TReturn UseJoin(OverridenJoinBuildAction buildAction)
            => UseJoin(JoinNode.Name, buildAction);
        #endregion
        
        /// <summary>Overrides a named fork node and its outgoing flows.</summary>
        /// <param name="forkNodeName">Registered name of this node in its containing Activity scope.</param>
        /// <param name="buildAction">Callback configuring the selected inherited element's replacement behavior. Its
        /// builder is scoped to that element.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.1.-Nodes" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
        public TReturn UseFork(string forkNodeName, OverridenForkBuildAction buildAction);

        /// <summary>Overrides the default-named fork node.</summary>
        /// <param name="buildAction">Callback configuring the selected inherited element's replacement behavior. Its
        /// builder is scoped to that element.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.1.-Nodes" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
        [DebuggerHidden]
        public TReturn UseFork(OverridenForkBuildAction buildAction)
            => UseFork(ForkNode.Name, buildAction);
        
        #region AddMerge
        /// <summary>Overrides a named merge node and its outgoing flows.</summary>
        /// <param name="mergeNodeName">Registered name of this node in its containing Activity scope.</param>
        /// <param name="buildAction">Callback configuring the selected inherited element's replacement behavior. Its
        /// builder is scoped to that element.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.1.-Nodes" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
        [DebuggerHidden]
        public TReturn UseMerge(string mergeNodeName, OverridenMergeBuildAction buildAction);

        /// <summary>Overrides the default-named merge node.</summary>
        /// <param name="buildAction">Callback configuring the selected inherited element's replacement behavior. Its
        /// builder is scoped to that element.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.1.-Nodes" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
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
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.1.-Nodes" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
        [DebuggerHidden]
        public TReturn UseControlDecision(string decisionNodeName, OverridenDecisionBuildAction buildAction);

        /// <summary>Selects and replaces the matching inherited flow from the current node.</summary>
        /// <param name="buildAction">Callback configuring the selected inherited element's replacement behavior. Its
        /// builder is scoped to that element.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.1.-Nodes" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
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
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.1.-Nodes" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
        [DebuggerHidden]
        public TReturn UseDecision<TToken>(string decisionNodeName, OverridenDecisionBuildAction<TToken> decisionBuildAction);

        /// <summary>Overrides the default-named typed decision node.</summary>
        /// <typeparam name="TToken">Token type carried by the object flow.</typeparam>
        /// <param name="buildAction">Callback configuring the selected inherited element's replacement behavior. Its
        /// builder is scoped to that element.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.1.-Nodes" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
        [DebuggerHidden]
        public TReturn UseDecision<TToken>(OverridenDecisionBuildAction<TToken> buildAction)
            => UseDecision(DecisionNode<TToken>.Name, buildAction);

        /// <summary>Overrides a decision for payload tokens with the specified names.</summary>
        /// <param name="tokenNames">Payload token names accepted by the decision.</param>
        /// <param name="decisionNodeName">Registered name of the decision node.</param>
        /// <param name="buildAction">Callback configuring the decision's outgoing flows.</param>
        TReturn UseDecision(IReadOnlyList<string> tokenNames, string decisionNodeName, OverridenDecisionBuildAction<Payload> buildAction)
            => PayloadFlowScope.With(tokenNames, nameof(tokenNames), () => UseDecision<Payload>(decisionNodeName, buildAction));

        /// <summary>Overrides the default-named decision for payload tokens with the specified names.</summary>
        /// <param name="tokenNames">Payload token names accepted by the decision.</param>
        /// <param name="buildAction">Callback configuring the decision's outgoing flows.</param>
        TReturn UseDecision(IReadOnlyList<string> tokenNames, OverridenDecisionBuildAction<Payload> buildAction)
            => UseDecision(tokenNames, DecisionNode.GetName(tokenNames), buildAction);

        /// <summary>Overrides a decision for payload tokens with the specified name.</summary>
        /// <param name="tokenName">Payload token name accepted by the decision.</param>
        /// <param name="decisionNodeName">Registered name of the decision node.</param>
        /// <param name="buildAction">Callback configuring the decision's outgoing flows.</param>
        TReturn UseDecision(string tokenName, string decisionNodeName, OverridenDecisionBuildAction<Payload> buildAction)
            => UseDecision(new[] { tokenName }, decisionNodeName, buildAction);

        /// <summary>Overrides the default-named decision for payload tokens with the specified name.</summary>
        /// <param name="tokenName">Payload token name accepted by the decision.</param>
        /// <param name="buildAction">Callback configuring the decision's outgoing flows.</param>
        TReturn UseDecision(string tokenName, OverridenDecisionBuildAction<Payload> buildAction)
            => UseDecision(new[] { tokenName }, buildAction);
        #endregion
        
        #region AddDataStore
        /// <summary>Overrides a named data-store node and its typed object flows.</summary>
        /// <param name="dataStoreNodeName">Registered name of this node in its containing Activity scope.</param>
        /// <param name="buildAction">Callback configuring the selected inherited element's replacement behavior. Its
        /// builder is scoped to that element.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.1.-Nodes" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
        [DebuggerHidden]
        public TReturn UseDataStore(string dataStoreNodeName, OverridenDataStoreBuildAction buildAction);

        /// <summary>Overrides the default-named data-store node.</summary>
        /// <param name="buildAction">Callback configuring the selected inherited element's replacement behavior. Its
        /// builder is scoped to that element.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.1.-Nodes" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
        [DebuggerHidden]
        public TReturn UseDataStore(OverridenDataStoreBuildAction buildAction)
            => UseDataStore(DataStoreNode.Name, buildAction);
        #endregion
    }
}
