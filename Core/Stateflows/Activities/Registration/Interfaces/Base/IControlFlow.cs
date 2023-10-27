namespace Stateflows.Activities.Registration.Interfaces.Base
{
    public interface IControlFlow<out TReturn>
    {
        TReturn AddControlFlow(string targetNodeName, ControlFlowBuilderAction buildAction = null);
    }

    public interface IControlFlow
    {
        void AddControlFlow(string targetNodeName, ControlFlowBuilderAction buildAction = null);
    }
}
