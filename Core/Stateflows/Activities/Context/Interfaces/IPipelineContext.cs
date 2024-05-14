using System.Collections.Generic;
using Stateflows.Common;

namespace Stateflows.Activities.Context.Interfaces
{
    public interface IPipelineContext : IFlowContext
    {
        IEnumerable<TokenHolder> Tokens { get; }
    }
}
