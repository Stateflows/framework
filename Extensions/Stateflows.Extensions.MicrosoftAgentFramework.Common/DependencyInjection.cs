using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Registration.Interfaces;
using Stateflows.MAF.AIAgents;
using Stateflows.MAF.AIAgents.Classes;

namespace Stateflows.Extensions.MicrosoftAgentFramework;

public static class DependencyInjection
{
    public static IStateflowsClientBuilder AddAIAgents(this IStateflowsClientBuilder builder)
    {
        builder.ServiceCollection.AddMicrosoftAgentFrameworkServices();

        return builder;
    }
    
    private static IServiceCollection AddMicrosoftAgentFrameworkServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IAIAgentLocator, AIAgentLocator>();

        return serviceCollection;
    }
}