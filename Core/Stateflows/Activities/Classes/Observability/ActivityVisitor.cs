using System.Threading.Tasks;

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

        // public virtual Task ElseControlFlowAddedAsync(string activityName, int activityVersion, string sourceNodeName, string targetNodeName)
        //     => Task.CompletedTask;

        public virtual Task FlowAddedAsync<TToken>(string activityName, int activityVersion, string sourceNodeName, string targetNodeName, bool isElse = false)
            => Task.CompletedTask;

        // public virtual Task TransformationFlowAddedAsync<TToken, TTransformedToken>(string activityName, int activityVersion,
        //     string sourceNodeName, string targetNodeName)
        //     => Task.CompletedTask;
        //
        // public virtual Task ElseFlowAddedAsync<TToken>(string activityName, int activityVersion, string sourceNodeName, string targetNodeName)
        //     => Task.CompletedTask;
        //
        // public virtual Task ElseTransformationFlowAddedAsync<TToken, TTransformedToken>(string activityName, int activityVersion,
        //     string sourceNodeName, string targetNodeName)
        //     => Task.CompletedTask;
        //
        // public virtual Task ControlFlowGuardTypeAddedAsync<TGuard>(string activityName, int activityVersion, string sourceNodeName,
        //     string targetNodeName = null)
        //     where TGuard : class, IControlFlowGuard
        //     => Task.CompletedTask;
        //
        // public virtual Task FlowGuardTypeAddedAsync<TToken, TGuard>(string activityName, int activityVersion, string sourceNodeName,
        //     string targetNodeName = null)
        //     where TGuard : class, IFlowGuard<TToken>
        //     => Task.CompletedTask;
        //
        // public virtual Task FlowTransformationTypeAddedAsync<TToken, TTransformedToken, TTransformation>(string activityName,
        //     int activityVersion, string sourceNodeName, string targetNodeName = null)
        //     where TTransformation : class, IFlowTransformation<TToken, TTransformedToken>
        //     => Task.CompletedTask;
    }
}