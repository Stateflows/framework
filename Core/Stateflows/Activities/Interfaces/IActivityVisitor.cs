using System.Threading.Tasks;
using Stateflows.Common;

namespace Stateflows.Activities
{
    /// <summary>Receives notifications about registered activity definitions, nodes, flows, and custom events.</summary>
    /// <remarks>Use visitors to integrate registration metadata with external tooling without participating in runtime execution.</remarks>
    public interface IActivityVisitor
    {
        /// <summary>Signals that a structurally accepted event node has been registered.</summary>
        /// <param name="definition">The event definition.</param>
        /// <param name="activityName">The activity name.</param>
        /// <param name="activityVersion">The activity version.</param>
        /// <param name="nodeName">The event node name.</param>
        /// <param name="parentNodeName">The containing structured node, if any.</param>
        Task AcceptEventNodeAddedAsync(EventDefinition definition, string activityName, int activityVersion,
            string nodeName, string parentNodeName = null) => Task.CompletedTask;

        /// <summary>Signals that an activity registration is about to be visited.</summary>
        /// <param name="activityName">The activity name.</param>
        /// <param name="activityVersion">The activity version.</param>
        /// <param name="ownerClass">The owning behavior class, if any.</param>
        /// <param name="parentClass">The parent behavior class, if any.</param>
        /// <param name="hasDefaultInstance">Whether a default instance is registered.</param>
        Task ActivityAddingAsync(string activityName, int activityVersion, BehaviorClass? ownerClass = null, BehaviorClass? parentClass = null, bool hasDefaultInstance = false);

        /// <summary>Signals that an activity registration has been visited.</summary>
        /// <param name="activityName">The activity name.</param>
        /// <param name="activityVersion">The activity version.</param>
        Task ActivityAddedAsync(string activityName, int activityVersion);

        /// <summary>Signals that a typed activity implementation is registered.</summary>
        /// <typeparam name="TActivity">The activity implementation type.</typeparam>
        /// <param name="activityName">The activity name.</param>
        /// <param name="activityVersion">The activity version.</param>
        Task ActivityTypeAddedAsync<TActivity>(string activityName, int activityVersion)
            where TActivity : class, IActivity;

        /// <summary>Signals registration of an initializer for a typed request event.</summary>
        /// <typeparam name="TInitializationEvent">The initialization request type.</typeparam>
        /// <param name="activityName">The activity name.</param>
        /// <param name="activityVersion">The activity version.</param>
        Task InitializerAddedAsync<TInitializationEvent>(string activityName, int activityVersion);

        /// <summary>Signals registration of a default initializer.</summary>
        /// <param name="activityName">The activity name.</param>
        /// <param name="activityVersion">The activity version.</param>
        Task DefaultInitializerAddedAsync(string activityName, int activityVersion);
        
        /// <summary>Signals registration of an activity finalizer.</summary>
        /// <param name="activityName">The activity name.</param>
        /// <param name="activityVersion">The activity version.</param>
        Task FinalizerAddedAsync(string activityName, int activityVersion);
        
        /// <summary>Signals registration of an activity node.</summary>
        /// <param name="activityName">The activity name.</param>
        /// <param name="activityVersion">The activity version.</param>
        /// <param name="nodeName">The node name.</param>
        /// <param name="nodeType">The kind of node registered.</param>
        /// <param name="parentNodeName">The containing structured node, if any.</param>
        Task NodeAddedAsync(string activityName, int activityVersion, string nodeName, NodeType nodeType, string parentNodeName = null);
        
        /// <summary>Signals registration of a typed activity node.</summary>
        /// <typeparam name="TNode">The node implementation type.</typeparam>
        /// <param name="activityName">The activity name.</param>
        /// <param name="activityVersion">The activity version.</param>
        /// <param name="nodeName">The node name.</param>
        Task NodeTypeAddedAsync<TNode>(string activityName, int activityVersion, string nodeName)
            where TNode : class, IActivityNode;
        
        /// <summary>Signals registration of an event-accepting node.</summary>
        /// <typeparam name="TEvent">The accepted event type.</typeparam>
        /// <param name="activityName">The activity name.</param>
        /// <param name="activityVersion">The activity version.</param>
        /// <param name="nodeName">The node name.</param>
        /// <param name="parentNodeName">The containing structured node, if any.</param>
        Task AcceptEventNodeAddedAsync<TEvent>(string activityName, int activityVersion, string nodeName, string parentNodeName = null);
        
        /// <summary>Signals registration of a typed event-accepting node.</summary>
        /// <typeparam name="TEvent">The accepted event type.</typeparam>
        /// <typeparam name="TAcceptEventAction">The node implementation type.</typeparam>
        /// <param name="activityName">The activity name.</param>
        /// <param name="activityVersion">The activity version.</param>
        /// <param name="nodeName">The node name.</param>
        Task AcceptEventNodeTypeAddedAsync<TEvent, TAcceptEventAction>(string activityName, int activityVersion, string nodeName)
            where TAcceptEventAction : class, IAcceptEventActionNode<TEvent>;
        
        /// <summary>Signals registration of a node that sends an event to a selected behavior.</summary>
        /// <typeparam name="TEvent">The event type sent.</typeparam>
        /// <param name="activityName">The activity name.</param>
        /// <param name="activityVersion">The activity version.</param>
        /// <param name="nodeName">The node name.</param>
        /// <param name="parentNodeName">The containing structured node, if any.</param>
        Task SendEventNodeAddedAsync<TEvent>(string activityName, int activityVersion, string nodeName, string parentNodeName = null);
        
        /// <summary>Signals registration of a typed send-event node.</summary>
        /// <typeparam name="TEvent">The event type sent.</typeparam>
        /// <typeparam name="TSendEventAction">The node implementation type.</typeparam>
        /// <param name="activityName">The activity name.</param>
        /// <param name="activityVersion">The activity version.</param>
        /// <param name="nodeName">The node name.</param>
        Task SendEventNodeTypeAddedAsync<TEvent, TSendEventAction>(string activityName, int activityVersion, string nodeName)
            where TSendEventAction : class, ISendEventActionNode<TEvent>;
        
        /// <summary>Signals registration of a node that publishes an event.</summary>
        /// <typeparam name="TEvent">The event type published.</typeparam>
        /// <param name="activityName">The activity name.</param>
        /// <param name="activityVersion">The activity version.</param>
        /// <param name="nodeName">The node name.</param>
        /// <param name="parentNodeName">The containing structured node, if any.</param>
        Task PublishEventNodeAddedAsync<TEvent>(string activityName, int activityVersion, string nodeName, string parentNodeName = null);
        
        /// <summary>Signals registration of a typed publish-event node.</summary>
        /// <typeparam name="TEvent">The event type published.</typeparam>
        /// <typeparam name="TPublishEventAction">The node implementation type.</typeparam>
        /// <param name="activityName">The activity name.</param>
        /// <param name="activityVersion">The activity version.</param>
        /// <param name="nodeName">The node name.</param>
        Task PublishEventNodeTypeAddedAsync<TEvent, TPublishEventAction>(string activityName, int activityVersion, string nodeName)
            where TPublishEventAction : class, IPublishEventActionNode<TEvent>;

        /// <summary>Signals registration of a control flow.</summary>
        /// <param name="activityName">The activity name.</param>
        /// <param name="activityVersion">The activity version.</param>
        /// <param name="sourceNodeName">The source node name.</param>
        /// <param name="targetNodeName">The target node name.</param>
        /// <param name="isElse">Whether this is the fallback else flow.</param>
        Task ControlFlowAddedAsync(string activityName, int activityVersion, string sourceNodeName, string targetNodeName, bool isElse = false);

        /// <summary>Signals registration of a typed control flow.</summary>
        /// <typeparam name="TControlFlow">The control-flow implementation type.</typeparam>
        /// <param name="activityName">The activity name.</param>
        /// <param name="activityVersion">The activity version.</param>
        /// <param name="sourceNodeName">The source node name.</param>
        /// <param name="targetNodeName">The target node name.</param>
        /// <param name="isElse">Whether this is the fallback else flow.</param>
        Task ControlFlowTypeAddedAsync<TControlFlow>(string activityName, int activityVersion, string sourceNodeName, string targetNodeName, bool isElse = false)
            where TControlFlow : class, IControlFlow;
        
        /// <summary>Signals registration of a typed object-token flow.</summary>
        /// <typeparam name="TToken">The token type carried by the flow.</typeparam>
        /// <param name="activityName">The activity name.</param>
        /// <param name="activityVersion">The activity version.</param>
        /// <param name="sourceNodeName">The source node name.</param>
        /// <param name="targetNodeName">The target node name.</param>
        /// <param name="isElse">Whether this is the fallback else flow.</param>
        Task FlowAddedAsync<TToken>(string activityName, int activityVersion, string sourceNodeName, string targetNodeName, bool isElse = false);

        /// <summary>Signals registration of a typed object-token flow implementation.</summary>
        /// <typeparam name="TToken">The token type carried by the flow.</typeparam>
        /// <typeparam name="TFlow">The flow implementation type.</typeparam>
        /// <param name="activityName">The activity name.</param>
        /// <param name="activityVersion">The activity version.</param>
        /// <param name="sourceNodeName">The source node name.</param>
        /// <param name="targetNodeName">The target node name.</param>
        /// <param name="isElse">Whether this is the fallback else flow.</param>
        Task FlowTypeAddedAsync<TToken, TFlow>(string activityName, int activityVersion, string sourceNodeName, string targetNodeName, bool isElse = false)
            where TFlow : class, IFlow<TToken>;
        
        /// <summary>Signals registration of a flow that transforms one token type to another.</summary>
        /// <typeparam name="TToken">The input token type.</typeparam>
        /// <typeparam name="TTransformedToken">The output token type.</typeparam>
        /// <param name="activityName">The activity name.</param>
        /// <param name="activityVersion">The activity version.</param>
        /// <param name="sourceNodeName">The source node name.</param>
        /// <param name="targetNodeName">The target node name.</param>
        /// <param name="isElse">Whether this is the fallback else flow.</param>
        Task TransformationFlowAddedAsync<TToken, TTransformedToken>(string activityName, int activityVersion, string sourceNodeName, string targetNodeName, bool isElse = false);

        /// <summary>Signals registration of a typed flow-transformation implementation.</summary>
        /// <typeparam name="TToken">The input token type.</typeparam>
        /// <typeparam name="TTransformedToken">The output token type.</typeparam>
        /// <typeparam name="TFlowTransformation">The transformation implementation type.</typeparam>
        /// <param name="activityName">The activity name.</param>
        /// <param name="activityVersion">The activity version.</param>
        /// <param name="sourceNodeName">The source node name.</param>
        /// <param name="targetNodeName">The target node name.</param>
        /// <param name="isElse">Whether this is the fallback else flow.</param>
        Task TransformationFlowTypeAddedAsync<TToken, TTransformedToken, TFlowTransformation>(string activityName, int activityVersion, string sourceNodeName, string targetNodeName, bool isElse = false)
            where TFlowTransformation : class, IFlowTransformation<TToken, TTransformedToken>;
        
        /// <summary>Signals registration of a typed custom event supported by the activity.</summary>
        /// <typeparam name="TEvent">The custom event type.</typeparam>
        /// <param name="activityName">The activity name.</param>
        /// <param name="activityVersion">The activity version.</param>
        /// <param name="supportedStatuses">The behavior statuses supported by the event.</param>
        Task CustomEventAddedAsync<TEvent>(string activityName, int activityVersion, BehaviorStatus[] supportedStatuses);

        // Task TransformationFlowAddedAsync<TToken, TTransformedToken>(string activityName, int activityVersion, string sourceNodeName, string targetNodeName);
        //
        // Task ElseFlowAddedAsync<TToken>(string activityName, int activityVersion, string sourceNodeName, string targetNodeName);
        //
        // Task ElseTransformationFlowAddedAsync<TToken, TTransformedToken>(string activityName, int activityVersion, string sourceNodeName, string targetNodeName);
        //
        // Task ControlFlowGuardTypeAddedAsync<TGuard>(string activityName, int activityVersion, string sourceNodeName, string targetNodeName = null)
        //     where TGuard : class, IControlFlowGuard;
        //
        // Task FlowGuardTypeAddedAsync<TToken, TGuard>(string activityName, int activityVersion, string sourceNodeName, string targetNodeName = null)
        //     where TGuard : class, IFlowGuard<TToken>;
        //
        // Task FlowTransformationTypeAddedAsync<TToken, TTransformedToken, TTransformation>(string activityName, int activityVersion, string sourceNodeName, string targetNodeName = null)
        //     where TTransformation : class, IFlowTransformation<TToken, TTransformedToken>;
    }
}
