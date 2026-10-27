using Stateflows.Extensions.MinimalAPIs.Attributes;

namespace Stateflows.MAF.AIAgents.Events;

[NoApiMapping]
internal sealed class AgenticHandoff
{
    public string HandoffMarker { get; set; }
}