using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace Stateflows.MAF.AIAgents.Registration;

/// <summary>Configures a newly registered agent through its builder.</summary>
/// <param name="builder">The agent builder to configure.</param>
public delegate void AIAgentBuildAction(IAIAgentBuilder builder);

/// <summary>Creates a Microsoft Agent Framework agent asynchronously.</summary>
/// <param name="serviceProvider">The services available to the factory.</param>
/// <param name="frameworkTools">Tools supplied by the Stateflows agent integration.</param>
/// <returns>A task producing the framework agent.</returns>
public delegate Task<AIAgent> AIAgentFactoryAsync(IServiceProvider serviceProvider, AITool[] frameworkTools);

/// <summary>Creates a Microsoft Agent Framework agent synchronously.</summary>
/// <param name="serviceProvider">The services available to the factory.</param>
/// <param name="frameworkTools">Tools supplied by the Stateflows agent integration.</param>
/// <returns>The framework agent.</returns>
public delegate AIAgent AIAgentFactory(IServiceProvider serviceProvider, AITool[] frameworkTools);

// public delegate Task<AgentThread> AgentThreadFactoryAsync(ChatHistory chatHistory);
//
// public delegate AgentThread AgentThreadFactory();

/// <summary>Configures the set of agents registered with Stateflows.</summary>
/// <param name="builder">The agent collection builder.</param>
public delegate void AgentsBuildAction(IAIAgentsBuilder builder);

/// <summary>Creates an interceptor instance asynchronously from the current service provider.</summary>
/// <param name="serviceProvider">The services available to the interceptor factory.</param>
/// <returns>A task producing the agent interceptor.</returns>
public delegate Task<IAIAgentInterceptor> AgentInterceptorFactoryAsync(IServiceProvider serviceProvider);

/// <summary>Creates an interceptor instance from the current service provider.</summary>
/// <param name="serviceProvider">The services available to the interceptor factory.</param>
/// <returns>The agent interceptor.</returns>
public delegate IAIAgentInterceptor AgentInterceptorFactory(IServiceProvider serviceProvider);
