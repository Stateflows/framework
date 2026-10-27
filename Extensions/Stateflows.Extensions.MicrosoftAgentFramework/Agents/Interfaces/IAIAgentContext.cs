using Stateflows.Activities.Context.Interfaces;
using Stateflows.Common;

namespace Stateflows.MAF.AIAgents
{
    /// <summary>Combines the active behavior context with activity input and output access for agent code.</summary>
    public interface IAIAgentContext : IBehaviorContext, IInput, IOutput
    {
        /// <summary>Gets the identifier of the current agent action behavior.</summary>
        new AIAgentId Id { get; }
    }
}
