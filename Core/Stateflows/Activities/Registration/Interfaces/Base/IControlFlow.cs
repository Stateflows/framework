namespace Stateflows.Activities.Registration.Interfaces.Base
{
    public interface IControlFlow<out TReturn>
    {
        TReturn AddControlFlow(string targetNodeName, ControlFlowBuildAction buildAction = null);
    }

    public interface IElseControlFlow<out TReturn>
    {
        TReturn AddElseControlFlow(string targetNodeName, ElseControlFlowBuildAction buildAction = null);
    }

    public interface IDecisionFlow<out TReturn>
    {
        TReturn AddFlow(string targetNodeName, ControlFlowBuildAction buildAction = null);
    }

    public interface IElseDecisionFlow<out TReturn>
    {
        TReturn AddElseFlow(string targetNodeName, ElseControlFlowBuildAction buildAction = null);
    }

    public interface IControlFlow
    {
        void AddControlFlow(string targetNodeName, ControlFlowBuildAction buildAction = null);
    }
}
