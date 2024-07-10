using Stateflows.Activities.Extensions;

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

        TReturn AddFlow<TTransformedToken, TTransformationFlow>(string targetNodeName)
            where TTransformationFlow : TransformationFlow<TToken, TTransformedToken>
        {
            (this as IInternal).Services.RegisterTransformationFlow<TTransformationFlow, TToken, TTransformedToken>();

            return AddFlow(
                targetNodeName,
                b => b.AddTransformationFlowEvents<TTransformationFlow, TToken, TTransformedToken>()
            );
        }

        TReturn AddFlow<TTransformedToken, TTransformationFlow, TTargetNode>()
            where TTransformationFlow : TransformationFlow<TToken, TTransformedToken>
            where TTargetNode : ActivityNode
            => AddFlow<TTransformedToken, TTransformationFlow>(ActivityNodeInfo<TTargetNode>.Name);
    }

    public interface IElseDecisionFlowBase<TToken, out TReturn>
    {
        TReturn AddElseFlow(string targetNodeName, ElseObjectFlowBuildAction<TToken> buildAction = null);

        TReturn AddElseFlow<TTargetNode>(ObjectFlowBuildAction<TToken> buildAction = null)
            where TTargetNode : ActivityNode
            => AddElseFlow(ActivityNodeInfo<TTargetNode>.Name, b => buildAction?.Invoke(b as IObjectFlowBuilder<TToken>));

        //TReturn AddElseFlow<TFlow>(string targetNodeName)
        //    where TFlow : Flow<TToken>
        //{
        //    (this as IInternal).Services.RegisterObjectFlow<TFlow, TToken>();

        //    return AddElseFlow(
        //        targetNodeName,
        //        b => (b as IObjectFlowBuilder<TToken>).AddObjectFlowEvents<TFlow, TToken>()
        //    );
        //}

        //TReturn AddElseFlow<TFlow, TTargetNode>()
        //    where TFlow : Flow<TToken>
        //    where TTargetNode : ActivityNode
        //    => AddElseFlow<TFlow>(ActivityNodeInfo<TTargetNode>.Name);

        TReturn AddElseFlow<TTransformedToken, TElseTransformationFlow>(string targetNodeName)
            where TElseTransformationFlow : ElseTransformationFlow<TToken, TTransformedToken>
        {
            (this as IInternal).Services.RegisterElseTransformationFlow<TElseTransformationFlow, TToken, TTransformedToken>();

            return AddElseFlow(
                targetNodeName,
                b => (b as IObjectFlowBuilder<TToken>).AddElseTransformationFlowEvents<TElseTransformationFlow, TToken, TTransformedToken>()
            );
        }

        TReturn AddElseFlow<TTransformedToken, TElseTransformationFlow, TTargetNode>()
            where TElseTransformationFlow : ElseTransformationFlow<TToken, TTransformedToken>
            where TTargetNode : ActivityNode
            => AddElseFlow<TTransformedToken, TElseTransformationFlow>(ActivityNodeInfo<TTargetNode>.Name);
    }

    public interface IObjectFlowBase
    {
        void AddFlow<TToken>(string targetNodeName, ObjectFlowBuildAction<TToken> buildAction = null);
    }
}
