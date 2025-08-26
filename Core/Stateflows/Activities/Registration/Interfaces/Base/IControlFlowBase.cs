using System.Diagnostics;
using Stateflows.Activities.Extensions;

namespace Stateflows.Activities.Registration.Interfaces.Base
{
    public interface IControlFlowBase<out TReturn>
    {
        TReturn AddControlFlow(string targetNodeName, ControlFlowBuildAction buildAction = null);
        
        [DebuggerHidden]
        public TReturn AddControlFlow<TTargetNode>(ControlFlowBuildAction buildAction = null)
            where TTargetNode : class, IActivityNode
            => AddControlFlow(ActivityNode<TTargetNode>.Name, buildAction);
        
        [DebuggerHidden]
        public TReturn AddControlFlow<TControlFlow>(string targetNodeName, ControlFlowBuildAction buildAction = null)
            where TControlFlow : class, IControlFlow
            => AddControlFlow(
                targetNodeName,
                b =>
                {
                    b.AddControlFlowEvents<TControlFlow>();
                    buildAction?.Invoke(b);
                }
            );

        [DebuggerHidden]
        public TReturn AddControlFlow<TFlow, TTargetNode>(ControlFlowBuildAction buildAction = null)
            where TFlow : class, IControlFlow
            where TTargetNode : class, IActivityNode
            => AddControlFlow<TFlow>(ActivityNode<TTargetNode>.Name, buildAction);
    }

    public interface IElseControlFlowBase<out TReturn>
    {
        TReturn AddElseControlFlow(string targetNodeName, ElseControlFlowBuildAction buildAction = null);
        
        [DebuggerHidden]
        public TReturn AddElseControlFlow<TTargetNode>(ElseControlFlowBuildAction buildAction = null)
            where TTargetNode : class, IActivityNode
            => AddElseControlFlow(ActivityNode<TTargetNode>.Name, buildAction);
        
        [DebuggerHidden]
        public TReturn AddElseControlFlow<TControlFlow>(string targetNodeName, ElseControlFlowBuildAction buildAction = null)
            where TControlFlow : class, IControlFlow
            => AddElseControlFlow(
                targetNodeName,
                b => buildAction?.Invoke(b)
            );

        [DebuggerHidden]
        public TReturn AddElseControlFlow<TFlow, TTargetNode>(ElseControlFlowBuildAction buildAction = null)
            where TFlow : class, IControlFlow
            where TTargetNode : class, IActivityNode
            => AddElseControlFlow<TFlow>(ActivityNode<TTargetNode>.Name, buildAction);
    }

    public interface IDecisionFlowBase<out TReturn>
    {
        TReturn AddFlow(string targetNodeName, ControlFlowBuildAction buildAction);
        
        [DebuggerHidden]
        public TReturn AddFlow<TTargetNode>(ControlFlowBuildAction buildAction = null)
            where TTargetNode : class, IActivityNode
            => AddFlow(ActivityNode<TTargetNode>.Name, buildAction);
        
        [DebuggerHidden]
        public TReturn AddFlow<TControlFlow>(string targetNodeName, ControlFlowBuildAction buildAction = null)
            where TControlFlow : class, IControlFlow
            => AddFlow(
                targetNodeName,
                b =>
                {
                    b.AddControlFlowEvents<TControlFlow>();
                    buildAction?.Invoke(b);
                }
            );

        [DebuggerHidden]
        public TReturn AddFlow<TFlow, TTargetNode>(ControlFlowBuildAction buildAction = null)
            where TFlow : class, IControlFlow
            where TTargetNode : class, IActivityNode
            => AddFlow<TFlow>(ActivityNode<TTargetNode>.Name, buildAction);
    }

    public interface IElseDecisionFlowBase<out TReturn>
    {
        TReturn AddElseFlow(string targetNodeName, ElseControlFlowBuildAction buildAction = null);
        
        [DebuggerHidden]
        public TReturn AddElseFlow<TTargetNode>(ElseControlFlowBuildAction buildAction = null)
            where TTargetNode : class, IActivityNode
            => AddElseFlow(ActivityNode<TTargetNode>.Name, buildAction);
        
        [DebuggerHidden]
        public TReturn AddElseFlow<TControlFlow>(string targetNodeName, ElseControlFlowBuildAction buildAction = null)
            where TControlFlow : class, IControlFlow
            => AddElseFlow(
                targetNodeName,
                b => buildAction?.Invoke(b)
            );

        [DebuggerHidden]
        public TReturn AddElseFlow<TFlow, TTargetNode>(ElseControlFlowBuildAction buildAction = null)
            where TFlow : class, IControlFlow
            where TTargetNode : class, IActivityNode
            => AddElseFlow<TFlow>(ActivityNode<TTargetNode>.Name, buildAction);
    }

    public interface IControlFlowBase
    {
        void AddControlFlow(string targetNodeName, ControlFlowBuildAction buildAction = null);
        
        [DebuggerHidden]
        public void AddControlFlow<TTargetNode>(ControlFlowBuildAction buildAction = null)
            where TTargetNode : class, IActivityNode
            => AddControlFlow(ActivityNode<TTargetNode>.Name, buildAction);
        
        [DebuggerHidden]
        public void AddControlFlow<TControlFlow>(string targetNodeName, ControlFlowBuildAction buildAction = null)
            where TControlFlow : class, IControlFlow
            => AddControlFlow(
                targetNodeName,
                b =>
                {
                    b.AddControlFlowEvents<TControlFlow>();
                    buildAction?.Invoke(b);
                }
            );

        [DebuggerHidden]
        public void AddControlFlow<TFlow, TTargetNode>(ControlFlowBuildAction buildAction = null)
            where TFlow : class, IControlFlow
            where TTargetNode : class, IActivityNode
            => AddControlFlow<TFlow>(ActivityNode<TTargetNode>.Name, buildAction);
    }
}
