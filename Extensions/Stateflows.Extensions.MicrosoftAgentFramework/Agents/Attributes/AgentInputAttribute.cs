namespace Stateflows.MAF.AIAgents;

/// <summary>Marks an agent method parameter as input supplied by the Stateflows activity or action.</summary>
[AttributeUsage(AttributeTargets.Parameter)]
public class AgentInputAttribute : Attribute;