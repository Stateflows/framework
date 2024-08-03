using Stateflows.Activities.Extensions;
using Stateflows.Common.Extensions;

namespace Stateflows.Activities.Registration.Interfaces.Base
{
    public interface IObjectFlowBase<out TReturn>
    {
        TReturn AddFlow<TToken>(string targetNodeName, ObjectFlowBuildAction<TToken> buildAction = null);
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
            where TFlow : class, IBaseFlow<TToken>
        {
            (this as IInternal).Services.AddServiceType<TFlow>();

            return AddFlow(
                targetNodeName,
                b => b.AddObjectFlowEvents<TFlow, TToken>()
            );
        }

        TReturn AddFlow<TFlow, TTargetNode>()
            where TFlow : class, IBaseFlow<TToken>
            where TTargetNode : class, IActivityNode
            => AddFlow<TFlow>(ActivityNode<TTargetNode>.Name);

        TReturn AddFlow<TTransformedToken, TTransformationFlow>(string targetNodeName)
            where TTransformationFlow : class, IFlowTransformation<TToken, TTransformedToken>
        {
            (this as IInternal).Services.AddServiceType<TTransformationFlow>();

            return AddFlow(
                targetNodeName,
                b => b.AddObjectTransformationFlowEvents<TTransformationFlow, TToken, TTransformedToken>()
            );
        }

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
        {
            (this as IInternal).Services.AddServiceType<TElseTransformationFlow>();

            return AddElseFlow(
                targetNodeName,
                b => (b as IObjectFlowBuilder<TToken>).AddElseObjectTransformationFlowEvents<TElseTransformationFlow, TToken, TTransformedToken>()
            );
        }

        TReturn AddElseFlow<TTransformedToken, TElseTransformationFlow, TTargetNode>()
            where TElseTransformationFlow : class, IFlowTransformation<TToken, TTransformedToken>
            where TTargetNode : class, IActivityNode
            => AddElseFlow<TTransformedToken, TElseTransformationFlow>(ActivityNode<TTargetNode>.Name);
    }

    public interface IObjectFlowBase
    {
        void AddFlow<TToken>(string targetNodeName, ObjectFlowBuildAction<TToken> buildAction = null);
    }
}
