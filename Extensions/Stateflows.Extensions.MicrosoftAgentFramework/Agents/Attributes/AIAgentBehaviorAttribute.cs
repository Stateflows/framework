using Stateflows.Common.Attributes;

namespace Stateflows.MAF.AIAgents;

/// <summary>Assigns an optional name and version to a Microsoft Agent Framework behavior class.</summary>
/// <param name="name">The optional registered agent name; when omitted, the type's readable name is used.</param>
/// <param name="version">The behavior version to register.</param>
[AttributeUsage(AttributeTargets.Class)]
public class AIAgentBehaviorAttribute(string? name = null, int version = 1) : BehaviorAttribute(name, version);
