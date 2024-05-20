using Stateflows.Activities.Extensions;
using Stateflows.Activities.Typed;

namespace Stateflows.Activities.Registration.Interfaces.Base
{
    public interface IObjectFlow<out TReturn>
    {
        TReturn AddFlow<TToken>(string targetNodeName, ObjectFlowBuildAction<TToken> buildAction = null);
    }

    public interface IElseObjectFlow<out TReturn>
    {
        TReturn AddElseFlow<TToken>(string targetNodeName, ElseObjectFlowBuildAction<TToken> buildAction = null);
    }

    public interface IDecisionFlow<TToken, out TReturn>
    {
        TReturn AddFlow(string targetNodeName, ObjectFlowBuildAction<TToken> buildAction = null);

        TReturn AddFlow<TTargetNode>(ObjectFlowBuildAction<TToken> buildAction = null)
            where TTargetNode : ActivityNode
            => AddFlow(ActivityNodeInfo<TTargetNode>.Name, buildAction);

        TReturn AddFlow<TFlow>(string targetNodeName)
            where TFlow : Flow<TToken>
        {
            (this as IInternal).Services.RegisterObjectFlow<TFlow, TToken>();

            return AddFlow(
                targetNodeName,
                b => b.AddObjectFlowEvents<TFlow, TToken>()
            );
        }

        TReturn AddFlow<TFlow, TTargetNode>()
            where TFlow : Flow<TToken>
            where TTargetNode : ActivityNode
            => AddFlow<TFlow>(ActivityNodeInfo<TTargetNode>.Name);

        TReturn AddFlow<TTransformedToken, TObjectTransformationFlow>(string targetNodeName)
            where TObjectTransformationFlow : TransformationFlow<TToken, TTransformedToken>
        {
            (this as IInternal).Services.RegisterObjectTransformationFlow<TObjectTransformationFlow, TToken, TTransformedToken>();

            return AddFlow(
                targetNodeName,
                b => b.AddObjectTransformationFlowEvents<TObjectTransformationFlow, TToken, TTransformedToken>()
            );
        }

        TReturn AddFlow<TTransformedToken, TObjectTransformationFlow, TTargetNode>()
            where TObjectTransformationFlow : TransformationFlow<TToken, TTransformedToken>
            where TTargetNode : ActivityNode
            => AddFlow<TTransformedToken, TObjectTransformationFlow>(ActivityNodeInfo<TTargetNode>.Name);
    }

    public interface IElseDecisionFlow<TToken, out TReturn>
    {
        TReturn AddElseFlow(string targetNodeName, ElseObjectFlowBuildAction<TToken> buildAction = null);

        TReturn AddElseFlow<TTargetNode>(ObjectFlowBuildAction<TToken> buildAction = null)
            where TTargetNode : ActivityNode
            => AddElseFlow(ActivityNodeInfo<TTargetNode>.Name, b => buildAction?.Invoke(b as IObjectFlowBuilder<TToken>));

        TReturn AddElseFlow<TFlow>(string targetNodeName)
            where TFlow : Flow<TToken>
        {
            (this as IInternal).Services.RegisterObjectFlow<TFlow, TToken>();

            return AddElseFlow(
                targetNodeName,
                b => (b as IObjectFlowBuilder<TToken>).AddObjectFlowEvents<TFlow, TToken>()
            );
        }

        TReturn AddElseFlow<TFlow, TTargetNode>()
            where TFlow : Flow<TToken>
            where TTargetNode : ActivityNode
            => AddElseFlow<TFlow>(ActivityNodeInfo<TTargetNode>.Name);

        TReturn AddElseFlow<TTransformedToken, TObjectTransformationFlow>(string targetNodeName)
            where TObjectTransformationFlow : TransformationFlow<TToken, TTransformedToken>
        {
            (this as IInternal).Services.RegisterObjectTransformationFlow<TObjectTransformationFlow, TToken, TTransformedToken>();

            return AddElseFlow(
                targetNodeName,
                b => (b as IObjectFlowBuilder<TToken>).AddObjectTransformationFlowEvents<TObjectTransformationFlow, TToken, TTransformedToken>()
            );
        }

        TReturn AddElseFlow<TTransformedToken, TObjectTransformationFlow, TTargetNode>()
            where TObjectTransformationFlow : TransformationFlow<TToken, TTransformedToken>
            where TTargetNode : ActivityNode
            => AddElseFlow<TTransformedToken, TObjectTransformationFlow>(ActivityNodeInfo<TTargetNode>.Name);
    }

    public interface IObjectFlow
    {
        void AddFlow<TToken>(string targetNodeName, ObjectFlowBuildAction<TToken> buildAction = null);
    }
}
