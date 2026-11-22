using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Entities.Enums;

namespace Stateflows.Activities
{
    /// <summary>No-op base class for finalized definition visitors.</summary>
    public abstract class ActivityVisitor : IActivityVisitor
    {
        public virtual Task ActivityAddingAsync(string activityId, string activityName, int activityVersion, BehaviorClass? ownerClass = null, BehaviorClass? parentClass = null, bool hasDefaultInstance = false) => Task.CompletedTask;
        public virtual Task ActivityAddedAsync(string activityId, string activityName, int activityVersion) => Task.CompletedTask;
        public virtual Task ActivityTypeAddedAsync<TActivity>(string activityId, string activityName, int activityVersion) where TActivity : class, IActivity => Task.CompletedTask;
        public virtual Task InitializerAddedAsync<TEvent>(string initializerId, string activityName, int activityVersion, IReadOnlyList<string> eventNames, int order) => Task.CompletedTask;
        public virtual Task InitializerTypeAddedAsync<TEvent, TInitializer>(string initializerId, string activityName, int activityVersion) where TInitializer : class, IInitializer<TEvent> => Task.CompletedTask;
        public virtual Task DefaultInitializerAddedAsync(string initializerId, string activityName, int activityVersion, int order) => Task.CompletedTask;
        public virtual Task DefaultInitializerTypeAddedAsync<TInitializer>(string initializerId, string activityName, int activityVersion) where TInitializer : class, IDefaultInitializer => Task.CompletedTask;
        public virtual Task FinalizerAddedAsync(string finalizerId, string activityName, int activityVersion, int order) => Task.CompletedTask;
        public virtual Task FinalizerTypeAddedAsync<TFinalizer>(string finalizerId, string activityName, int activityVersion) where TFinalizer : class, IFinalizer => Task.CompletedTask;
        public virtual Task NodeAddedAsync(string nodeId, string activityName, int activityVersion, string nodeName, NodeType nodeType, string? parentNodeId) => Task.CompletedTask;
        public virtual Task NodeTypeAddedAsync<TNode>(string nodeId, string activityName, int activityVersion) where TNode : class, IActivityNode => Task.CompletedTask;
        public virtual Task NodeConfigurationAddedAsync(string nodeId, string activityName, int activityVersion, NodeOptions options, int chunkSize, int maxConcurrency, bool anchored, PublishScope publishScope, int timeToLive, bool retain) => Task.CompletedTask;
        public virtual Task InputPinAddedAsync<TToken>(string pinId, string activityName, int activityVersion, string? parentNodeId) => Task.CompletedTask;
        public virtual Task OutputPinAddedAsync<TToken>(string pinId, string activityName, int activityVersion, string? parentNodeId) => Task.CompletedTask;
        public virtual Task AcceptEventNodeAddedAsync<TEvent>(string nodeId, string activityName, int activityVersion, IReadOnlyList<string> eventNames) => Task.CompletedTask;
        public virtual Task AcceptEventNodeTypeAddedAsync<TEvent, TNode>(string nodeId, string activityName, int activityVersion) where TNode : class, IAcceptEventActionNode<TEvent> => Task.CompletedTask;
        public virtual Task TimeEventNodeTypeAddedAsync<TNode>(string nodeId, string activityName, int activityVersion) where TNode : class, ITimeEventActionNode => Task.CompletedTask;
        public virtual Task SendEventNodeAddedAsync<TEvent>(string nodeId, string activityName, int activityVersion, IReadOnlyList<string> eventNames) => Task.CompletedTask;
        public virtual Task SendEventNodeTypeAddedAsync<TEvent, TNode>(string nodeId, string activityName, int activityVersion) where TNode : class, ISendEventActionNode<TEvent> => Task.CompletedTask;
        public virtual Task PublishEventNodeAddedAsync<TEvent>(string nodeId, string activityName, int activityVersion, IReadOnlyList<string> eventNames) => Task.CompletedTask;
        public virtual Task PublishEventNodeTypeAddedAsync<TEvent, TNode>(string nodeId, string activityName, int activityVersion) where TNode : class, IPublishEventActionNode<TEvent> => Task.CompletedTask;
        public virtual Task ControlFlowAddedAsync(string flowId, string activityName, int activityVersion, string sourceNodeId, string targetNodeId, bool isElse) => Task.CompletedTask;
        public virtual Task ControlFlowTypeAddedAsync<TFlow>(string flowId, string activityName, int activityVersion) where TFlow : class, IControlFlow => Task.CompletedTask;
        public virtual Task FlowAddedAsync<TToken>(string flowId, string activityName, int activityVersion, string sourceNodeId, string targetNodeId, bool isElse) => Task.CompletedTask;
        public virtual Task FlowTypeAddedAsync<TToken, TFlow>(string flowId, string activityName, int activityVersion) where TFlow : class, IFlow<TToken> => Task.CompletedTask;
        public virtual Task FlowConfigurationAddedAsync(string flowId, string activityName, int activityVersion, Type targetTokenType, int weight, bool isCrossScope, string ownerNodeId) => Task.CompletedTask;
        public virtual Task NodeOperationAddedAsync(string operationId, string activityName, int activityVersion, string nodeId, ActivityOperationKind kind, int order) => Task.CompletedTask;
        public virtual Task NodeOperationTypeAddedAsync<TImplementation>(string operationId, string activityName, int activityVersion) where TImplementation : class => Task.CompletedTask;
        public virtual Task FlowGuardAddedAsync<TToken>(string guardId, string activityName, int activityVersion, string flowId, int order) => Task.CompletedTask;
        public virtual Task FlowGuardExpressionAddedAsync(string guardId, string activityName, int activityVersion, string flowId, string? parentGuardId, ActivityGuardKind kind, int order) => Task.CompletedTask;
        public virtual Task FlowGuardTypeAddedAsync<TToken, TGuard>(string guardId, string activityName, int activityVersion) where TGuard : class, IFlowGuard<TToken> => Task.CompletedTask;
        public virtual Task ControlFlowGuardTypeAddedAsync<TGuard>(string guardId, string activityName, int activityVersion) where TGuard : class, IControlFlowGuard => Task.CompletedTask;
        public virtual Task TransformationFlowAddedAsync<TToken, TTransformedToken>(string transformationId, string activityName, int activityVersion, string flowId, int order) => Task.CompletedTask;
        public virtual Task TransformationFlowTypeAddedAsync<TToken, TTransformedToken, TTransformation>(string transformationId, string activityName, int activityVersion) where TTransformation : class, IFlowTransformation<TToken, TTransformedToken> => Task.CompletedTask;
    }
}
