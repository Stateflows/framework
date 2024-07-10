namespace Stateflows.Activities.Registration.Interfaces.Base
{
    public interface IControlFlowBase<out TReturn>
    {
        TReturn AddControlFlow(string targetNodeName, ControlFlowBuildAction buildAction = null);
    }

    public interface IElseControlFlowBase<out TReturn>
    {
        TReturn AddElseControlFlow(string targetNodeName, ElseControlFlowBuildAction buildAction = null);
    }

    public interface IDecisionFlowBase<out TReturn>
    {
        TReturn AddFlow(string targetNodeName, ControlFlowBuildAction buildAction = null);
    }

    public interface IElseDecisionFlowBase<out TReturn>
    {
        TReturn AddElseFlow(string targetNodeName, ElseControlFlowBuildAction buildAction = null);
    }

    public interface IControlFlowBase
    {
        void AddControlFlow(string targetNodeName, ControlFlowBuildAction buildAction = null);
    }
}
