using Stateflows.Extensions.MinimalAPIs.Attributes;

namespace Stateflows.MAF.AIAgents.Events;

[NoApiMapping]
internal sealed class AgenticDecision
{
    public string DecisionMarker { get; set; }
}