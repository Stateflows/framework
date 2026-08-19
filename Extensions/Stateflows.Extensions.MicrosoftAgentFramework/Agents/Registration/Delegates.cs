using Microsoft.Agents.AI;

namespace Stateflows.MAF.AIAgents.Registration;

public delegate void AgentBuildAction(IAIAgentBuilder builder);

public delegate Task<AIAgent> AIAgentFactoryAsync(IServiceProvider serviceProvider);

public delegate AIAgent AIAgentFactory(IServiceProvider serviceProvider);

// public delegate Task<AgentThread> AgentThreadFactoryAsync(ChatHistory chatHistory);
//
// public delegate AgentThread AgentThreadFactory();

public delegate void AgentsBuildAction(IAIAgentsBuilder builder);

public delegate Task<IAIAgentInterceptor> AgentInterceptorFactoryAsync(IServiceProvider serviceProvider);

public delegate IAIAgentInterceptor AgentInterceptorFactory(IServiceProvider serviceProvider);
