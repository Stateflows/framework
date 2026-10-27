using System.Reflection;
using Stateflows.MAF.AIAgents.Registration;

namespace Stateflows.MAF.AIAgents
{
    /// <summary>Registers Microsoft Agent Framework agents and configures their action behavior.</summary>
    public interface IAIAgentsBuilder
    {
        /// <summary>Scans one assembly for types marked with <see cref="AIAgentBehaviorAttribute"/>.</summary>
        /// <param name="assembly">The assembly to scan.</param>
        /// <returns>This builder for further registrations.</returns>
        /// <remarks>The current implementation does not add registrations for the discovered types.</remarks>
        IAIAgentsBuilder AddFromAssembly(Assembly assembly);
        /// <summary>Scans each supplied assembly for types marked with <see cref="AIAgentBehaviorAttribute"/>.</summary>
        /// <param name="assemblies">The assemblies to scan.</param>
        /// <returns>This builder for further registrations.</returns>
        /// <remarks>The current implementation delegates to <see cref="AddFromAssembly(Assembly)"/>, which does not add registrations.</remarks>
        IAIAgentsBuilder AddFromAssemblies(IEnumerable<Assembly> assemblies);
        /// <summary>Scans all currently loaded assemblies for agent behavior attributes.</summary>
        /// <returns>This builder for further registrations.</returns>
        /// <remarks>The current implementation delegates to <see cref="AddFromAssemblies(IEnumerable{Assembly})"/>, which does not add registrations.</remarks>
        IAIAgentsBuilder AddFromLoadedAssemblies();
        /// <summary>Registers an agent from an asynchronous factory under the default version.</summary>
        /// <param name="agentName">The registered agent name.</param>
        /// <param name="aiAgentFactoryAsync">Creates the framework agent using a service provider and available tools.</param>
        /// <param name="agentBuildAction">Optional configuration for the agent action and its consumed events or tokens.</param>
        /// <returns>This builder for further registrations.</returns>
        IAIAgentsBuilder AddAIAgent(string agentName, AIAgentFactoryAsync aiAgentFactoryAsync, AIAgentBuildAction? agentBuildAction = null);
        /// <summary>Registers a versioned agent from an asynchronous factory.</summary>
        /// <param name="agentName">The registered agent name.</param>
        /// <param name="version">The agent behavior version.</param>
        /// <param name="aiAgentFactoryAsync">Creates the framework agent using a service provider and available tools.</param>
        /// <param name="agentBuildAction">Optional configuration for the agent action and its consumed events or tokens.</param>
        /// <returns>This builder for further registrations.</returns>
        IAIAgentsBuilder AddAIAgent(string agentName, int version, AIAgentFactoryAsync aiAgentFactoryAsync, AIAgentBuildAction? agentBuildAction = null);
        /// <summary>Registers an agent implementation type under its inferred name or the supplied name.</summary>
        /// <typeparam name="TAgent">The type implementing the Stateflows agent contract.</typeparam>
        /// <param name="agentName">An optional registered name; when omitted, the name is derived from the agent type and its attribute.</param>
        /// <param name="version">The agent behavior version, defaulting to the first version.</param>
        /// <param name="agentBuildAction">Optional configuration for the agent action and its consumed events or tokens.</param>
        /// <returns>This builder for further registrations.</returns>
        IAIAgentsBuilder AddAIAgent<TAgent>(string? agentName = null, int version = 1, AIAgentBuildAction? agentBuildAction = null)
            where TAgent : class, IAIAgent;

        // IAgentsBuilder AddInterceptor<TInterceptor>()
        //     where TInterceptor : class, IAgentInterceptor;
        // IAgentsBuilder AddInterceptor(AgentInterceptorFactoryAsync interceptorFactoryAsync);
        // IAgentsBuilder AddInterceptor(AgentInterceptorFactory interceptorFactory)
        //     => AddInterceptor(serviceProvider => Task.FromResult(interceptorFactory(serviceProvider)));
        // IAgentsBuilder AddExceptionHandler<TExceptionHandler>()
        //     where TExceptionHandler : class, IAgentExceptionHandler;
        // IAgentsBuilder AddExceptionHandler(AgentExceptionHandlerFactoryAsync exceptionHandlerFactoryAsync);
        // IAgentsBuilder AddExceptionHandler(AgentExceptionHandlerFactory exceptionHandlerFactory)
        //     => AddExceptionHandler(serviceProvider => Task.FromResult(exceptionHandlerFactory(serviceProvider)));
    }
}
