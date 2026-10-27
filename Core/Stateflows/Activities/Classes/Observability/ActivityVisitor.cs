using System.Threading.Tasks;
using Stateflows.Common;

namespace Stateflows.Activities
{
    /// <summary>Convenience base class for activity visitors with no-op callbacks.</summary>
    /// <remarks>Override only the registration callbacks needed by an integration.</remarks>
    public abstract class ActivityVisitor : IActivityVisitor
    {
        /// <inheritdoc/>
        public virtual Task AcceptEventNodeAddedAsync(EventDefinition definition, string activityName, int activityVersion,
            string nodeName, string parentNodeName = null) => Task.CompletedTask;

        /// <inheritdoc/>
        public virtual Task ActivityAddingAsync(string activityName, int activityVersion, BehaviorClass? ownerClass = null, BehaviorClass? parentClass = null, bool hasDefaultInstance = false)
            => Task.CompletedTask;

        /// <inheritdoc/>
        public virtual Task ActivityAddedAsync(string activityName, int activityVersion)
            => Task.CompletedTask;

        /// <inheritdoc/>
        public virtual Task ActivityTypeAddedAsync<TActivity>(string activityName, int activityVersion)
            where TActivity : class, IActivity
            => Task.CompletedTask;

        /// <inheritdoc/>
        public virtual Task InitializerAddedAsync<TInitializationEvent>(string activityName, int activityVersion)
            => Task.CompletedTask;

        /// <inheritdoc/>
        public virtual Task DefaultInitializerAddedAsync(string activityName, int activityVersion)
            => Task.CompletedTask;

        /// <inheritdoc/>
        public virtual Task FinalizerAddedAsync(string activityName, int activityVersion)
            => Task.CompletedTask;

        /// <inheritdoc/>
        public virtual Task NodeAddedAsync(string activityName, int activityVersion, string nodeName, NodeType nodeType,
            string parentNodeName = null)
            => Task.CompletedTask;

        /// <inheritdoc/>
        public virtual Task NodeTypeAddedAsync<TNode>(string activityName, int activityVersion, string nodeName)
            where TNode : class, IActivityNode
            => Task.CompletedTask;

        /// <inheritdoc/>
        public virtual Task AcceptEventNodeAddedAsync<TEvent>(string activityName, int activityVersion, string nodeName,
            string parentNodeName = null)
            => Task.CompletedTask;

        /// <inheritdoc/>
        public virtual Task AcceptEventNodeTypeAddedAsync<TEvent, TAcceptEventAction>(string activityName, int activityVersion, string nodeName)
            where TAcceptEventAction : class, IAcceptEventActionNode<TEvent>
            => Task.CompletedTask;

        /// <inheritdoc/>
        public virtual Task SendEventNodeAddedAsync<TEvent>(string activityName, int activityVersion, string nodeName,
            string parentNodeName = null)
            => Task.CompletedTask;

        /// <inheritdoc/>
        public virtual Task SendEventNodeTypeAddedAsync<TEvent, TSendEventAction>(string activityName, int activityVersion, string nodeName)
            where TSendEventAction : class, ISendEventActionNode<TEvent>
            => Task.CompletedTask;

        /// <inheritdoc/>
        public virtual Task PublishEventNodeAddedAsync<TEvent>(string activityName, int activityVersion, string nodeName,
            string parentNodeName = null)
            => Task.CompletedTask;

        /// <inheritdoc/>
        public virtual Task PublishEventNodeTypeAddedAsync<TEvent, TPublishEventAction>(string activityName, int activityVersion,
            string nodeName) where TPublishEventAction : class, IPublishEventActionNode<TEvent>
            => Task.CompletedTask;

        /// <inheritdoc/>
        public virtual Task ControlFlowAddedAsync(string activityName, int activityVersion, string sourceNodeName, string targetNodeName, bool isElse = false)
            => Task.CompletedTask;

        /// <inheritdoc/>
        public virtual Task ControlFlowTypeAddedAsync<TControlFlow>(string activityName, int activityVersion, string sourceNodeName,
            string targetNodeName, bool isElse = false) where TControlFlow : class, IControlFlow
            => Task.CompletedTask;

        /// <inheritdoc/>
        public virtual Task FlowAddedAsync<TToken>(string activityName, int activityVersion, string sourceNodeName, string targetNodeName, bool isElse = false)
            => Task.CompletedTask;

        /// <inheritdoc/>
        public virtual Task FlowTypeAddedAsync<TToken, TFlow>(string activityName, int activityVersion, string sourceNodeName,
            string targetNodeName, bool isElse = false) where TFlow : class, IFlow<TToken>
            => Task.CompletedTask;

        /// <inheritdoc/>
        public virtual Task TransformationFlowAddedAsync<TToken, TTransformedToken>(string activityName, int activityVersion,
            string sourceNodeName, string targetNodeName, bool isElse = false)
            => Task.CompletedTask;

        /// <inheritdoc/>
        public virtual Task TransformationFlowTypeAddedAsync<TToken, TTransformedToken, TFlowTransformation>(string activityName,
            int activityVersion, string sourceNodeName, string targetNodeName, bool isElse = false) where TFlowTransformation : class, IFlowTransformation<TToken, TTransformedToken>
            => Task.CompletedTask;

        /// <inheritdoc/>
        public virtual Task CustomEventAddedAsync<TEvent>(string activityName, int activityVersion, BehaviorStatus[] supportedStatuses)
            => Task.CompletedTask;
    }
}