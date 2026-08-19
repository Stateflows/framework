namespace Stateflows.MAF.AIAgents;

[AttributeUsage(AttributeTargets.Class)]
public class AgenticTargetAttribute(string? name = null, string? description = null) : Attribute;