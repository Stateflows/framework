using Stateflows.Entities.Attributes;

namespace Stateflows.MAF.AIAgents;

public interface AIAgentSessionEntity
{
    [Field(FieldAccess.Get | FieldAccess.Set)]
    public string AIAgentSessionData { get; set; }
}