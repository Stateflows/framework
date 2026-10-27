using Stateflows.Entities.Attributes;

namespace Stateflows.MAF.AIAgents;

/// <summary>Defines a Stateflows entity field for persisting an AI agent session payload.</summary>
public interface AIAgentSessionEntity
{
    /// <summary>Gets or sets the serialized session data for an agent instance.</summary>
    [Field(FieldAccess.Get | FieldAccess.Set)]
    public string AIAgentSessionData { get; set; }
}