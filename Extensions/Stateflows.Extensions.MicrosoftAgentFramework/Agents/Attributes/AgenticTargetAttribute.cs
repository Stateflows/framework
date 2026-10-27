namespace Stateflows.MAF.AIAgents;

/// <summary>Provides an agentic target's name and description for framework-driven transition selection.</summary>
/// <param name="name">The optional target name.</param>
/// <param name="description">The optional target description presented to the agent.</param>
[AttributeUsage(AttributeTargets.Class)]
public class AgenticTargetAttribute(string? name = null, string? description = null) : Attribute;