using System.Collections.Generic;
using System.Diagnostics;
using Stateflows.Common;
using Stateflows.Payloads;
using Stateflows.Activities.Registration.Builders;
using Stateflows.Activities.Extensions;

namespace Stateflows.Activities.Registration.Interfaces.Base
{
    /// <summary>Adds typed object flows from the current node.</summary>
    /// <typeparam name="TReturn">Fluent builder type returned after this Activity element is configured.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.1.-Data-Flow" />
    public interface IObjectFlowBase<out TReturn>
    {
        /// <summary>Adds a typed object flow from the current node to the named target.</summary>
        /// <typeparam name="TToken">Token type carried by the object flow.</typeparam>
        /// <param name="targetNodeName">Registered name of the destination node in the containing Activity scope.</param>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.1.-Data-Flow" />
        TReturn AddFlow<TToken>(string targetNodeName, ObjectFlowBuildAction<TToken> buildAction = null);

        /// <summary>Adds a typed object flow from the current node to the named target.</summary>
        /// <typeparam name="TToken">Token type carried by the object flow.</typeparam>
        /// <typeparam name="TTargetNode">Destination node implementation type; its registered node name selects the flow
        /// target.</typeparam>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.1.-Data-Flow" />
        TReturn AddFlow<TToken, TTargetNode>(ObjectFlowBuildAction<TToken> buildAction = null)
            where TTargetNode : class, IActivityNode
            => AddFlow(ActivityNode<TTargetNode>.Name, buildAction);

        /// <summary>Adds a typed object flow from the current node to the named target.</summary>
        /// <typeparam name="TToken">Token type carried by the object flow.</typeparam>
        /// <typeparam name="TFlow">Typed flow implementation implementing <see cref="IFlow&lt;TToken&gt;" />.</typeparam>
        /// <param name="targetNodeName">Registered name of the destination node in the containing Activity scope.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.1.-Data-Flow" />
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
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.1.-Data-Flow" />
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
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.1.-Data-Flow" />
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
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.1.-Data-Flow" />
        TReturn AddFlow<TToken, TTransformedToken, TTransformationFlow, TTargetNode>()
            where TTransformationFlow : class, IFlowTransformation<TToken, TTransformedToken>
            where TTargetNode : class, IActivityNode
            => AddFlow<TToken, TTransformedToken, TTransformationFlow>(ActivityNode<TTargetNode>.Name);

        #region Payload flows
        /// <summary>Adds an object flow carrying payload tokens with any of the given names.</summary>
        /// <param name="tokenNames">Names of the payload tokens the flow takes from this node.</param>
        /// <param name="targetNodeName">Registered name of the target node.</param>
        /// <param name="buildAction">Callback configuring guards and transformations of the flow.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/4.6.-Payloads" />
        [DebuggerHidden]
        TReturn AddPayloadFlow(IReadOnlyList<string> tokenNames, string targetNodeName, ObjectFlowBuildAction<Payload> buildAction = null)
            => PayloadFlowScope.With(tokenNames, nameof(tokenNames), () => AddFlow(targetNodeName, buildAction));

        /// <summary>Adds an object flow carrying payload tokens with any of the given names to the node registered
        /// under the default name of <typeparamref name="TTargetNode"/>.</summary>
        /// <typeparam name="TTargetNode">Target node type.</typeparam>
        /// <param name="tokenNames">Names of the payload tokens the flow takes from this node.</param>
        /// <param name="buildAction">Callback configuring guards and transformations of the flow.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/4.6.-Payloads" />
        [DebuggerHidden]
        TReturn AddPayloadFlow<TTargetNode>(IReadOnlyList<string> tokenNames, ObjectFlowBuildAction<Payload> buildAction = null)
            where TTargetNode : class, IActivityNode
            => AddPayloadFlow(tokenNames, ActivityNode<TTargetNode>.Name, buildAction);

        /// <summary>Adds a class-based object flow carrying payload tokens with any of the given names.</summary>
        /// <typeparam name="TFlow">Flow class.</typeparam>
        /// <param name="tokenNames">Names of the payload tokens the flow takes from this node.</param>
        /// <param name="targetNodeName">Registered name of the target node.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/4.6.-Payloads" />
        [DebuggerHidden]
        TReturn AddPayloadFlow<TFlow>(IReadOnlyList<string> tokenNames, string targetNodeName)
            where TFlow : class, IFlow<Payload>
            => AddPayloadFlow(tokenNames, targetNodeName, b => b.AddObjectFlowEvents<TFlow, Payload>());

        /// <summary>Adds a class-based object flow carrying payload tokens with any of the given names to the node
        /// registered under the default name of <typeparamref name="TTargetNode"/>.</summary>
        /// <typeparam name="TFlow">Flow class.</typeparam>
        /// <typeparam name="TTargetNode">Target node type.</typeparam>
        /// <param name="tokenNames">Names of the payload tokens the flow takes from this node.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/4.6.-Payloads" />
        [DebuggerHidden]
        TReturn AddPayloadFlow<TFlow, TTargetNode>(IReadOnlyList<string> tokenNames)
            where TFlow : class, IFlow<Payload>
            where TTargetNode : class, IActivityNode
            => AddPayloadFlow<TFlow>(tokenNames, ActivityNode<TTargetNode>.Name);

        /// <summary>Adds a class-based object flow that transforms payload tokens with any of the given names to
        /// <typeparamref name="TTransformedToken"/>.</summary>
        /// <typeparam name="TTransformedToken">Token type delivered to the target.</typeparam>
        /// <typeparam name="TTransformationFlow">Transformation flow class.</typeparam>
        /// <param name="tokenNames">Names of the payload tokens the flow takes from this node.</param>
        /// <param name="targetNodeName">Registered name of the target node.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/4.6.-Payloads" />
        [DebuggerHidden]
        TReturn AddPayloadFlow<TTransformedToken, TTransformationFlow>(IReadOnlyList<string> tokenNames, string targetNodeName)
            where TTransformationFlow : class, IFlowTransformation<Payload, TTransformedToken>
            => AddPayloadFlow(tokenNames, targetNodeName, b => b.AddObjectTransformationFlowEvents<TTransformationFlow, Payload, TTransformedToken>());

        /// <summary>Adds a class-based object flow that transforms payload tokens with any of the given names to
        /// <typeparamref name="TTransformedToken"/>, targeting the node registered under the default name of
        /// <typeparamref name="TTargetNode"/>.</summary>
        /// <typeparam name="TTransformedToken">Token type delivered to the target.</typeparam>
        /// <typeparam name="TTransformationFlow">Transformation flow class.</typeparam>
        /// <typeparam name="TTargetNode">Target node type.</typeparam>
        /// <param name="tokenNames">Names of the payload tokens the flow takes from this node.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/4.6.-Payloads" />
        [DebuggerHidden]
        TReturn AddPayloadFlow<TTransformedToken, TTransformationFlow, TTargetNode>(IReadOnlyList<string> tokenNames)
            where TTransformationFlow : class, IFlowTransformation<Payload, TTransformedToken>
            where TTargetNode : class, IActivityNode
            => AddPayloadFlow<TTransformedToken, TTransformationFlow>(tokenNames, ActivityNode<TTargetNode>.Name);

        /// <summary>Adds a class-based object flow that transforms payload tokens with any of the given names to
        /// payload tokens with any of the transformed names.</summary>
        /// <typeparam name="TTransformedToken">Payload token type delivered to the target.</typeparam>
        /// <typeparam name="TTransformationFlow">Transformation flow class.</typeparam>
        /// <param name="tokenNames">Names of the payload tokens the flow takes from this node.</param>
        /// <param name="transformedNames">Names of the payload tokens the transformation may produce.</param>
        /// <param name="targetNodeName">Registered name of the target node.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/4.6.-Payloads" />
        [DebuggerHidden]
        TReturn AddPayloadFlow<TTransformedToken, TTransformationFlow>(IReadOnlyList<string> tokenNames, IReadOnlyList<string> transformedNames, string targetNodeName)
            where TTransformedToken : Payload
            where TTransformationFlow : class, IFlowTransformation<Payload, TTransformedToken>
        {
            var names = PayloadNameSet.Validate(transformedNames, nameof(transformedNames));
            return AddPayloadFlow(tokenNames, targetNodeName, b => PayloadFlowScope.With(names, nameof(transformedNames),
                () => b.AddObjectTransformationFlowEvents<TTransformationFlow, Payload, TTransformedToken>()));
        }

        /// <summary>Adds a class-based object flow that transforms payload tokens with any of the given names to
        /// payload tokens with any of the transformed names, targeting the node registered under the default name of
        /// <typeparamref name="TTargetNode"/>.</summary>
        /// <typeparam name="TTransformedToken">Payload token type delivered to the target.</typeparam>
        /// <typeparam name="TTransformationFlow">Transformation flow class.</typeparam>
        /// <typeparam name="TTargetNode">Target node type.</typeparam>
        /// <param name="tokenNames">Names of the payload tokens the flow takes from this node.</param>
        /// <param name="transformedNames">Names of the payload tokens the transformation may produce.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/4.6.-Payloads" />
        [DebuggerHidden]
        TReturn AddPayloadFlow<TTransformedToken, TTransformationFlow, TTargetNode>(IReadOnlyList<string> tokenNames, IReadOnlyList<string> transformedNames)
            where TTransformedToken : Payload
            where TTransformationFlow : class, IFlowTransformation<Payload, TTransformedToken>
            where TTargetNode : class, IActivityNode
            => AddPayloadFlow<TTransformedToken, TTransformationFlow>(tokenNames, transformedNames, ActivityNode<TTargetNode>.Name);

        /// <summary>Adds a class-based object flow that transforms <typeparamref name="TToken"/> tokens to payload
        /// tokens with any of the transformed names.</summary>
        /// <typeparam name="TToken">Token type the flow takes from this node.</typeparam>
        /// <typeparam name="TTransformedToken">Payload token type delivered to the target.</typeparam>
        /// <typeparam name="TTransformationFlow">Transformation flow class.</typeparam>
        /// <param name="transformedNames">Names of the payload tokens the transformation may produce.</param>
        /// <param name="targetNodeName">Registered name of the target node.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/4.6.-Payloads" />
        [DebuggerHidden]
        TReturn AddPayloadFlow<TToken, TTransformedToken, TTransformationFlow>(IReadOnlyList<string> transformedNames, string targetNodeName)
            where TTransformedToken : Payload
            where TTransformationFlow : class, IFlowTransformation<TToken, TTransformedToken>
        {
            var names = PayloadNameSet.Validate(transformedNames, nameof(transformedNames));
            return AddFlow<TToken>(targetNodeName, b => PayloadFlowScope.With(names, nameof(transformedNames),
                () => b.AddObjectTransformationFlowEvents<TTransformationFlow, TToken, TTransformedToken>()));
        }

        /// <summary>Adds a class-based object flow that transforms <typeparamref name="TToken"/> tokens to payload
        /// tokens with any of the transformed names, targeting the node registered under the default name of
        /// <typeparamref name="TTargetNode"/>.</summary>
        /// <typeparam name="TToken">Token type the flow takes from this node.</typeparam>
        /// <typeparam name="TTransformedToken">Payload token type delivered to the target.</typeparam>
        /// <typeparam name="TTransformationFlow">Transformation flow class.</typeparam>
        /// <typeparam name="TTargetNode">Target node type.</typeparam>
        /// <param name="transformedNames">Names of the payload tokens the transformation may produce.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/4.6.-Payloads" />
        [DebuggerHidden]
        TReturn AddPayloadFlow<TToken, TTransformedToken, TTransformationFlow, TTargetNode>(IReadOnlyList<string> transformedNames)
            where TTransformedToken : Payload
            where TTransformationFlow : class, IFlowTransformation<TToken, TTransformedToken>
            where TTargetNode : class, IActivityNode
            => AddPayloadFlow<TToken, TTransformedToken, TTransformationFlow>(transformedNames, ActivityNode<TTargetNode>.Name);
        #endregion
    }
    
    /// <summary>Provides fluent APIs for extending elements inherited from a reusable Activity definition.</summary>
    /// <typeparam name="TReturn">Fluent builder type returned after this Activity element is configured.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.1.-Data-Flow" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
    public interface IOverridenObjectFlowBase<out TReturn>
    {
        /// <summary>Selects and replaces the matching inherited flow from the current node.</summary>
        /// <typeparam name="TToken">Token type carried by the object flow.</typeparam>
        /// <param name="targetNodeName">Registered name of the destination node in the containing Activity scope.</param>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.1.-Data-Flow" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
        TReturn UseFlow<TToken>(string targetNodeName, ObjectFlowBuildAction<TToken> buildAction = null);

        /// <summary>Selects and replaces the matching inherited flow from the current node.</summary>
        /// <typeparam name="TToken">Token type carried by the object flow.</typeparam>
        /// <typeparam name="TTargetNode">Destination node implementation type; its registered node name selects the flow
        /// target.</typeparam>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.1.-Data-Flow" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
        TReturn UseFlow<TToken, TTargetNode>(ObjectFlowBuildAction<TToken> buildAction = null)
            where TTargetNode : class, IActivityNode
            => UseFlow(ActivityNode<TTargetNode>.Name, buildAction);

        /// <summary>Overrides the inherited object flow to the target node whose payload token names equal the given
        /// set (in any order).</summary>
        /// <param name="tokenNames">Names of the payload tokens carried by the inherited flow.</param>
        /// <param name="targetNodeName">Registered name of the target node.</param>
        /// <param name="buildAction">Callback adding guards and transformations to the inherited flow.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/4.6.-Payloads" />
        [DebuggerHidden]
        TReturn UsePayloadFlow(IReadOnlyList<string> tokenNames, string targetNodeName, ObjectFlowBuildAction<Payload> buildAction = null)
            => PayloadFlowScope.With(tokenNames, nameof(tokenNames), () => UseFlow(targetNodeName, buildAction));

        /// <summary>Overrides the inherited payload object flow to the node registered under the default name of
        /// <typeparamref name="TTargetNode"/>.</summary>
        /// <typeparam name="TTargetNode">Target node type.</typeparam>
        /// <param name="tokenNames">Names of the payload tokens carried by the inherited flow.</param>
        /// <param name="buildAction">Callback adding guards and transformations to the inherited flow.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/4.6.-Payloads" />
        [DebuggerHidden]
        TReturn UsePayloadFlow<TTargetNode>(IReadOnlyList<string> tokenNames, ObjectFlowBuildAction<Payload> buildAction = null)
            where TTargetNode : class, IActivityNode
            => UsePayloadFlow(tokenNames, ActivityNode<TTargetNode>.Name, buildAction);
    }

    /// <summary>Adds fallback typed object flows from the current node.</summary>
    /// <typeparam name="TReturn">Fluent builder type returned after this Activity element is configured.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.1.-Data-Flow" />
    public interface IElseObjectFlowBase<out TReturn>
    {
        /// <summary>Adds a fallback flow used when no ordinary outgoing flow is eligible.</summary>
        /// <typeparam name="TToken">Token type carried by the object flow.</typeparam>
        /// <param name="targetNodeName">Registered name of the destination node in the containing Activity scope.</param>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.1.-Data-Flow" />
        TReturn AddElseFlow<TToken>(string targetNodeName, ElseObjectFlowBuildAction<TToken> buildAction = null);

        /// <summary>Adds a fallback object flow carrying payload tokens with any of the given names.</summary>
        /// <param name="tokenNames">Names of the payload tokens the flow takes from this node.</param>
        /// <param name="targetNodeName">Registered name of the target node.</param>
        /// <param name="buildAction">Callback configuring transformations of the flow.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/4.6.-Payloads" />
        [DebuggerHidden]
        TReturn AddElsePayloadFlow(IReadOnlyList<string> tokenNames, string targetNodeName, ElseObjectFlowBuildAction<Payload> buildAction = null)
            => PayloadFlowScope.With(tokenNames, nameof(tokenNames), () => AddElseFlow(targetNodeName, buildAction));

        /// <summary>Adds a fallback payload flow to the node identified by its type.</summary>
        TReturn AddElsePayloadFlow<TTargetNode>(IReadOnlyList<string> tokenNames, ElseObjectFlowBuildAction<Payload> buildAction = null)
            where TTargetNode : class, IActivityNode
            => AddElsePayloadFlow(tokenNames, ActivityNode<TTargetNode>.Name, buildAction);
    }

    /// <summary>Adds typed object decision flows from a decision node.</summary>
    /// <typeparam name="TToken">Token type carried by the object flow.</typeparam>
    /// <typeparam name="TReturn">Fluent builder type returned after this Activity element is configured.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.1.-Data-Flow" />
    public interface IDecisionFlowBase<out TToken, out TReturn>
    {
        /// <summary>Adds a guarded object flow from the decision node to a target for its token type.</summary>
        /// <param name="targetNodeName">Registered name of the destination node in the containing Activity scope.</param>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.1.-Data-Flow" />
        TReturn AddFlow(string targetNodeName, ObjectFlowBuildAction<TToken> buildAction = null);

        /// <summary>Adds a guarded object flow from the decision node to a target for its token type.</summary>
        /// <typeparam name="TTargetNode">Destination node implementation type; its registered node name selects the flow
        /// target.</typeparam>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.1.-Data-Flow" />
        TReturn AddFlow<TTargetNode>(ObjectFlowBuildAction<TToken> buildAction = null)
            where TTargetNode : class, IActivityNode
            => AddFlow(ActivityNode<TTargetNode>.Name, buildAction);

        /// <summary>Adds a guarded object flow from the decision node to a target for its token type.</summary>
        /// <typeparam name="TFlow">Typed flow implementation implementing <see cref="IFlow&lt;TToken&gt;" />.</typeparam>
        /// <param name="targetNodeName">Registered name of the destination node in the containing Activity scope.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.1.-Data-Flow" />
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
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.1.-Data-Flow" />
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
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.1.-Data-Flow" />
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
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.1.-Data-Flow" />
        TReturn AddFlow<TTransformedToken, TTransformationFlow, TTargetNode>()
            where TTransformationFlow : class, IFlowTransformation<TToken, TTransformedToken>
            where TTargetNode : class, IActivityNode
            => AddFlow<TTransformedToken, TTransformationFlow>(ActivityNode<TTargetNode>.Name);

        /// <summary>Adds a class-based object flow that transforms the decision's tokens to payload tokens with any of
        /// the transformed names.</summary>
        /// <typeparam name="TTransformedToken">Payload token type delivered to the target.</typeparam>
        /// <typeparam name="TTransformationFlow">Transformation flow class.</typeparam>
        /// <param name="transformedNames">Names of the payload tokens the transformation may produce.</param>
        /// <param name="targetNodeName">Registered name of the target node.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/4.6.-Payloads" />
        [DebuggerHidden]
        TReturn AddPayloadFlow<TTransformedToken, TTransformationFlow>(IReadOnlyList<string> transformedNames, string targetNodeName)
            where TTransformedToken : Payload
            where TTransformationFlow : class, IFlowTransformation<TToken, TTransformedToken>
        {
            var names = PayloadNameSet.Validate(transformedNames, nameof(transformedNames));
            return AddFlow(targetNodeName, b => PayloadFlowScope.With(names, nameof(transformedNames),
                () => b.AddObjectTransformationFlowEvents<TTransformationFlow, TToken, TTransformedToken>()));
        }

        /// <summary>Adds a class-based object flow that transforms the decision's tokens to payload tokens with any of
        /// the transformed names, targeting the node registered under the default name of
        /// <typeparamref name="TTargetNode"/>.</summary>
        /// <typeparam name="TTransformedToken">Payload token type delivered to the target.</typeparam>
        /// <typeparam name="TTransformationFlow">Transformation flow class.</typeparam>
        /// <typeparam name="TTargetNode">Target node type.</typeparam>
        /// <param name="transformedNames">Names of the payload tokens the transformation may produce.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/4.6.-Payloads" />
        [DebuggerHidden]
        TReturn AddPayloadFlow<TTransformedToken, TTransformationFlow, TTargetNode>(IReadOnlyList<string> transformedNames)
            where TTransformedToken : Payload
            where TTransformationFlow : class, IFlowTransformation<TToken, TTransformedToken>
            where TTargetNode : class, IActivityNode
            => AddPayloadFlow<TTransformedToken, TTransformationFlow>(transformedNames, ActivityNode<TTargetNode>.Name);
    }

    /// <summary>Provides fluent APIs for extending elements inherited from a reusable Activity definition.</summary>
    /// <typeparam name="TToken">Token type carried by the object flow.</typeparam>
    /// <typeparam name="TReturn">Fluent builder type returned after this Activity element is configured.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.1.-Data-Flow" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
    public interface IOverridenDecisionFlowBase<out TToken, out TReturn>
    {
        /// <summary>Selects and replaces the matching inherited flow from the current node.</summary>
        /// <param name="targetNodeName">Registered name of the destination node in the containing Activity scope.</param>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.1.-Data-Flow" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
        TReturn UseFlow(string targetNodeName, ObjectFlowBuildAction<TToken> buildAction = null);

        /// <summary>Selects and replaces the matching inherited flow from the current node.</summary>
        /// <typeparam name="TTargetNode">Destination node implementation type; its registered node name selects the flow
        /// target.</typeparam>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.1.-Data-Flow" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
        TReturn UseFlow<TTargetNode>(ObjectFlowBuildAction<TToken> buildAction = null)
            where TTargetNode : class, IActivityNode
            => UseFlow(ActivityNode<TTargetNode>.Name, buildAction);

        /// <summary>Overrides the inherited decision flow carrying payload tokens with the specified names.</summary>
        /// <param name="tokenNames">Names of the payload tokens carried by the inherited flow.</param>
        /// <param name="targetNodeName">Registered name of the destination node.</param>
        /// <param name="buildAction">Callback configuring guards and transformations of the flow.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/4.6.-Payloads" />
        TReturn UsePayloadFlow(IReadOnlyList<string> tokenNames, string targetNodeName,
            ObjectFlowBuildAction<Payload> buildAction = null)
            => PayloadFlowScope.With(tokenNames, nameof(tokenNames), () => UseFlow(targetNodeName,
                builder => buildAction?.Invoke((IObjectFlowBuilder<Payload>)builder)));

        /// <summary>Overrides the inherited decision flow to the node registered under the target type.</summary>
        /// <typeparam name="TTargetNode">Target node type.</typeparam>
        /// <param name="tokenNames">Names of the payload tokens carried by the inherited flow.</param>
        /// <param name="buildAction">Callback configuring guards and transformations of the flow.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/4.6.-Payloads" />
        TReturn UsePayloadFlow<TTargetNode>(IReadOnlyList<string> tokenNames,
            ObjectFlowBuildAction<Payload> buildAction = null)
            where TTargetNode : class, IActivityNode
            => UsePayloadFlow(tokenNames, ActivityNode<TTargetNode>.Name, buildAction);
    }

    /// <summary>Adds fallback typed object flows from a decision node.</summary>
    /// <typeparam name="TToken">Token type carried by the object flow.</typeparam>
    /// <typeparam name="TReturn">Fluent builder type returned after this Activity element is configured.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.1.-Data-Flow" />
    public interface IElseDecisionFlowBase<out TToken, out TReturn>
    {
        /// <summary>Adds a fallback flow used when no ordinary outgoing flow is eligible.</summary>
        /// <param name="targetNodeName">Registered name of the destination node in the containing Activity scope.</param>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.1.-Data-Flow" />
        TReturn AddElseFlow(string targetNodeName, ElseObjectFlowBuildAction<TToken> buildAction = null);

        /// <summary>Adds a fallback flow used when no ordinary outgoing flow is eligible.</summary>
        /// <typeparam name="TTargetNode">Destination node implementation type; its registered node name selects the flow
        /// target.</typeparam>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.1.-Data-Flow" />
        TReturn AddElseFlow<TTargetNode>(ObjectFlowBuildAction<TToken> buildAction = null)
            where TTargetNode : class, IActivityNode
            => AddElseFlow(ActivityNode<TTargetNode>.Name, b => buildAction?.Invoke(b as IObjectFlowBuilder<TToken>));

        /// <summary>Adds a fallback flow used when no ordinary outgoing flow is eligible.</summary>
        /// <typeparam name="TTransformedToken">Token type produced by the object-flow transformation.</typeparam>
        /// <typeparam name="TElseTransformationFlow">Typed flow transformation implementing
        /// <see cref="IFlowTransformation{TToken, TTransformedToken}" />.</typeparam>
        /// <param name="targetNodeName">Registered name of the destination node in the containing Activity scope.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.1.-Data-Flow" />
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
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.1.-Data-Flow" />
        TReturn AddElseFlow<TTransformedToken, TElseTransformationFlow, TTargetNode>()
            where TElseTransformationFlow : class, IFlowTransformation<TToken, TTransformedToken>
            where TTargetNode : class, IActivityNode
            => AddElseFlow<TTransformedToken, TElseTransformationFlow>(ActivityNode<TTargetNode>.Name);

        /// <summary>Adds a class-based fallback flow that transforms the decision's tokens to payload tokens with any
        /// of the transformed names.</summary>
        /// <typeparam name="TTransformedToken">Payload token type delivered to the target.</typeparam>
        /// <typeparam name="TElseTransformationFlow">Transformation flow class.</typeparam>
        /// <param name="transformedNames">Names of the payload tokens the transformation may produce.</param>
        /// <param name="targetNodeName">Registered name of the target node.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/4.6.-Payloads" />
        [DebuggerHidden]
        TReturn AddElsePayloadFlow<TTransformedToken, TElseTransformationFlow>(IReadOnlyList<string> transformedNames, string targetNodeName)
            where TTransformedToken : Payload
            where TElseTransformationFlow : class, IFlowTransformation<TToken, TTransformedToken>
        {
            var names = PayloadNameSet.Validate(transformedNames, nameof(transformedNames));
            return AddElseFlow(targetNodeName, b => PayloadFlowScope.With(names, nameof(transformedNames),
                () => (b as IObjectFlowBuilder<TToken>).AddElseObjectTransformationFlowEvents<TElseTransformationFlow, TToken, TTransformedToken>()));
        }

        /// <summary>Adds a class-based fallback flow that transforms the decision's tokens to payload tokens with any
        /// of the transformed names, targeting the node registered under the default name of
        /// <typeparamref name="TTargetNode"/>.</summary>
        /// <typeparam name="TTransformedToken">Payload token type delivered to the target.</typeparam>
        /// <typeparam name="TElseTransformationFlow">Transformation flow class.</typeparam>
        /// <typeparam name="TTargetNode">Target node type.</typeparam>
        /// <param name="transformedNames">Names of the payload tokens the transformation may produce.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/4.6.-Payloads" />
        [DebuggerHidden]
        TReturn AddElsePayloadFlow<TTransformedToken, TElseTransformationFlow, TTargetNode>(IReadOnlyList<string> transformedNames)
            where TTransformedToken : Payload
            where TElseTransformationFlow : class, IFlowTransformation<TToken, TTransformedToken>
            where TTargetNode : class, IActivityNode
            => AddElsePayloadFlow<TTransformedToken, TElseTransformationFlow>(transformedNames, ActivityNode<TTargetNode>.Name);
    }

    /// <summary>Provides fluent APIs for extending elements inherited from a reusable Activity definition.</summary>
    /// <typeparam name="TToken">Token type carried by the object flow.</typeparam>
    /// <typeparam name="TReturn">Fluent builder type returned after this Activity element is configured.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.1.-Data-Flow" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
    public interface IOverridenElseDecisionFlowBase<out TToken, out TReturn>
    {
        /// <summary>Selects and replaces the matching inherited flow from the current node.</summary>
        /// <param name="targetNodeName">Registered name of the destination node in the containing Activity scope.</param>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.1.-Data-Flow" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
        TReturn UseElseFlow(string targetNodeName, ElseObjectFlowBuildAction<TToken> buildAction = null);

        /// <summary>Selects and replaces the matching inherited flow from the current node.</summary>
        /// <typeparam name="TTargetNode">Destination node implementation type; its registered node name selects the flow
        /// target.</typeparam>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.1.-Data-Flow" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
        TReturn UseElseFlow<TTargetNode>(ObjectFlowBuildAction<TToken> buildAction = null)
            where TTargetNode : class, IActivityNode
            => UseElseFlow(ActivityNode<TTargetNode>.Name, b => buildAction?.Invoke(b as IObjectFlowBuilder<TToken>));
    }

    /// <summary>Adds typed object flows where the builder does not return a fluent result.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.1.-Data-Flow" />
    public interface IObjectFlowBase
    {
        /// <summary>Adds a typed object flow from the current node to the named target.</summary>
        /// <typeparam name="TToken">Token type carried by the object flow.</typeparam>
        /// <param name="targetNodeName">Registered name of the destination node in the containing Activity scope.</param>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.1.-Data-Flow" />
        void AddFlow<TToken>(string targetNodeName, ObjectFlowBuildAction<TToken> buildAction = null);
        
        /// <summary>Adds a typed object flow from the current node to the named target.</summary>
        /// <typeparam name="TToken">Token type carried by the object flow.</typeparam>
        /// <typeparam name="TTargetNode">Destination node implementation type; its registered node name selects the flow
        /// target.</typeparam>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.1.-Data-Flow" />
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
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.1.-Data-Flow" />
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
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.1.-Data-Flow" />
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
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.1.-Data-Flow" />
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
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.1.-Data-Flow" />
        [DebuggerHidden]
        public void AddFlow<TToken, TTransformedToken, TTransformationFlow, TTargetNode>(ObjectFlowBuildAction<TTransformedToken> buildAction = null)
            where TTransformationFlow : class, IFlowTransformation<TToken, TTransformedToken>
            where TTargetNode : class, IActivityNode
            => AddFlow<TToken, TTransformedToken, TTransformationFlow>(ActivityNode<TTargetNode>.Name, buildAction);
    }

    /// <summary>Provides fluent APIs for extending elements inherited from a reusable Activity definition.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.1.-Data-Flow" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
    public interface IOverridenObjectFlowBase
    {
        /// <summary>Selects and replaces the matching inherited flow from the current node.</summary>
        /// <typeparam name="TToken">Token type carried by the object flow.</typeparam>
        /// <param name="targetNodeName">Registered name of the destination node in the containing Activity scope.</param>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.1.-Data-Flow" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
        void UseFlow<TToken>(string targetNodeName, ObjectFlowBuildAction<TToken> buildAction = null);
        
        /// <summary>Selects and replaces the matching inherited flow from the current node.</summary>
        /// <typeparam name="TToken">Token type carried by the object flow.</typeparam>
        /// <typeparam name="TTargetNode">Destination node implementation type; its registered node name selects the flow
        /// target.</typeparam>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.2.1.-Data-Flow" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
        [DebuggerHidden]
        public void UseFlow<TToken, TTargetNode>(ObjectFlowBuildAction<TToken> buildAction = null)
            where TTargetNode : class, IActivityNode
            => UseFlow<TToken>(ActivityNode<TTargetNode>.Name, buildAction);
    }
}
