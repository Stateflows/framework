using System.Diagnostics;
using Stateflows.Activities.Extensions;

namespace Stateflows.Activities.Registration.Interfaces.Base
{
    public interface IObjectFlowBase<out TReturn>
    {
        TReturn AddFlow<TToken>(string targetNodeName, ObjectFlowBuildAction<TToken> buildAction = null);

        TReturn AddFlow<TToken, TTargetNode>(ObjectFlowBuildAction<TToken> buildAction = null)
            where TTargetNode : class, IActivityNode
            => AddFlow(ActivityNode<TTargetNode>.Name, buildAction);

        TReturn AddFlow<TToken, TFlow>(string targetNodeName)
            where TFlow : class, IFlow<TToken>
            => AddFlow<TToken>(
                targetNodeName,
                b => b.AddObjectFlowEvents<TFlow, TToken>()
            );

        TReturn AddFlow<TToken, TFlow, TTargetNode>()
            where TFlow : class, IFlow<TToken>
            where TTargetNode : class, IActivityNode
            => AddFlow<TToken, TFlow>(ActivityNode<TTargetNode>.Name);

        TReturn AddFlow<TToken, TTransformedToken, TTransformationFlow>(string targetNodeName)
            where TTransformationFlow : class, IFlowTransformation<TToken, TTransformedToken>
            => AddFlow<TToken>(
                targetNodeName,
                b => b.AddObjectTransformationFlowEvents<TTransformationFlow, TToken, TTransformedToken>()
            );

        TReturn AddFlow<TToken, TTransformedToken, TTransformationFlow, TTargetNode>()
            where TTransformationFlow : class, IFlowTransformation<TToken, TTransformedToken>
            where TTargetNode : class, IActivityNode
            => AddFlow<TToken, TTransformedToken, TTransformationFlow>(ActivityNode<TTargetNode>.Name);
    }

    public interface IElseObjectFlowBase<out TReturn>
    {
        TReturn AddElseFlow<TToken>(string targetNodeName, ElseObjectFlowBuildAction<TToken> buildAction = null);
    }

    public interface IDecisionFlowBase<TToken, out TReturn>
    {
        TReturn AddFlow(string targetNodeName, ObjectFlowBuildAction<TToken> buildAction = null);

        TReturn AddFlow<TTargetNode>(ObjectFlowBuildAction<TToken> buildAction = null)
            where TTargetNode : class, IActivityNode
            => AddFlow(ActivityNode<TTargetNode>.Name, buildAction);

        TReturn AddFlow<TFlow>(string targetNodeName)
            where TFlow : class, IFlow<TToken>
            => AddFlow(
                targetNodeName,
                b => b.AddObjectFlowEvents<TFlow, TToken>()
            );

        TReturn AddFlow<TFlow, TTargetNode>()
            where TFlow : class, IFlow<TToken>
            where TTargetNode : class, IActivityNode
            => AddFlow<TFlow>(ActivityNode<TTargetNode>.Name);

        TReturn AddFlow<TTransformedToken, TTransformationFlow>(string targetNodeName)
            where TTransformationFlow : class, IFlowTransformation<TToken, TTransformedToken>
            => AddFlow(
                targetNodeName,
                b => b.AddObjectTransformationFlowEvents<TTransformationFlow, TToken, TTransformedToken>()
            );

        TReturn AddFlow<TTransformedToken, TTransformationFlow, TTargetNode>()
            where TTransformationFlow : class, IFlowTransformation<TToken, TTransformedToken>
            where TTargetNode : class, IActivityNode
            => AddFlow<TTransformedToken, TTransformationFlow>(ActivityNode<TTargetNode>.Name);
    }

    public interface IElseDecisionFlowBase<TToken, out TReturn>
    {
        TReturn AddElseFlow(string targetNodeName, ElseObjectFlowBuildAction<TToken> buildAction = null);

        TReturn AddElseFlow<TTargetNode>(ObjectFlowBuildAction<TToken> buildAction = null)
            where TTargetNode : class, IActivityNode
            => AddElseFlow(ActivityNode<TTargetNode>.Name, b => buildAction?.Invoke(b as IObjectFlowBuilder<TToken>));

        TReturn AddElseFlow<TTransformedToken, TElseTransformationFlow>(string targetNodeName)
            where TElseTransformationFlow : class, IFlowTransformation<TToken, TTransformedToken>
            => AddElseFlow(
                targetNodeName,
                b => (b as IObjectFlowBuilder<TToken>).AddElseObjectTransformationFlowEvents<TElseTransformationFlow, TToken, TTransformedToken>()
            );

        TReturn AddElseFlow<TTransformedToken, TElseTransformationFlow, TTargetNode>()
            where TElseTransformationFlow : class, IFlowTransformation<TToken, TTransformedToken>
            where TTargetNode : class, IActivityNode
            => AddElseFlow<TTransformedToken, TElseTransformationFlow>(ActivityNode<TTargetNode>.Name);
    }

    public interface IObjectFlowBase
    {
        void AddFlow<TToken>(string targetNodeName, ObjectFlowBuildAction<TToken> buildAction = null);
        
        [DebuggerHidden]
        public void AddFlow<TToken, TTargetNode>(ObjectFlowBuildAction<TToken> buildAction = null)
            where TTargetNode : class, IActivityNode
            => AddFlow<TToken>(ActivityNode<TTargetNode>.Name, buildAction);

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

        [DebuggerHidden]
        public void AddFlow<TToken, TFlow, TTargetNode>(ObjectFlowBuildAction<TToken> buildAction = null)
            where TFlow : class, IFlow<TToken>
            where TTargetNode : class, IActivityNode
            => AddFlow<TToken, TFlow>(ActivityNode<TTargetNode>.Name, buildAction);

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

        [DebuggerHidden]
        public void AddFlow<TToken, TTransformedToken, TTransformationFlow, TTargetNode>(ObjectFlowBuildAction<TTransformedToken> buildAction = null)
            where TTransformationFlow : class, IFlowTransformation<TToken, TTransformedToken>
            where TTargetNode : class, IActivityNode
            => AddFlow<TToken, TTransformedToken, TTransformationFlow>(ActivityNode<TTargetNode>.Name, buildAction);
    }
}
