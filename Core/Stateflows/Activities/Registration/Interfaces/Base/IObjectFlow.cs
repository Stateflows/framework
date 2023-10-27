namespace Stateflows.Activities.Registration.Interfaces.Base
{
    public interface IObjectFlow<out TReturn>
    {
        TReturn AddObjectFlow<TToken>(string targetNodeName, FlowBuilderAction<TToken> buildAction = null)
            where TToken : Token, new();
    }

    public interface IObjectFlow
    {
        void AddObjectFlow<TToken>(string targetNodeName, FlowBuilderAction<TToken> buildAction = null)
            where TToken : Token, new();
    }
}
