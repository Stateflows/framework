using Stateflows.Activities.Context.Interfaces;
using Stateflows.Common;

namespace Stateflows.MAF.AIAgents
{
    public interface IAIAgentContext : IBehaviorContext, IInput, IOutput
    {
        /// <summary>
        /// Identifier of current Action behavior
        /// </summary>
        new AgentId Id { get; }
    }
}
