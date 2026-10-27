using System.Collections.Generic;
using Stateflows.Common;

namespace Stateflows.Activities.Context.Interfaces
{
    /// <summary>Context for a pipeline stage, exposing the tokens currently in the pipeline.</summary>
    public interface IPipelineContext : IActivityFlowContext
    {
        /// <summary>Tokens presented to the pipeline stage.</summary>
        IEnumerable<TokenHolder> Tokens { get; }
    }
}
