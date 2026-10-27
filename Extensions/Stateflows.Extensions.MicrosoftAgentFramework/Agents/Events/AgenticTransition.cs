using Stateflows.Extensions.MinimalAPIs.Attributes;

namespace Stateflows.MAF.AIAgents.Events;

[NoApiMapping]
internal sealed class AgenticTransition
{
    public string TransitionMarker { get; set; }
}