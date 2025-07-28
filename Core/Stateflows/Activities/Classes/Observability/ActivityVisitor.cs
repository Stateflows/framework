using System.Threading.Tasks;
using Stateflows.Common;

namespace Stateflows.Activities
{
    public abstract class ActivityVisitor : IActivityVisitor
    {
        public virtual Task ActivityAddedAsync(string activityName, int activityVersion)
            => Task.CompletedTask;

        public virtual Task ActivityTypeAddedAsync<TActivity>(string activityName, int activityVersion)
            where TActivity : class, IActivity
            => Task.CompletedTask;

        public virtual Task InitializerAddedAsync<TInitializationEvent>(string activityName, int activityVersion)
            => Task.CompletedTask;

        public virtual Task DefaultInitializerAddedAsync(string activityName, int activityVersion)
            => Task.CompletedTask;

        public virtual Task FinalizerAddedAsync(string activityName, int activityVersion)
            => Task.CompletedTask;

        public virtual Task NodeAddedAsync(string activityName, int activityVersion, string nodeName, NodeType nodeType,
            string parentNodeName = null)
            => Task.CompletedTask;

        public virtual Task NodeTypeAddedAsync<TNode>(string activityName, int activityVersion, string nodeName)
            where TNode : class, IActivityNode
            => Task.CompletedTask;

        public virtual Task AcceptEventNodeAddedAsync<TEvent>(string activityName, int activityVersion, string nodeName,
            string parentNodeName = null)
            => Task.CompletedTask;

        public virtual Task AcceptEventNodeTypeAddedAsync<TEvent, TAcceptEventAction>(string activityName, int activityVersion, string nodeName)
            where TAcceptEventAction : class, IAcceptEventActionNode<TEvent>
            => Task.CompletedTask;

        public virtual Task SendEventNodeAddedAsync<TEvent>(string activityName, int activityVersion, string nodeName,
            string parentNodeName = null)
            => Task.CompletedTask;

        public virtual Task SendEventNodeTypeAddedAsync<TEvent, TSendEventAction>(string activityName, int activityVersion, string nodeName)
            where TSendEventAction : class, ISendEventActionNode<TEvent>
            => Task.CompletedTask;

        public virtual Task ControlFlowAddedAsync(string activityName, int activityVersion, string sourceNodeName, string targetNodeName, bool isElse = false)
            => Task.CompletedTask;

        public virtual Task ControlFlowTypeAddedAsync<TControlFlow>(string activityName, int activityVersion, string sourceNodeName,
            string targetNodeName, bool isElse = false) where TControlFlow : class, IControlFlow
            => Task.CompletedTask;

        public virtual Task FlowAddedAsync<TToken>(string activityName, int activityVersion, string sourceNodeName, string targetNodeName, bool isElse = false)
            => Task.CompletedTask;

        public virtual Task FlowTypeAddedAsync<TToken, TFlow>(string activityName, int activityVersion, string sourceNodeName,
            string targetNodeName, bool isElse = false) where TFlow : class, IFlow<TToken>
            => Task.CompletedTask;

        public virtual Task TransformationFlowAddedAsync<TToken, TTransformedToken>(string activityName, int activityVersion,
            string sourceNodeName, string targetNodeName, bool isElse = false)
            => Task.CompletedTask;

        public virtual Task TransformationFlowTypeAddedAsync<TToken, TTransformedToken, TFlowTransformation>(string activityName,
            int activityVersion, string sourceNodeName, string targetNodeName, bool isElse = false) where TFlowTransformation : class, IFlowTransformation<TToken, TTransformedToken>
            => Task.CompletedTask;

        public virtual Task CustomEventAddedAsync<TEvent>(string activityName, int activityVersion, BehaviorStatus[] supportedStatuses)
            => Task.CompletedTask;
    }
}