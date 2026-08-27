using System.Reflection;
using Stateflows.MAF.AIAgents.Registration;

namespace Stateflows.MAF.AIAgents
{
    public interface IAIAgentsBuilder
    {
        IAIAgentsBuilder AddFromAssembly(Assembly assembly);
        IAIAgentsBuilder AddFromAssemblies(IEnumerable<Assembly> assemblies);
        IAIAgentsBuilder AddFromLoadedAssemblies();
        IAIAgentsBuilder AddAIAgent(string agentName, AIAgentFactoryAsync aiAgentFactoryAsync, AIAgentBuildAction? agentBuildAction = null);
        IAIAgentsBuilder AddAIAgent(string agentName, int version, AIAgentFactoryAsync aiAgentFactoryAsync, AIAgentBuildAction? agentBuildAction = null);
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
