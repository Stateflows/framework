using Stateflows.Activities;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Common.Interfaces;
using Stateflows.MAF.AIAgents.Classes;
using Stateflows.MAF.AIAgents.Registration;

namespace Stateflows.MAF.AIAgents.Extensions;

/// <summary>Registers AI-agent action nodes in regular and overridden activities.</summary>
public static class ActivityBuilderAIExtensions
{
    /// <summary>Adds a named agent action node using a synchronous agent factory.</summary>
    /// <param name="activityBuilder">The activity builder receiving the node.</param>
    /// <param name="aiAgentActionNodeName">The node name within the activity.</param>
    /// <param name="aiAgentFactory">Creates the Microsoft Agent Framework agent.</param>
    /// <param name="buildAction">Optional configuration for the Stateflows action node.</param>
    /// <returns>The activity builder for further configuration.</returns>
    public static IActivityBuilder AddAIAgentAction(this IActivityBuilder activityBuilder, string aiAgentActionNodeName, AIAgentFactory aiAgentFactory, ActionBuildAction? buildAction = null)
        => AddAIAgentAction(activityBuilder, aiAgentActionNodeName, (sp, tools) => Task.FromResult(aiAgentFactory(sp, tools)), buildAction);
    
    /// <summary>Adds an agent action node using the default node name and a synchronous factory.</summary>
    /// <param name="activityBuilder">The activity builder receiving the node.</param>
    /// <param name="aiAgentFactory">Creates the Microsoft Agent Framework agent.</param>
    /// <param name="buildAction">Optional configuration for the Stateflows action node.</param>
    /// <returns>The activity builder for further configuration.</returns>
    public static IActivityBuilder AddAIAgentAction(this IActivityBuilder activityBuilder, AIAgentFactory aiAgentFactory, ActionBuildAction? buildAction = null)
        => AddAIAgentAction(activityBuilder, ActivityNode<AIAgentActionNode>.Name, aiAgentFactory, buildAction);

    /// <summary>Adds a named agent action node using an asynchronous agent factory.</summary>
    /// <param name="activityBuilder">The activity builder receiving the node.</param>
    /// <param name="aiAgentActionNodeName">The node name within the activity.</param>
    /// <param name="aiAgentFactoryAsync">Asynchronously creates the Microsoft Agent Framework agent.</param>
    /// <param name="buildAction">Optional configuration for the Stateflows action node.</param>
    /// <returns>The activity builder for further configuration.</returns>
    public static IActivityBuilder AddAIAgentAction(this IActivityBuilder activityBuilder,
        string aiAgentActionNodeName, AIAgentFactoryAsync aiAgentFactoryAsync, ActionBuildAction? buildAction = null)
        => activityBuilder.AddAction<AIAgentActionNode>(b =>
        {
            b.AddConfiguration(aiAgentFactoryAsync);
            
            buildAction?.Invoke(b as IActionBuilder);
        });
    
    /// <summary>Adds an agent action node using the default node name and an asynchronous factory.</summary>
    /// <param name="activityBuilder">The activity builder receiving the node.</param>
    /// <param name="aiAgentFactoryAsync">Asynchronously creates the Microsoft Agent Framework agent.</param>
    /// <param name="buildAction">Optional configuration for the Stateflows action node.</param>
    /// <returns>The activity builder for further configuration.</returns>
    public static IActivityBuilder AddAIAgentAction(this IActivityBuilder activityBuilder, AIAgentFactoryAsync aiAgentFactoryAsync, ActionBuildAction? buildAction = null)
        => AddAIAgentAction(activityBuilder, ActivityNode<AIAgentActionNode>.Name, aiAgentFactoryAsync, buildAction);
    
    /// <summary>Adds a named action node that creates an agent of the specified implementation type.</summary>
    /// <typeparam name="TAIAgent">The Stateflows agent implementation type.</typeparam>
    /// <param name="activityBuilder">The activity builder receiving the node.</param>
    /// <param name="aiAgentActionNodeName">The node name within the activity.</param>
    /// <param name="buildAction">Optional configuration for the Stateflows action node.</param>
    /// <returns>The activity builder for further configuration.</returns>
    public static IActivityBuilder AddAIAgentAction<TAIAgent>(this IActivityBuilder activityBuilder, string aiAgentActionNodeName, ActionBuildAction? buildAction = null)
        where TAIAgent : class, IAIAgent
        => activityBuilder.AddAction<AIAgentActionNode<TAIAgent>>(b =>
        {
            buildAction?.Invoke(b as IActionBuilder);
        });

    /// <summary>Adds an agent action node of the specified implementation type using its default node name.</summary>
    /// <typeparam name="TAIAgent">The Stateflows agent implementation type.</typeparam>
    /// <param name="activityBuilder">The activity builder receiving the node.</param>
    /// <param name="buildAction">Optional configuration for the Stateflows action node.</param>
    /// <returns>The activity builder for further configuration.</returns>
    public static IActivityBuilder AddAIAgentAction<TAIAgent>(this IActivityBuilder activityBuilder, ActionBuildAction? buildAction = null)
        where TAIAgent : class, IAIAgent
        => AddAIAgentAction<TAIAgent>(activityBuilder, ActivityNode<AIAgentActionNode<TAIAgent>>.Name, buildAction);
    
    
    
    
    /// <summary>Adds a named agent action node to an overridden activity using a synchronous factory.</summary>
    /// <param name="activityBuilder">The overridden activity builder receiving the node.</param>
    /// <param name="aiAgentActionNodeName">The node name within the activity.</param>
    /// <param name="aiAgentFactory">Creates the Microsoft Agent Framework agent.</param>
    /// <param name="buildAction">Optional configuration for the Stateflows action node.</param>
    /// <returns>The overridden activity builder for further configuration.</returns>
    public static IOverridenActivityBuilder AddAIAgentAction(this IOverridenActivityBuilder activityBuilder, string aiAgentActionNodeName, AIAgentFactory aiAgentFactory, ActionBuildAction? buildAction = null)
        => AddAIAgentAction(activityBuilder, aiAgentActionNodeName, (sp, tools) => Task.FromResult(aiAgentFactory(sp, tools)), buildAction);
    
    /// <summary>Adds an agent action node to an overridden activity using the default node name and a synchronous factory.</summary>
    /// <param name="activityBuilder">The overridden activity builder receiving the node.</param>
    /// <param name="aiAgentFactory">Creates the Microsoft Agent Framework agent.</param>
    /// <param name="buildAction">Optional configuration for the Stateflows action node.</param>
    /// <returns>The overridden activity builder for further configuration.</returns>
    public static IOverridenActivityBuilder AddAIAgentAction(this IOverridenActivityBuilder activityBuilder, AIAgentFactory aiAgentFactory, ActionBuildAction? buildAction = null)
        => AddAIAgentAction(activityBuilder, ActivityNode<AIAgentActionNode>.Name, aiAgentFactory, buildAction);

    /// <summary>Adds a named agent action node to an overridden activity using an asynchronous factory.</summary>
    /// <param name="activityBuilder">The overridden activity builder receiving the node.</param>
    /// <param name="aiAgentActionNodeName">The node name within the activity.</param>
    /// <param name="aiAgentFactoryAsync">Asynchronously creates the Microsoft Agent Framework agent.</param>
    /// <param name="buildAction">Optional configuration for the Stateflows action node.</param>
    /// <returns>The overridden activity builder for further configuration.</returns>
    public static IOverridenActivityBuilder AddAIAgentAction(this IOverridenActivityBuilder activityBuilder,
        string aiAgentActionNodeName, AIAgentFactoryAsync aiAgentFactoryAsync, ActionBuildAction? buildAction = null)
        => ((IActivityBuilder)activityBuilder).AddAIAgentAction(aiAgentActionNodeName, aiAgentFactoryAsync, buildAction) as IOverridenActivityBuilder;
    
    /// <summary>Adds an agent action node to an overridden activity using the default node name and an asynchronous factory.</summary>
    /// <param name="activityBuilder">The overridden activity builder receiving the node.</param>
    /// <param name="aiAgentFactoryAsync">Asynchronously creates the Microsoft Agent Framework agent.</param>
    /// <param name="buildAction">Optional configuration for the Stateflows action node.</param>
    /// <returns>The overridden activity builder for further configuration.</returns>
    public static IOverridenActivityBuilder AddAIAgentAction(this IOverridenActivityBuilder activityBuilder, AIAgentFactoryAsync aiAgentFactoryAsync, ActionBuildAction? buildAction = null)
        => AddAIAgentAction(activityBuilder, ActivityNode<AIAgentActionNode>.Name, aiAgentFactoryAsync, buildAction);
    
    /// <summary>Adds a named typed-agent action node to an overridden activity.</summary>
    /// <typeparam name="TAIAgent">The Stateflows agent implementation type.</typeparam>
    /// <param name="activityBuilder">The overridden activity builder receiving the node.</param>
    /// <param name="aiAgentActionNodeName">The node name within the activity.</param>
    /// <param name="buildAction">Optional configuration for the Stateflows action node.</param>
    /// <returns>The overridden activity builder for further configuration.</returns>
    public static IOverridenActivityBuilder AddAIAgentAction<TAIAgent>(this IOverridenActivityBuilder activityBuilder, string aiAgentActionNodeName, ActionBuildAction? buildAction = null)
        where TAIAgent : class, IAIAgent
        => ((IActivityBuilder)activityBuilder).AddAIAgentAction<TAIAgent>(aiAgentActionNodeName, buildAction) as IOverridenActivityBuilder;

    /// <summary>Adds a typed-agent action node to an overridden activity using its default node name.</summary>
    /// <typeparam name="TAIAgent">The Stateflows agent implementation type.</typeparam>
    /// <param name="activityBuilder">The overridden activity builder receiving the node.</param>
    /// <param name="buildAction">Optional configuration for the Stateflows action node.</param>
    /// <returns>The overridden activity builder for further configuration.</returns>
    public static IOverridenActivityBuilder AddAIAgentAction<TAIAgent>(this IOverridenActivityBuilder activityBuilder, ActionBuildAction? buildAction = null)
        where TAIAgent : class, IAIAgent
        => AddAIAgentAction<TAIAgent>(activityBuilder, ActivityNode<AIAgentActionNode<TAIAgent>>.Name, buildAction);
}