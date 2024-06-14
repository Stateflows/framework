using System.Collections.Generic;

namespace Stateflows.Activities.Context.Interfaces
{
    public interface IPipelineContext : IActivityFlowContext
    {
        IEnumerable<TokenHolder> Tokens { get; }
    }
}
