using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Entities.Enums;

namespace Stateflows.Activities
{
    /// <summary>
    /// Receives an ordered description of a finalized activity graph during an
    /// activity-registration visit. Each callback is awaited before the next
    /// callback; a callback failure stops the visit and propagates to its caller.
    /// The activity identifier is provided before its name and version, followed by
    /// callback-specific metadata. Graph nodes are declared before their
    /// configuration and operations, then initializer/finalizer registrations and
    /// flows are reported, with <see cref="ActivityAddedAsync"/> last.
    /// </summary>
    /// <remarks>
    /// Graph and definition identifiers are engine identities within a definition;
    /// node identifiers identify nodes, while flow and operation identifiers identify
    /// their respective registrations. The <c>order</c> arguments are zero-based
    /// ordinals local to the relevant initializer, finalizer, node, flow, or guard
    /// expression. Generic type arguments identify the CLR event, token, or
    /// implementation type associated with a notification.
    /// </remarks>
    public interface IActivityVisitor
    {
        /// <summary>
        /// Runs first for an activity definition, before its type, nodes,
        /// registrations, configuration, and completion notifications.
        /// </summary>
        /// <param name="activityId">The engine identity of the activity definition.</param>
        /// <param name="activityName">The registered activity name associated with this definition.</param>
        /// <param name="activityVersion">The registered version of the activity definition.</param>
        /// <param name="ownerClass">The owning behavior class, when the activity belongs to another behavior.</param>
        /// <param name="parentClass">The parent behavior class, when the activity is nested under another behavior.</param>
        /// <param name="hasDefaultInstance">Whether a default instance initialization is registered for this activity's behavior class.</param>
        /// <returns>A task completed when this visitor has finished handling the notification.</returns>
        Task ActivityAddingAsync(string activityId, string activityName, int activityVersion, BehaviorClass? ownerClass = null, BehaviorClass? parentClass = null, bool hasDefaultInstance = false) => Task.CompletedTask;

        /// <summary>
        /// Runs after all other notifications for this activity and is the strictly
        /// final callback of the visit.
        /// </summary>
        /// <param name="activityId">The engine identity of the activity definition.</param>
        /// <param name="activityName">The registered activity name associated with this definition.</param>
        /// <param name="activityVersion">The registered version of the activity definition.</param>
        /// <returns>A task completed when this visitor has finished handling the notification.</returns>
        Task ActivityAddedAsync(string activityId, string activityName, int activityVersion) => Task.CompletedTask;

        /// <summary>Reports the activity's registered CLR implementation type after the initial notification.</summary>
        /// <typeparam name="TActivity">The concrete activity implementation type.</typeparam>
        /// <param name="activityId">The engine identity of the activity definition.</param>
        /// <param name="activityName">The registered activity name associated with this definition.</param>
        /// <param name="activityVersion">The registered version of the activity definition.</param>
        /// <returns>A task completed when this visitor has finished handling the notification.</returns>
        Task ActivityTypeAddedAsync<TActivity>(string activityId, string activityName, int activityVersion) where TActivity : class, IActivity => Task.CompletedTask;

        /// <summary>Reports one event-triggered initializer action registration.</summary>
        /// <typeparam name="TEvent">The CLR event type that triggers this initializer.</typeparam>
        /// <param name="initializerId">The engine identity of this initializer action registration.</param>
        /// <param name="activityName">The registered activity name associated with the initializer.</param>
        /// <param name="activityVersion">The registered version of the activity definition.</param>
        /// <param name="eventNames">The ordered mapped event names that trigger this initializer.</param>
        /// <param name="order">The zero-based position of this action in the initializer pipeline for this event.</param>
        /// <returns>A task completed when this visitor has finished handling the notification.</returns>
        Task InitializerAddedAsync<TEvent>(string initializerId, string activityName, int activityVersion, IReadOnlyList<string> eventNames, int order) => Task.CompletedTask;

        /// <summary>Reports the implementation type registered for an event-triggered initializer action.</summary>
        /// <typeparam name="TEvent">The CLR event type that triggers the initializer.</typeparam>
        /// <typeparam name="TInitializer">The concrete initializer implementation type.</typeparam>
        /// <param name="initializerId">The engine identity of the initializer action registration.</param>
        /// <param name="activityName">The registered activity name associated with the initializer.</param>
        /// <param name="activityVersion">The registered version of the activity definition.</param>
        /// <returns>A task completed when this visitor has finished handling the notification.</returns>
        Task InitializerTypeAddedAsync<TEvent, TInitializer>(string initializerId, string activityName, int activityVersion) where TInitializer : class, IInitializer<TEvent> => Task.CompletedTask;

        /// <summary>Reports one default-initializer action registration.</summary>
        /// <param name="initializerId">The engine identity of this default-initializer action registration.</param>
        /// <param name="activityName">The registered activity name associated with the initializer.</param>
        /// <param name="activityVersion">The registered version of the activity definition.</param>
        /// <param name="order">The zero-based position of this action in the default-initializer pipeline.</param>
        /// <returns>A task completed when this visitor has finished handling the notification.</returns>
        Task DefaultInitializerAddedAsync(string initializerId, string activityName, int activityVersion, int order) => Task.CompletedTask;

        /// <summary>Reports the implementation type registered for the default initializer.</summary>
        /// <typeparam name="TInitializer">The concrete default-initializer implementation type.</typeparam>
        /// <param name="initializerId">The engine identity of the default-initializer action registration.</param>
        /// <param name="activityName">The registered activity name associated with the initializer.</param>
        /// <param name="activityVersion">The registered version of the activity definition.</param>
        /// <returns>A task completed when this visitor has finished handling the notification.</returns>
        Task DefaultInitializerTypeAddedAsync<TInitializer>(string initializerId, string activityName, int activityVersion) where TInitializer : class, IDefaultInitializer => Task.CompletedTask;

        /// <summary>Reports one activity finalizer action registration.</summary>
        /// <param name="finalizerId">The engine identity of this finalizer action registration.</param>
        /// <param name="activityName">The registered activity name associated with the finalizer.</param>
        /// <param name="activityVersion">The registered version of the activity definition.</param>
        /// <param name="order">The zero-based position of this action in the activity finalization pipeline.</param>
        /// <returns>A task completed when this visitor has finished handling the notification.</returns>
        Task FinalizerAddedAsync(string finalizerId, string activityName, int activityVersion, int order) => Task.CompletedTask;

        /// <summary>Reports the implementation type registered for a finalizer action.</summary>
        /// <typeparam name="TFinalizer">The concrete finalizer implementation type.</typeparam>
        /// <param name="finalizerId">The engine identity of the finalizer action registration.</param>
        /// <param name="activityName">The registered activity name associated with the finalizer.</param>
        /// <param name="activityVersion">The registered version of the activity definition.</param>
        /// <returns>A task completed when this visitor has finished handling the notification.</returns>
        Task FinalizerTypeAddedAsync<TFinalizer>(string finalizerId, string activityName, int activityVersion) where TFinalizer : class, IFinalizer => Task.CompletedTask;

        /// <summary>
        /// Reports a node in containment traversal order, before its configuration,
        /// operations, and any specialized event-node notification.
        /// </summary>
        /// <param name="nodeId">The engine identity of the node within this activity definition.</param>
        /// <param name="activityName">The registered activity name containing the node.</param>
        /// <param name="activityVersion">The registered version of the activity definition.</param>
        /// <param name="nodeName">The node's registered name.</param>
        /// <param name="nodeType">The graph node kind.</param>
        /// <param name="parentNodeId">The containing node's identity, or <see langword="null"/> when the node is at the activity root.</param>
        /// <returns>A task completed when this visitor has finished handling the notification.</returns>
        Task NodeAddedAsync(string nodeId, string activityName, int activityVersion, string nodeName, NodeType nodeType, string? parentNodeId) => Task.CompletedTask;

        /// <summary>Reports a node's concrete CLR implementation type when that type is registered.</summary>
        /// <typeparam name="TNode">The concrete node implementation type.</typeparam>
        /// <param name="nodeId">The engine identity of the node within this activity definition.</param>
        /// <param name="activityName">The registered activity name containing the node.</param>
        /// <param name="activityVersion">The registered version of the activity definition.</param>
        /// <returns>A task completed when this visitor has finished handling the notification.</returns>
        Task NodeTypeAddedAsync<TNode>(string nodeId, string activityName, int activityVersion) where TNode : class, IActivityNode => Task.CompletedTask;

        /// <summary>Reports the finalized configuration options for a node.</summary>
        /// <param name="nodeId">The engine identity of the configured node.</param>
        /// <param name="activityName">The registered activity name containing the node.</param>
        /// <param name="activityVersion">The registered version of the activity definition.</param>
        /// <param name="options">The node's configured options.</param>
        /// <param name="chunkSize">The configured batch chunk size.</param>
        /// <param name="maxConcurrency">The configured maximum concurrency.</param>
        /// <param name="anchored">Whether the node is configured as anchored.</param>
        /// <param name="publishScope">The configured publication scope.</param>
        /// <param name="timeToLive">For a publication node, the configured time to live in seconds; zero disables publication history and retries unless the published event is retained.</param>
        /// <param name="retain">Whether node data is configured to be retained.</param>
        /// <returns>A task completed when this visitor has finished handling the notification.</returns>
        Task NodeConfigurationAddedAsync(string nodeId, string activityName, int activityVersion, NodeOptions options, int chunkSize, int maxConcurrency, bool anchored, PublishScope publishScope, int timeToLive, bool retain) => Task.CompletedTask;

        /// <summary>Reports a token input pin owned by a node or by the activity root.</summary>
        /// <typeparam name="TToken">The CLR token type accepted by this input pin.</typeparam>
        /// <param name="pinId">The engine identity of the pin.</param>
        /// <param name="activityName">The registered activity name containing the pin.</param>
        /// <param name="activityVersion">The registered version of the activity definition.</param>
        /// <param name="parentNodeId">The owning node's identity, or <see langword="null"/> for a root-level pin.</param>
        /// <returns>A task completed when this visitor has finished handling the notification.</returns>
        Task InputPinAddedAsync<TToken>(string pinId, string activityName, int activityVersion, string? parentNodeId) => Task.CompletedTask;

        /// <summary>Reports a token output pin owned by a node or by the activity root.</summary>
        /// <typeparam name="TToken">The CLR token type produced by this output pin.</typeparam>
        /// <param name="pinId">The engine identity of the pin.</param>
        /// <param name="activityName">The registered activity name containing the pin.</param>
        /// <param name="activityVersion">The registered version of the activity definition.</param>
        /// <param name="parentNodeId">The owning node's identity, or <see langword="null"/> for a root-level pin.</param>
        /// <returns>A task completed when this visitor has finished handling the notification.</returns>
        Task OutputPinAddedAsync<TToken>(string pinId, string activityName, int activityVersion, string? parentNodeId) => Task.CompletedTask;

        /// <summary>Reports the mapped event names accepted by an accept-event node.</summary>
        /// <typeparam name="TEvent">The CLR event type accepted by the node.</typeparam>
        /// <param name="nodeId">The engine identity of the accept-event node.</param>
        /// <param name="activityName">The registered activity name containing the node.</param>
        /// <param name="activityVersion">The registered version of the activity definition.</param>
        /// <param name="eventNames">The ordered effective event names accepted by the node.</param>
        /// <returns>A task completed when this visitor has finished handling the notification.</returns>
        Task AcceptEventNodeAddedAsync<TEvent>(string nodeId, string activityName, int activityVersion, IReadOnlyList<string> eventNames) => Task.CompletedTask;

        /// <summary>Reports the implementation type of an accept-event node.</summary>
        /// <typeparam name="TEvent">The CLR event type associated with the implementation.</typeparam>
        /// <typeparam name="TNode">The concrete accept-event node implementation type.</typeparam>
        /// <param name="nodeId">The engine identity of the node.</param>
        /// <param name="activityName">The registered activity name containing the node.</param>
        /// <param name="activityVersion">The registered version of the activity definition.</param>
        /// <returns>A task completed when this visitor has finished handling the notification.</returns>
        Task AcceptEventNodeTypeAddedAsync<TEvent, TNode>(string nodeId, string activityName, int activityVersion) where TNode : class, IAcceptEventActionNode<TEvent> => Task.CompletedTask;

        /// <summary>Reports the implementation type of a time-event node.</summary>
        /// <typeparam name="TNode">The concrete time-event node implementation type.</typeparam>
        /// <param name="nodeId">The engine identity of the node.</param>
        /// <param name="activityName">The registered activity name containing the node.</param>
        /// <param name="activityVersion">The registered version of the activity definition.</param>
        /// <returns>A task completed when this visitor has finished handling the notification.</returns>
        Task TimeEventNodeTypeAddedAsync<TNode>(string nodeId, string activityName, int activityVersion) where TNode : class, ITimeEventActionNode => Task.CompletedTask;

        /// <summary>Reports the mapped event names sent by a send-event node.</summary>
        /// <typeparam name="TEvent">The CLR event type sent by the node.</typeparam>
        /// <param name="nodeId">The engine identity of the send-event node.</param>
        /// <param name="activityName">The registered activity name containing the node.</param>
        /// <param name="activityVersion">The registered version of the activity definition.</param>
        /// <param name="eventNames">The ordered effective event names sent by the node.</param>
        /// <returns>A task completed when this visitor has finished handling the notification.</returns>
        Task SendEventNodeAddedAsync<TEvent>(string nodeId, string activityName, int activityVersion, IReadOnlyList<string> eventNames) => Task.CompletedTask;

        /// <summary>Reports the implementation type of a send-event node.</summary>
        /// <typeparam name="TEvent">The CLR event type associated with the implementation.</typeparam>
        /// <typeparam name="TNode">The concrete send-event node implementation type.</typeparam>
        /// <param name="nodeId">The engine identity of the node.</param>
        /// <param name="activityName">The registered activity name containing the node.</param>
        /// <param name="activityVersion">The registered version of the activity definition.</param>
        /// <returns>A task completed when this visitor has finished handling the notification.</returns>
        Task SendEventNodeTypeAddedAsync<TEvent, TNode>(string nodeId, string activityName, int activityVersion) where TNode : class, ISendEventActionNode<TEvent> => Task.CompletedTask;

        /// <summary>Reports the mapped event names published by a publish-event node.</summary>
        /// <typeparam name="TEvent">The CLR event type published by the node.</typeparam>
        /// <param name="nodeId">The engine identity of the publish-event node.</param>
        /// <param name="activityName">The registered activity name containing the node.</param>
        /// <param name="activityVersion">The registered version of the activity definition.</param>
        /// <param name="eventNames">The ordered effective event names published by the node.</param>
        /// <returns>A task completed when this visitor has finished handling the notification.</returns>
        Task PublishEventNodeAddedAsync<TEvent>(string nodeId, string activityName, int activityVersion, IReadOnlyList<string> eventNames) => Task.CompletedTask;

        /// <summary>Reports the implementation type of a publish-event node.</summary>
        /// <typeparam name="TEvent">The CLR event type associated with the implementation.</typeparam>
        /// <typeparam name="TNode">The concrete publish-event node implementation type.</typeparam>
        /// <param name="nodeId">The engine identity of the node.</param>
        /// <param name="activityName">The registered activity name containing the node.</param>
        /// <param name="activityVersion">The registered version of the activity definition.</param>
        /// <returns>A task completed when this visitor has finished handling the notification.</returns>
        Task PublishEventNodeTypeAddedAsync<TEvent, TNode>(string nodeId, string activityName, int activityVersion) where TNode : class, IPublishEventActionNode<TEvent> => Task.CompletedTask;

        /// <summary>Reports a control-flow edge between two node identities.</summary>
        /// <param name="flowId">The engine identity of the control-flow edge.</param>
        /// <param name="activityName">The registered activity name containing the edge.</param>
        /// <param name="activityVersion">The registered version of the activity definition.</param>
        /// <param name="sourceNodeId">The identity of the edge's source node.</param>
        /// <param name="targetNodeId">The identity of the edge's target node.</param>
        /// <param name="isElse">Whether this edge is marked as an else branch.</param>
        /// <returns>A task completed when this visitor has finished handling the notification.</returns>
        Task ControlFlowAddedAsync(string flowId, string activityName, int activityVersion, string sourceNodeId, string targetNodeId, bool isElse) => Task.CompletedTask;

        /// <summary>Reports the implementation type registered for a control-flow edge.</summary>
        /// <typeparam name="TFlow">The concrete control-flow implementation type.</typeparam>
        /// <param name="flowId">The engine identity of the control-flow edge.</param>
        /// <param name="activityName">The registered activity name containing the edge.</param>
        /// <param name="activityVersion">The registered version of the activity definition.</param>
        /// <returns>A task completed when this visitor has finished handling the notification.</returns>
        Task ControlFlowTypeAddedAsync<TFlow>(string flowId, string activityName, int activityVersion) where TFlow : class, IControlFlow => Task.CompletedTask;

        /// <summary>Reports a token-carrying flow between two node identities.</summary>
        /// <typeparam name="TToken">The CLR token type carried by the flow.</typeparam>
        /// <param name="flowId">The engine identity of the flow.</param>
        /// <param name="activityName">The registered activity name containing the flow.</param>
        /// <param name="activityVersion">The registered version of the activity definition.</param>
        /// <param name="sourceNodeId">The identity of the flow's source node.</param>
        /// <param name="targetNodeId">The identity of the flow's target node.</param>
        /// <param name="isElse">Whether this flow is marked as an else branch.</param>
        /// <returns>A task completed when this visitor has finished handling the notification.</returns>
        Task FlowAddedAsync<TToken>(string flowId, string activityName, int activityVersion, string sourceNodeId, string targetNodeId, bool isElse) => Task.CompletedTask;

        /// <summary>Reports the implementation type registered for a token-carrying flow.</summary>
        /// <typeparam name="TToken">The CLR token type carried by the flow.</typeparam>
        /// <typeparam name="TFlow">The concrete flow implementation type.</typeparam>
        /// <param name="flowId">The engine identity of the flow.</param>
        /// <param name="activityName">The registered activity name containing the flow.</param>
        /// <param name="activityVersion">The registered version of the activity definition.</param>
        /// <returns>A task completed when this visitor has finished handling the notification.</returns>
        Task FlowTypeAddedAsync<TToken, TFlow>(string flowId, string activityName, int activityVersion) where TFlow : class, IFlow<TToken> => Task.CompletedTask;

        /// <summary>Reports the finalized routing and token configuration of a flow.</summary>
        /// <param name="flowId">The engine identity of the flow.</param>
        /// <param name="activityName">The registered activity name containing the flow.</param>
        /// <param name="activityVersion">The registered version of the activity definition.</param>
        /// <param name="targetTokenType">The CLR token type expected at the target.</param>
        /// <param name="weight">The configured flow weight.</param>
        /// <param name="isCrossScope">Whether the flow crosses a node scope.</param>
        /// <param name="ownerNodeId">The identity of the graph node or graph scope that owns the flow.</param>
        /// <returns>A task completed when this visitor has finished handling the notification.</returns>
        Task FlowConfigurationAddedAsync(string flowId, string activityName, int activityVersion, Type targetTokenType, int weight, bool isCrossScope, string ownerNodeId) => Task.CompletedTask;

        /// <summary>Reports an operation registered in a node's initialize, execute, or finalize pipeline.</summary>
        /// <param name="operationId">The engine identity of the node-operation registration.</param>
        /// <param name="activityName">The registered activity name containing the operation.</param>
        /// <param name="activityVersion">The registered version of the activity definition.</param>
        /// <param name="nodeId">The identity of the node whose operation is registered.</param>
        /// <param name="kind">Whether the operation runs during node initialization, execution, or finalization.</param>
        /// <param name="order">The zero-based position in the node pipeline identified by <paramref name="kind"/>.</param>
        /// <returns>A task completed when this visitor has finished handling the notification.</returns>
        Task NodeOperationAddedAsync(string operationId, string activityName, int activityVersion, string nodeId, ActivityOperationKind kind, int order) => Task.CompletedTask;

        /// <summary>Reports the concrete implementation type of a registered node operation.</summary>
        /// <typeparam name="TImplementation">The CLR implementation type registered for the operation.</typeparam>
        /// <param name="operationId">The engine identity of the node-operation registration.</param>
        /// <param name="activityName">The registered activity name containing the operation.</param>
        /// <param name="activityVersion">The registered version of the activity definition.</param>
        /// <returns>A task completed when this visitor has finished handling the notification.</returns>
        Task NodeOperationTypeAddedAsync<TImplementation>(string operationId, string activityName, int activityVersion) where TImplementation : class => Task.CompletedTask;

        /// <summary>Reports a leaf guard registration in a token-carrying flow's guard pipeline.</summary>
        /// <typeparam name="TToken">The CLR token type considered by the guard.</typeparam>
        /// <param name="guardId">The engine identity of the guard registration.</param>
        /// <param name="activityName">The registered activity name containing the guard.</param>
        /// <param name="activityVersion">The registered version of the activity definition.</param>
        /// <param name="flowId">The identity of the flow whose guard pipeline contains this registration.</param>
        /// <param name="order">The zero-based position of the guard in its flow pipeline or guard group.</param>
        /// <returns>A task completed when this visitor has finished handling the notification.</returns>
        Task FlowGuardAddedAsync<TToken>(string guardId, string activityName, int activityVersion, string flowId, int order) => Task.CompletedTask;

        /// <summary>Reports a guard expression, including a Boolean group or leaf, in a flow pipeline.</summary>
        /// <param name="guardId">The engine identity of this guard expression.</param>
        /// <param name="activityName">The registered activity name containing the expression.</param>
        /// <param name="activityVersion">The registered version of the activity definition.</param>
        /// <param name="flowId">The identity of the flow whose guard pipeline contains the expression.</param>
        /// <param name="parentGuardId">The containing Boolean expression's identity, or <see langword="null"/> for a top-level expression.</param>
        /// <param name="kind">The expression kind, such as a leaf, conjunction, or disjunction.</param>
        /// <param name="order">The zero-based position among the top-level pipeline entries or within the parent expression.</param>
        /// <returns>A task completed when this visitor has finished handling the notification.</returns>
        Task FlowGuardExpressionAddedAsync(string guardId, string activityName, int activityVersion, string flowId, string? parentGuardId, ActivityGuardKind kind, int order) => Task.CompletedTask;

        /// <summary>Reports the concrete implementation type of a typed token-flow guard.</summary>
        /// <typeparam name="TToken">The CLR token type considered by the guard.</typeparam>
        /// <typeparam name="TGuard">The concrete guard implementation type.</typeparam>
        /// <param name="guardId">The engine identity of the guard registration.</param>
        /// <param name="activityName">The registered activity name containing the guard.</param>
        /// <param name="activityVersion">The registered version of the activity definition.</param>
        /// <returns>A task completed when this visitor has finished handling the notification.</returns>
        Task FlowGuardTypeAddedAsync<TToken, TGuard>(string guardId, string activityName, int activityVersion) where TGuard : class, IFlowGuard<TToken> => Task.CompletedTask;

        /// <summary>Reports the concrete implementation type of a control-flow guard.</summary>
        /// <typeparam name="TGuard">The concrete control-flow guard implementation type.</typeparam>
        /// <param name="guardId">The engine identity of the guard registration.</param>
        /// <param name="activityName">The registered activity name containing the guard.</param>
        /// <param name="activityVersion">The registered version of the activity definition.</param>
        /// <returns>A task completed when this visitor has finished handling the notification.</returns>
        Task ControlFlowGuardTypeAddedAsync<TGuard>(string guardId, string activityName, int activityVersion) where TGuard : class, IControlFlowGuard => Task.CompletedTask;

        /// <summary>Reports a token transformation registered in a flow pipeline.</summary>
        /// <typeparam name="TToken">The input CLR token type accepted by the transformation.</typeparam>
        /// <typeparam name="TTransformedToken">The output CLR token type produced by the transformation.</typeparam>
        /// <param name="transformationId">The engine identity of this transformation registration.</param>
        /// <param name="activityName">The registered activity name containing the transformation.</param>
        /// <param name="activityVersion">The registered version of the activity definition.</param>
        /// <param name="flowId">The identity of the flow whose pipeline contains the transformation.</param>
        /// <param name="order">The zero-based position of the transformation in the flow pipeline.</param>
        /// <returns>A task completed when this visitor has finished handling the notification.</returns>
        Task TransformationFlowAddedAsync<TToken, TTransformedToken>(string transformationId, string activityName, int activityVersion, string flowId, int order) => Task.CompletedTask;

        /// <summary>Reports an implementation type registered for a flow token transformation.</summary>
        /// <typeparam name="TToken">The input CLR token type accepted by the transformation.</typeparam>
        /// <typeparam name="TTransformedToken">The output CLR token type produced by the transformation.</typeparam>
        /// <typeparam name="TTransformation">The concrete transformation implementation type.</typeparam>
        /// <param name="transformationId">The engine identity of the transformation registration.</param>
        /// <param name="activityName">The registered activity name containing the transformation.</param>
        /// <param name="activityVersion">The registered version of the activity definition.</param>
        /// <returns>A task completed when this visitor has finished handling the notification.</returns>
        Task TransformationFlowTypeAddedAsync<TToken, TTransformedToken, TTransformation>(string transformationId, string activityName, int activityVersion) where TTransformation : class, IFlowTransformation<TToken, TTransformedToken> => Task.CompletedTask;

        /// <summary>Reports payload token names declared by an activity element.</summary>
        Task TokenNamesDeclaredAsync(string elementId, string activityName, int activityVersion,
            TokenNamesDeclarationKind kind, IReadOnlyList<string> tokenNames) => Task.CompletedTask;
    }
}
