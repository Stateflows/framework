using System.Collections.Generic;

namespace Stateflows.Activities.Context.Interfaces
{
    public interface IPipelineContext : IFlowContext
    {
        IEnumerable<Token> Tokens { get; }
    }
}
