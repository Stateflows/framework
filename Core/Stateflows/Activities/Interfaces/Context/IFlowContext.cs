using System;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities
{
    public interface IFlowContext
    {
        ISourceNodeContext SourceNode { get; }

        INodeContext TargetNode { get; }

        Type TokenType { get; }

        int Weight { get; }
    }
}
