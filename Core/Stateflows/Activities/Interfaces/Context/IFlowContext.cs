using System;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities
{
    public interface IFlowContext
    {
        ISourceNodeContext SourceNode { get; }

        INodeContext TargetNode { get; }

        Type TokenType { get; }
        
        Type TargetTokenType { get; }

        int Weight { get; }
    }

    public interface IBeforeFlowContext : IFlowContext
    {
        int TokenCount { get; }
        
        int SourceTokenCount { get; }
    }

    public interface IAfterFlowContext : IBeforeFlowContext
    {
        bool Activated { get; }
        
        int TargetTokenCount { get; }
    }
}
