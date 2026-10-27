using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Registration.Interfaces;
using Stateflows.MAF.AIAgents;
using Stateflows.MAF.AIAgents.Classes;

namespace Stateflows.Extensions.MicrosoftAgentFramework;

/// <summary>Registers shared Microsoft Agent Framework services for Stateflows clients.</summary>
public static class DependencyInjection
{
    /// <summary>Adds the AI-agent locator service to the Stateflows client service collection.</summary>
    /// <param name="builder">The Stateflows client builder to configure.</param>
    /// <returns>The same client builder, for further configuration.</returns>
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