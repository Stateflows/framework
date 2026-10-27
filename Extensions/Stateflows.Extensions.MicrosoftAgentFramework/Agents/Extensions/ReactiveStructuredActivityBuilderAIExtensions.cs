using Stateflows.Activities;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Common.Interfaces;
using Stateflows.MAF.AIAgents.Classes;
using Stateflows.MAF.AIAgents.Registration;

namespace Stateflows.MAF.AIAgents.Extensions;

/// <summary>Registers AI-agent action nodes in reactive structured activities.</summary>
public static class ReactiveStructuredActivityBuilderAIExtensions
{
    /// <summary>Adds a named agent action node using a synchronous agent factory.</summary>
    /// <param name="activityBuilder">The reactive structured activity builder receiving the node.</param>
    /// <param name="aiAgentActionNodeName">The node name within the activity.</param>
    /// <param name="aiAgentFactory">Creates the Microsoft Agent Framework agent.</param>
    /// <param name="buildAction">Optional configuration for the Stateflows action node.</param>
    /// <returns>The activity builder for further configuration.</returns>
    public static IReactiveStructuredActivityBuilder AddAIAgentAction(this IReactiveStructuredActivityBuilder activityBuilder, string aiAgentActionNodeName, AIAgentFactory aiAgentFactory, ActionBuildAction? buildAction = null)
        => AddAIAgentAction(activityBuilder, aiAgentActionNodeName, (sp, tools) => Task.FromResult(aiAgentFactory(sp, tools)), buildAction);
    
    /// <summary>Adds an agent action node using the default node name and a synchronous factory.</summary>
    /// <param name="activityBuilder">The reactive structured activity builder receiving the node.</param>
    /// <param name="aiAgentFactory">Creates the Microsoft Agent Framework agent.</param>
    /// <param name="buildAction">Optional configuration for the Stateflows action node.</param>
    /// <returns>The activity builder for further configuration.</returns>
    public static IReactiveStructuredActivityBuilder AddAIAgentAction(this IReactiveStructuredActivityBuilder activityBuilder, AIAgentFactory aiAgentFactory, ActionBuildAction? buildAction = null)
        => AddAIAgentAction(activityBuilder, ActivityNode<AIAgentActionNode>.Name, aiAgentFactory, buildAction);

    /// <summary>Adds a named agent action node using an asynchronous agent factory.</summary>
    /// <param name="activityBuilder">The reactive structured activity builder receiving the node.</param>
    /// <param name="aiAgentActionNodeName">The node name within the activity.</param>
    /// <param name="aiAgentFactoryAsync">Asynchronously creates the Microsoft Agent Framework agent.</param>
    /// <param name="buildAction">Optional configuration for the Stateflows action node.</param>
    /// <returns>The activity builder for further configuration.</returns>
    public static IReactiveStructuredActivityBuilder AddAIAgentAction(this IReactiveStructuredActivityBuilder activityBuilder,
        string aiAgentActionNodeName, AIAgentFactoryAsync aiAgentFactoryAsync, ActionBuildAction? buildAction = null)
        => activityBuilder.AddAction<AIAgentActionNode>(b =>
        {
            b.AddConfiguration(aiAgentFactoryAsync);
            
            buildAction?.Invoke(b as IActionBuilder);
        });
    
    /// <summary>Adds an agent action node using the default node name and an asynchronous factory.</summary>
    /// <param name="activityBuilder">The reactive structured activity builder receiving the node.</param>
    /// <param name="aiAgentFactoryAsync">Asynchronously creates the Microsoft Agent Framework agent.</param>
    /// <param name="buildAction">Optional configuration for the Stateflows action node.</param>
    /// <returns>The activity builder for further configuration.</returns>
    public static IReactiveStructuredActivityBuilder AddAIAgentAction(this IReactiveStructuredActivityBuilder activityBuilder, AIAgentFactoryAsync aiAgentFactoryAsync, ActionBuildAction? buildAction = null)
        => AddAIAgentAction(activityBuilder, ActivityNode<AIAgentActionNode>.Name, aiAgentFactoryAsync, buildAction);
    
    /// <summary>Adds a named action node that creates an agent of the specified implementation type.</summary>
    /// <typeparam name="TAIAgent">The Stateflows agent implementation type.</typeparam>
    /// <param name="activityBuilder">The reactive structured activity builder receiving the node.</param>
    /// <param name="aiAgentActionNodeName">The node name within the activity.</param>
    /// <param name="buildAction">Optional configuration for the Stateflows action node.</param>
    /// <returns>The activity builder for further configuration.</returns>
    public static IReactiveStructuredActivityBuilder AddAIAgentAction<TAIAgent>(this IReactiveStructuredActivityBuilder activityBuilder, string aiAgentActionNodeName, ActionBuildAction? buildAction = null)
        where TAIAgent : class, IAIAgent
        => activityBuilder.AddAction<AIAgentActionNode<TAIAgent>>(b =>
        {
            buildAction?.Invoke(b as IActionBuilder);
        });

    /// <summary>Adds an agent action node of the specified implementation type using its default node name.</summary>
    /// <typeparam name="TAIAgent">The Stateflows agent implementation type.</typeparam>
    /// <param name="activityBuilder">The reactive structured activity builder receiving the node.</param>
    /// <param name="buildAction">Optional configuration for the Stateflows action node.</param>
    /// <returns>The activity builder for further configuration.</returns>
    public static IReactiveStructuredActivityBuilder AddAIAgentAction<TAIAgent>(this IReactiveStructuredActivityBuilder activityBuilder, ActionBuildAction? buildAction = null)
        where TAIAgent : class, IAIAgent
        => AddAIAgentAction<TAIAgent>(activityBuilder, ActivityNode<AIAgentActionNode<TAIAgent>>.Name, buildAction);
    
    
    
    
    /// <summary>Adds a named agent action node to an overridden reactive activity using a synchronous factory.</summary>
    /// <param name="activityBuilder">The overridden reactive structured activity builder receiving the node.</param>
    /// <param name="aiAgentActionNodeName">The node name within the activity.</param>
    /// <param name="aiAgentFactory">Creates the Microsoft Agent Framework agent.</param>
    /// <param name="buildAction">Optional configuration for the Stateflows action node.</param>
    /// <returns>The overridden activity builder for further configuration.</returns>
    public static IOverridenReactiveStructuredActivityBuilder AddAIAgentAction(this IOverridenReactiveStructuredActivityBuilder activityBuilder, string aiAgentActionNodeName, AIAgentFactory aiAgentFactory, ActionBuildAction? buildAction = null)
        => AddAIAgentAction(activityBuilder, aiAgentActionNodeName, (sp, tools) => Task.FromResult(aiAgentFactory(sp, tools)), buildAction);
    
    /// <summary>Adds an agent action node to an overridden reactive activity using the default name and a synchronous factory.</summary>
    /// <param name="activityBuilder">The overridden reactive structured activity builder receiving the node.</param>
    /// <param name="aiAgentFactory">Creates the Microsoft Agent Framework agent.</param>
    /// <param name="buildAction">Optional configuration for the Stateflows action node.</param>
    /// <returns>The overridden activity builder for further configuration.</returns>
    public static IOverridenReactiveStructuredActivityBuilder AddAIAgentAction(this IOverridenReactiveStructuredActivityBuilder activityBuilder, AIAgentFactory aiAgentFactory, ActionBuildAction? buildAction = null)
        => AddAIAgentAction(activityBuilder, ActivityNode<AIAgentActionNode>.Name, aiAgentFactory, buildAction);

    /// <summary>Adds a named agent action node to an overridden reactive activity using an asynchronous factory.</summary>
    /// <param name="activityBuilder">The overridden reactive structured activity builder receiving the node.</param>
    /// <param name="aiAgentActionNodeName">The node name within the activity.</param>
    /// <param name="aiAgentFactoryAsync">Asynchronously creates the Microsoft Agent Framework agent.</param>
    /// <param name="buildAction">Optional configuration for the Stateflows action node.</param>
    /// <returns>The overridden activity builder for further configuration.</returns>
    public static IOverridenReactiveStructuredActivityBuilder AddAIAgentAction(this IOverridenReactiveStructuredActivityBuilder activityBuilder,
        string aiAgentActionNodeName, AIAgentFactoryAsync aiAgentFactoryAsync, ActionBuildAction? buildAction = null)
        => ((IReactiveStructuredActivityBuilder)activityBuilder).AddAIAgentAction(aiAgentActionNodeName, aiAgentFactoryAsync, buildAction) as IOverridenReactiveStructuredActivityBuilder;
    
    /// <summary>Adds an agent action node to an overridden reactive activity using the default name and an asynchronous factory.</summary>
    /// <param name="activityBuilder">The overridden reactive structured activity builder receiving the node.</param>
    /// <param name="aiAgentFactoryAsync">Asynchronously creates the Microsoft Agent Framework agent.</param>
    /// <param name="buildAction">Optional configuration for the Stateflows action node.</param>
    /// <returns>The overridden activity builder for further configuration.</returns>
    public static IOverridenReactiveStructuredActivityBuilder AddAIAgentAction(this IOverridenReactiveStructuredActivityBuilder activityBuilder, AIAgentFactoryAsync aiAgentFactoryAsync, ActionBuildAction? buildAction = null)
        => AddAIAgentAction(activityBuilder, ActivityNode<AIAgentActionNode>.Name, aiAgentFactoryAsync, buildAction);
    
    /// <summary>Adds a named typed-agent action node to an overridden reactive activity.</summary>
    /// <typeparam name="TAIAgent">The Stateflows agent implementation type.</typeparam>
    /// <param name="activityBuilder">The overridden reactive structured activity builder receiving the node.</param>
    /// <param name="aiAgentActionNodeName">The node name within the activity.</param>
    /// <param name="buildAction">Optional configuration for the Stateflows action node.</param>
    /// <returns>The overridden activity builder for further configuration.</returns>
    public static IOverridenReactiveStructuredActivityBuilder AddAIAgentAction<TAIAgent>(this IOverridenReactiveStructuredActivityBuilder activityBuilder, string aiAgentActionNodeName, ActionBuildAction? buildAction = null)
        where TAIAgent : class, IAIAgent
        => ((IReactiveStructuredActivityBuilder)activityBuilder).AddAIAgentAction<TAIAgent>(aiAgentActionNodeName, buildAction) as IOverridenReactiveStructuredActivityBuilder;

    /// <summary>Adds a typed-agent action node to an overridden reactive activity using its default node name.</summary>
    /// <typeparam name="TAIAgent">The Stateflows agent implementation type.</typeparam>
    /// <param name="activityBuilder">The overridden reactive structured activity builder receiving the node.</param>
    /// <param name="buildAction">Optional configuration for the Stateflows action node.</param>
    /// <returns>The overridden activity builder for further configuration.</returns>
    public static IOverridenReactiveStructuredActivityBuilder AddAIAgentAction<TAIAgent>(this IOverridenReactiveStructuredActivityBuilder activityBuilder, ActionBuildAction? buildAction = null)
        where TAIAgent : class, IAIAgent
        => AddAIAgentAction<TAIAgent>(activityBuilder, ActivityNode<AIAgentActionNode<TAIAgent>>.Name, buildAction);
}