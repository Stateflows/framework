using Stateflows.Common.Attributes;

namespace Stateflows.MAF.AIAgents;

[AttributeUsage(AttributeTargets.Class)]
public class AIAgentBehaviorAttribute(string? name = null, int version = 1) : BehaviorAttribute(name, version);
