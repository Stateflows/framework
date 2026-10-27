using System.Diagnostics;
using Stateflows.Activities.Extensions;

namespace Stateflows.Activities.Registration.Interfaces.Base
{
    /// <summary>Adds typed object flows from the current node.</summary>
    /// <typeparam name="TReturn">Fluent builder type returned after this Activity element is configured.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-data-flow" />
    public interface IObjectFlowBase<out TReturn>
    {
        /// <summary>Adds a typed object flow from the current node to the named target.</summary>
        /// <typeparam name="TToken">Token type carried by the object flow.</typeparam>
        /// <param name="targetNodeName">Registered name of the destination node in the containing Activity scope.</param>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-data-flow" />
        TReturn AddFlow<TToken>(string targetNodeName, ObjectFlowBuildAction<TToken> buildAction = null);

        /// <summary>Adds a typed object flow from the current node to the named target.</summary>
        /// <typeparam name="TToken">Token type carried by the object flow.</typeparam>
        /// <typeparam name="TTargetNode">Destination node implementation type; its registered node name selects the flow
        /// target.</typeparam>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-data-flow" />
        TReturn AddFlow<TToken, TTargetNode>(ObjectFlowBuildAction<TToken> buildAction = null)
            where TTargetNode : class, IActivityNode
            => AddFlow(ActivityNode<TTargetNode>.Name, buildAction);

        /// <summary>Adds a typed object flow from the current node to the named target.</summary>
        /// <typeparam name="TToken">Token type carried by the object flow.</typeparam>
        /// <typeparam name="TFlow">Typed flow implementation implementing <see cref="IFlow&lt;TToken&gt;" />.</typeparam>
        /// <param name="targetNodeName">Registered name of the destination node in the containing Activity scope.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-data-flow" />
        TReturn AddFlow<TToken, TFlow>(string targetNodeName)
            where TFlow : class, IFlow<TToken>
            => AddFlow<TToken>(
                targetNodeName,
                b => b.AddObjectFlowEvents<TFlow, TToken>()
            );

        /// <summary>Adds a typed object flow from the current node to the named target.</summary>
        /// <typeparam name="TToken">Token type carried by the object flow.</typeparam>
        /// <typeparam name="TFlow">Typed flow implementation implementing <see cref="IFlow&lt;TToken&gt;" />.</typeparam>
        /// <typeparam name="TTargetNode">Destination node implementation type; its registered node name selects the flow
        /// target.</typeparam>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-data-flow" />
        TReturn AddFlow<TToken, TFlow, TTargetNode>()
            where TFlow : class, IFlow<TToken>
            where TTargetNode : class, IActivityNode
            => AddFlow<TToken, TFlow>(ActivityNode<TTargetNode>.Name);

        /// <summary>Adds a typed object flow from the current node to the named target.</summary>
        /// <typeparam name="TToken">Token type carried by the object flow.</typeparam>
        /// <typeparam name="TTransformedToken">Token type produced by the object-flow transformation.</typeparam>
        /// <typeparam name="TTransformationFlow">Typed flow transformation implementing
        /// <see cref="IFlowTransformation{TToken, TTransformedToken}" />.</typeparam>
        /// <param name="targetNodeName">Registered name of the destination node in the containing Activity scope.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-data-flow" />
        TReturn AddFlow<TToken, TTransformedToken, TTransformationFlow>(string targetNodeName)
            where TTransformationFlow : class, IFlowTransformation<TToken, TTransformedToken>
            => AddFlow<TToken>(
                targetNodeName,
                b => b.AddObjectTransformationFlowEvents<TTransformationFlow, TToken, TTransformedToken>()
            );

        /// <summary>Adds a typed object flow from the current node to the named target.</summary>
        /// <typeparam name="TToken">Token type carried by the object flow.</typeparam>
        /// <typeparam name="TTransformedToken">Token type produced by the object-flow transformation.</typeparam>
        /// <typeparam name="TTransformationFlow">Typed flow transformation implementing
        /// <see cref="IFlowTransformation{TToken, TTransformedToken}" />.</typeparam>
        /// <typeparam name="TTargetNode">Destination node implementation type; its registered node name selects the flow
        /// target.</typeparam>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-data-flow" />
        TReturn AddFlow<TToken, TTransformedToken, TTransformationFlow, TTargetNode>()
            where TTransformationFlow : class, IFlowTransformation<TToken, TTransformedToken>
            where TTargetNode : class, IActivityNode
            => AddFlow<TToken, TTransformedToken, TTransformationFlow>(ActivityNode<TTargetNode>.Name);
    }
    
    /// <summary>Provides fluent APIs for extending elements inherited from a reusable Activity definition.</summary>
    /// <typeparam name="TReturn">Fluent builder type returned after this Activity element is configured.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-data-flow" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    public interface IOverridenObjectFlowBase<out TReturn>
    {
        /// <summary>Selects and replaces the matching inherited flow from the current node.</summary>
        /// <typeparam name="TToken">Token type carried by the object flow.</typeparam>
        /// <param name="targetNodeName">Registered name of the destination node in the containing Activity scope.</param>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-data-flow" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        TReturn UseFlow<TToken>(string targetNodeName, ObjectFlowBuildAction<TToken> buildAction = null);

        /// <summary>Selects and replaces the matching inherited flow from the current node.</summary>
        /// <typeparam name="TToken">Token type carried by the object flow.</typeparam>
        /// <typeparam name="TTargetNode">Destination node implementation type; its registered node name selects the flow
        /// target.</typeparam>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-data-flow" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        TReturn UseFlow<TToken, TTargetNode>(ObjectFlowBuildAction<TToken> buildAction = null)
            where TTargetNode : class, IActivityNode
            => UseFlow(ActivityNode<TTargetNode>.Name, buildAction);
    }

    /// <summary>Adds fallback typed object flows from the current node.</summary>
    /// <typeparam name="TReturn">Fluent builder type returned after this Activity element is configured.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-data-flow" />
    public interface IElseObjectFlowBase<out TReturn>
    {
        /// <summary>Adds a fallback flow used when no ordinary outgoing flow is eligible.</summary>
        /// <typeparam name="TToken">Token type carried by the object flow.</typeparam>
        /// <param name="targetNodeName">Registered name of the destination node in the containing Activity scope.</param>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-data-flow" />
        TReturn AddElseFlow<TToken>(string targetNodeName, ElseObjectFlowBuildAction<TToken> buildAction = null);
    }

    /// <summary>Adds typed object decision flows from a decision node.</summary>
    /// <typeparam name="TToken">Token type carried by the object flow.</typeparam>
    /// <typeparam name="TReturn">Fluent builder type returned after this Activity element is configured.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-data-flow" />
    public interface IDecisionFlowBase<out TToken, out TReturn>
    {
        /// <summary>Adds a guarded object flow from the decision node to a target for its token type.</summary>
        /// <param name="targetNodeName">Registered name of the destination node in the containing Activity scope.</param>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-data-flow" />
        TReturn AddFlow(string targetNodeName, ObjectFlowBuildAction<TToken> buildAction = null);

        /// <summary>Adds a guarded object flow from the decision node to a target for its token type.</summary>
        /// <typeparam name="TTargetNode">Destination node implementation type; its registered node name selects the flow
        /// target.</typeparam>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-data-flow" />
        TReturn AddFlow<TTargetNode>(ObjectFlowBuildAction<TToken> buildAction = null)
            where TTargetNode : class, IActivityNode
            => AddFlow(ActivityNode<TTargetNode>.Name, buildAction);

        /// <summary>Adds a guarded object flow from the decision node to a target for its token type.</summary>
        /// <typeparam name="TFlow">Typed flow implementation implementing <see cref="IFlow&lt;TToken&gt;" />.</typeparam>
        /// <param name="targetNodeName">Registered name of the destination node in the containing Activity scope.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-data-flow" />
        TReturn AddFlow<TFlow>(string targetNodeName)
            where TFlow : class, IFlow<TToken>
            => AddFlow(
                targetNodeName,
                b => b.AddObjectFlowEvents<TFlow, TToken>()
            );

        /// <summary>Adds a guarded object flow from the decision node to a target for its token type.</summary>
        /// <typeparam name="TFlow">Typed flow implementation implementing <see cref="IFlow&lt;TToken&gt;" />.</typeparam>
        /// <typeparam name="TTargetNode">Destination node implementation type; its registered node name selects the flow
        /// target.</typeparam>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-data-flow" />
        TReturn AddFlow<TFlow, TTargetNode>()
            where TFlow : class, IFlow<TToken>
            where TTargetNode : class, IActivityNode
            => AddFlow<TFlow>(ActivityNode<TTargetNode>.Name);

        /// <summary>Adds a guarded object flow from the decision node to a target for its token type.</summary>
        /// <typeparam name="TTransformedToken">Token type produced by the object-flow transformation.</typeparam>
        /// <typeparam name="TTransformationFlow">Typed flow transformation implementing
        /// <see cref="IFlowTransformation{TToken, TTransformedToken}" />.</typeparam>
        /// <param name="targetNodeName">Registered name of the destination node in the containing Activity scope.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-data-flow" />
        TReturn AddFlow<TTransformedToken, TTransformationFlow>(string targetNodeName)
            where TTransformationFlow : class, IFlowTransformation<TToken, TTransformedToken>
            => AddFlow(
                targetNodeName,
                b => b.AddObjectTransformationFlowEvents<TTransformationFlow, TToken, TTransformedToken>()
            );

        /// <summary>Adds a guarded object flow from the decision node to a target for its token type.</summary>
        /// <typeparam name="TTransformedToken">Token type produced by the object-flow transformation.</typeparam>
        /// <typeparam name="TTransformationFlow">Typed flow transformation implementing
        /// <see cref="IFlowTransformation{TToken, TTransformedToken}" />.</typeparam>
        /// <typeparam name="TTargetNode">Destination node implementation type; its registered node name selects the flow
        /// target.</typeparam>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-data-flow" />
        TReturn AddFlow<TTransformedToken, TTransformationFlow, TTargetNode>()
            where TTransformationFlow : class, IFlowTransformation<TToken, TTransformedToken>
            where TTargetNode : class, IActivityNode
            => AddFlow<TTransformedToken, TTransformationFlow>(ActivityNode<TTargetNode>.Name);
    }

    /// <summary>Provides fluent APIs for extending elements inherited from a reusable Activity definition.</summary>
    /// <typeparam name="TToken">Token type carried by the object flow.</typeparam>
    /// <typeparam name="TReturn">Fluent builder type returned after this Activity element is configured.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-data-flow" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    public interface IOverridenDecisionFlowBase<out TToken, out TReturn>
    {
        /// <summary>Selects and replaces the matching inherited flow from the current node.</summary>
        /// <param name="targetNodeName">Registered name of the destination node in the containing Activity scope.</param>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-data-flow" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        TReturn UseFlow(string targetNodeName, ObjectFlowBuildAction<TToken> buildAction = null);

        /// <summary>Selects and replaces the matching inherited flow from the current node.</summary>
        /// <typeparam name="TTargetNode">Destination node implementation type; its registered node name selects the flow
        /// target.</typeparam>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-data-flow" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        TReturn UseFlow<TTargetNode>(ObjectFlowBuildAction<TToken> buildAction = null)
            where TTargetNode : class, IActivityNode
            => UseFlow(ActivityNode<TTargetNode>.Name, buildAction);
    }

    /// <summary>Adds fallback typed object flows from a decision node.</summary>
    /// <typeparam name="TToken">Token type carried by the object flow.</typeparam>
    /// <typeparam name="TReturn">Fluent builder type returned after this Activity element is configured.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-data-flow" />
    public interface IElseDecisionFlowBase<out TToken, out TReturn>
    {
        /// <summary>Adds a fallback flow used when no ordinary outgoing flow is eligible.</summary>
        /// <param name="targetNodeName">Registered name of the destination node in the containing Activity scope.</param>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-data-flow" />
        TReturn AddElseFlow(string targetNodeName, ElseObjectFlowBuildAction<TToken> buildAction = null);

        /// <summary>Adds a fallback flow used when no ordinary outgoing flow is eligible.</summary>
        /// <typeparam name="TTargetNode">Destination node implementation type; its registered node name selects the flow
        /// target.</typeparam>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-data-flow" />
        TReturn AddElseFlow<TTargetNode>(ObjectFlowBuildAction<TToken> buildAction = null)
            where TTargetNode : class, IActivityNode
            => AddElseFlow(ActivityNode<TTargetNode>.Name, b => buildAction?.Invoke(b as IObjectFlowBuilder<TToken>));

        /// <summary>Adds a fallback flow used when no ordinary outgoing flow is eligible.</summary>
        /// <typeparam name="TTransformedToken">Token type produced by the object-flow transformation.</typeparam>
        /// <typeparam name="TElseTransformationFlow">Typed flow transformation implementing
        /// <see cref="IFlowTransformation{TToken, TTransformedToken}" />.</typeparam>
        /// <param name="targetNodeName">Registered name of the destination node in the containing Activity scope.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-data-flow" />
        TReturn AddElseFlow<TTransformedToken, TElseTransformationFlow>(string targetNodeName)
            where TElseTransformationFlow : class, IFlowTransformation<TToken, TTransformedToken>
            => AddElseFlow(
                targetNodeName,
                b => (b as IObjectFlowBuilder<TToken>).AddElseObjectTransformationFlowEvents<TElseTransformationFlow, TToken, TTransformedToken>()
            );

        /// <summary>Adds a fallback flow used when no ordinary outgoing flow is eligible.</summary>
        /// <typeparam name="TTransformedToken">Token type produced by the object-flow transformation.</typeparam>
        /// <typeparam name="TElseTransformationFlow">Typed flow transformation implementing
        /// <see cref="IFlowTransformation{TToken, TTransformedToken}" />.</typeparam>
        /// <typeparam name="TTargetNode">Destination node implementation type; its registered node name selects the flow
        /// target.</typeparam>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-data-flow" />
        TReturn AddElseFlow<TTransformedToken, TElseTransformationFlow, TTargetNode>()
            where TElseTransformationFlow : class, IFlowTransformation<TToken, TTransformedToken>
            where TTargetNode : class, IActivityNode
            => AddElseFlow<TTransformedToken, TElseTransformationFlow>(ActivityNode<TTargetNode>.Name);
    }

    /// <summary>Provides fluent APIs for extending elements inherited from a reusable Activity definition.</summary>
    /// <typeparam name="TToken">Token type carried by the object flow.</typeparam>
    /// <typeparam name="TReturn">Fluent builder type returned after this Activity element is configured.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-data-flow" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    public interface IOverridenElseDecisionFlowBase<out TToken, out TReturn>
    {
        /// <summary>Selects and replaces the matching inherited flow from the current node.</summary>
        /// <param name="targetNodeName">Registered name of the destination node in the containing Activity scope.</param>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-data-flow" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        TReturn UseElseFlow(string targetNodeName, ElseObjectFlowBuildAction<TToken> buildAction = null);

        /// <summary>Selects and replaces the matching inherited flow from the current node.</summary>
        /// <typeparam name="TTargetNode">Destination node implementation type; its registered node name selects the flow
        /// target.</typeparam>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-data-flow" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        TReturn UseElseFlow<TTargetNode>(ObjectFlowBuildAction<TToken> buildAction = null)
            where TTargetNode : class, IActivityNode
            => UseElseFlow(ActivityNode<TTargetNode>.Name, b => buildAction?.Invoke(b as IObjectFlowBuilder<TToken>));
    }

    /// <summary>Adds typed object flows where the builder does not return a fluent result.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-data-flow" />
    public interface IObjectFlowBase
    {
        /// <summary>Adds a typed object flow from the current node to the named target.</summary>
        /// <typeparam name="TToken">Token type carried by the object flow.</typeparam>
        /// <param name="targetNodeName">Registered name of the destination node in the containing Activity scope.</param>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-data-flow" />
        void AddFlow<TToken>(string targetNodeName, ObjectFlowBuildAction<TToken> buildAction = null);
        
        /// <summary>Adds a typed object flow from the current node to the named target.</summary>
        /// <typeparam name="TToken">Token type carried by the object flow.</typeparam>
        /// <typeparam name="TTargetNode">Destination node implementation type; its registered node name selects the flow
        /// target.</typeparam>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-data-flow" />
        [DebuggerHidden]
        public void AddFlow<TToken, TTargetNode>(ObjectFlowBuildAction<TToken> buildAction = null)
            where TTargetNode : class, IActivityNode
            => AddFlow<TToken>(ActivityNode<TTargetNode>.Name, buildAction);

        /// <summary>Adds a typed object flow from the current node to the named target.</summary>
        /// <typeparam name="TToken">Token type carried by the object flow.</typeparam>
        /// <typeparam name="TFlow">Typed flow implementation implementing <see cref="IFlow&lt;TToken&gt;" />.</typeparam>
        /// <param name="targetNodeName">Registered name of the destination node in the containing Activity scope.</param>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-data-flow" />
        [DebuggerHidden]
        public void AddFlow<TToken, TFlow>(string targetNodeName, ObjectFlowBuildAction<TToken> buildAction = null)
            where TFlow : class, IFlow<TToken>
            => AddFlow<TToken>(
                targetNodeName,
                b =>
                {
                    b.AddObjectFlowEvents<TFlow, TToken>();
                    buildAction?.Invoke(b);
                }
            );

        /// <summary>Adds a typed object flow from the current node to the named target.</summary>
        /// <typeparam name="TToken">Token type carried by the object flow.</typeparam>
        /// <typeparam name="TFlow">Typed flow implementation implementing <see cref="IFlow&lt;TToken&gt;" />.</typeparam>
        /// <typeparam name="TTargetNode">Destination node implementation type; its registered node name selects the flow
        /// target.</typeparam>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-data-flow" />
        [DebuggerHidden]
        public void AddFlow<TToken, TFlow, TTargetNode>(ObjectFlowBuildAction<TToken> buildAction = null)
            where TFlow : class, IFlow<TToken>
            where TTargetNode : class, IActivityNode
            => AddFlow<TToken, TFlow>(ActivityNode<TTargetNode>.Name, buildAction);

        /// <summary>Adds a typed object flow from the current node to the named target.</summary>
        /// <typeparam name="TToken">Token type carried by the object flow.</typeparam>
        /// <typeparam name="TTransformedToken">Token type produced by the object-flow transformation.</typeparam>
        /// <typeparam name="TTransformationFlow">Typed flow transformation implementing
        /// <see cref="IFlowTransformation{TToken, TTransformedToken}" />.</typeparam>
        /// <param name="targetNodeName">Registered name of the destination node in the containing Activity scope.</param>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-data-flow" />
        [DebuggerHidden]
        public void AddFlow<TToken, TTransformedToken, TTransformationFlow>(string targetNodeName, ObjectFlowBuildAction<TTransformedToken> buildAction = null)
            where TTransformationFlow : class, IFlowTransformation<TToken, TTransformedToken>
            => AddFlow<TToken>(
                targetNodeName,
                b =>
                {
                    b.AddObjectTransformationFlowEvents<TTransformationFlow, TToken, TTransformedToken>();
                    buildAction?.Invoke(b as IObjectFlowBuilder<TTransformedToken>);
                }
            );

        /// <summary>Adds a typed object flow from the current node to the named target.</summary>
        /// <typeparam name="TToken">Token type carried by the object flow.</typeparam>
        /// <typeparam name="TTransformedToken">Token type produced by the object-flow transformation.</typeparam>
        /// <typeparam name="TTransformationFlow">Typed flow transformation implementing
        /// <see cref="IFlowTransformation{TToken, TTransformedToken}" />.</typeparam>
        /// <typeparam name="TTargetNode">Destination node implementation type; its registered node name selects the flow
        /// target.</typeparam>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-data-flow" />
        [DebuggerHidden]
        public void AddFlow<TToken, TTransformedToken, TTransformationFlow, TTargetNode>(ObjectFlowBuildAction<TTransformedToken> buildAction = null)
            where TTransformationFlow : class, IFlowTransformation<TToken, TTransformedToken>
            where TTargetNode : class, IActivityNode
            => AddFlow<TToken, TTransformedToken, TTransformationFlow>(ActivityNode<TTargetNode>.Name, buildAction);
    }

    /// <summary>Provides fluent APIs for extending elements inherited from a reusable Activity definition.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-data-flow" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    public interface IOverridenObjectFlowBase
    {
        /// <summary>Selects and replaces the matching inherited flow from the current node.</summary>
        /// <typeparam name="TToken">Token type carried by the object flow.</typeparam>
        /// <param name="targetNodeName">Registered name of the destination node in the containing Activity scope.</param>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-data-flow" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        void UseFlow<TToken>(string targetNodeName, ObjectFlowBuildAction<TToken> buildAction = null);
        
        /// <summary>Selects and replaces the matching inherited flow from the current node.</summary>
        /// <typeparam name="TToken">Token type carried by the object flow.</typeparam>
        /// <typeparam name="TTargetNode">Destination node implementation type; its registered node name selects the flow
        /// target.</typeparam>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-data-flow" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        [DebuggerHidden]
        public void UseFlow<TToken, TTargetNode>(ObjectFlowBuildAction<TToken> buildAction = null)
            where TTargetNode : class, IActivityNode
            => UseFlow<TToken>(ActivityNode<TTargetNode>.Name, buildAction);
    }
}
