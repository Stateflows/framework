﻿using Stateflows.Common;

namespace Stateflows.Activities.Registration.Interfaces.Base
{
    public interface IObjectFlow<out TReturn>
    {
        TReturn AddFlow<TToken>(string targetNodeName, ObjectFlowBuildAction<TToken> buildAction = null)
            where TToken : Token, new();
    }

    public interface IElseObjectFlow<out TReturn>
    {
        TReturn AddElseFlow<TToken>(string targetNodeName, ElseObjectFlowBuildAction<TToken> buildAction = null)
            where TToken : Token, new();
    }

    public interface IDecisionFlow<out TToken, out TReturn>
        where TToken : Token, new()
    {
        TReturn AddFlow(string targetNodeName, ObjectFlowBuildAction<TToken> buildAction = null);
    }

    public interface IElseDecisionFlow<out TToken, out TReturn>
        where TToken : Token, new()
    {
        TReturn AddElseFlow(string targetNodeName, ElseObjectFlowBuildAction<TToken> buildAction = null);
    }

    public interface IObjectFlow
    {
        void AddFlow<TToken>(string targetNodeName, ObjectFlowBuildAction<TToken> buildAction = null)
            where TToken : Token, new();
    }
}
