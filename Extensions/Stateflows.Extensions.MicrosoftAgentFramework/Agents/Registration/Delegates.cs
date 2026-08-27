using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace Stateflows.MAF.AIAgents.Registration;

public delegate void AIAgentBuildAction(IAIAgentBuilder builder);

public delegate Task<AIAgent> AIAgentFactoryAsync(IServiceProvider serviceProvider, AITool[] frameworkTools);

public delegate AIAgent AIAgentFactory(IServiceProvider serviceProvider, AITool[] frameworkTools);

// public delegate Task<AgentThread> AgentThreadFactoryAsync(ChatHistory chatHistory);
//
// public delegate AgentThread AgentThreadFactory();

public delegate void AgentsBuildAction(IAIAgentsBuilder builder);

public delegate Task<IAIAgentInterceptor> AgentInterceptorFactoryAsync(IServiceProvider serviceProvider);

public delegate IAIAgentInterceptor AgentInterceptorFactory(IServiceProvider serviceProvider);
