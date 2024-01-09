namespace Stateflows.Activities.Registration.Interfaces.Base
{
    public interface IObjectFlow<out TReturn>
    {
        TReturn AddObjectFlow<TToken>(string targetNodeName, ObjectFlowBuilderAction<TToken> buildAction = null)
            where TToken : Token, new();
    }

    public interface IElseObjectFlow<out TReturn>
    {
        TReturn AddElseObjectFlow<TToken>(string targetNodeName, ElseObjectFlowBuilderAction<TToken> buildAction = null)
            where TToken : Token, new();
    }

    public interface IObjectFlow<out TToken, out TReturn>
        where TToken : Token, new()
    {
        TReturn AddObjectFlow(string targetNodeName, ObjectFlowBuilderAction<TToken> buildAction = null);
    }

    public interface IElseObjectFlow<out TToken, out TReturn>
        where TToken : Token, new()
    {
        TReturn AddElseObjectFlow(string targetNodeName, ElseObjectFlowBuilderAction<TToken> buildAction = null);
    }

    public interface IObjectFlow
    {
        void AddObjectFlow<TToken>(string targetNodeName, ObjectFlowBuilderAction<TToken> buildAction = null)
            where TToken : Token, new();
    }
}
