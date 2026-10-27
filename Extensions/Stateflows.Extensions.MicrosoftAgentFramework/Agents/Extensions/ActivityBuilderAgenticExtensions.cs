using Stateflows.Activities;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Common.Interfaces;
using Stateflows.MAF.AIAgents.Classes;
using Stateflows.MAF.AIAgents.Registration;

namespace Stateflows.MAF.AIAgents.Extensions;

/// <summary>Builds structured activity nodes that host Microsoft Agent Framework conversations.</summary>
public static class ActivityBuilderAgenticExtensions
{
    /// <summary>Adds a named agentic structured activity using a synchronous agent factory.</summary>
    /// <param name="activityBuilder">The activity builder receiving the agentic node.</param>
    /// <param name="agenticActivityNodeName">The structured node name.</param>
    /// <param name="aiAgentFactory">Creates the Microsoft Agent Framework agent.</param>
    /// <param name="buildAction">Optional configuration for the structured activity's external inputs and outputs.</param>
    /// <returns>The activity builder for further configuration.</returns>
    public static IActivityBuilder AddAgenticActivity(this IActivityBuilder activityBuilder, string agenticActivityNodeName, AIAgentFactory aiAgentFactory, ReactiveStructuredActivityExternalsBuildAction? buildAction = null)
        => AddAgenticActivity(activityBuilder, agenticActivityNodeName, (sp, tools) => Task.FromResult(aiAgentFactory(sp, tools)), buildAction);
    
    /// <summary>Adds an agentic structured activity using the default node name and a synchronous factory.</summary>
    /// <param name="activityBuilder">The activity builder receiving the agentic node.</param>
    /// <param name="aiAgentFactory">Creates the Microsoft Agent Framework agent.</param>
    /// <param name="buildAction">Optional configuration for the structured activity's external inputs and outputs.</param>
    /// <returns>The activity builder for further configuration.</returns>
    public static IActivityBuilder AddAgenticActivity(this IActivityBuilder activityBuilder, AIAgentFactory aiAgentFactory, ReactiveStructuredActivityExternalsBuildAction? buildAction = null)
        => AddAgenticActivity(activityBuilder, ActivityNode<AIAgentActionNode>.Name, aiAgentFactory, buildAction);

    /// <summary>Adds a named agentic structured activity using an asynchronous agent factory.</summary>
    /// <param name="activityBuilder">The activity builder receiving the agentic node.</param>
    /// <param name="agenticActivityNodeName">The structured node name.</param>
    /// <param name="aiAgentFactoryAsync">Asynchronously creates the Microsoft Agent Framework agent.</param>
    /// <param name="buildAction">Optional configuration for the structured activity's external inputs and outputs.</param>
    /// <returns>The activity builder for further configuration.</returns>
    public static IActivityBuilder AddAgenticActivity(this IActivityBuilder activityBuilder,
        string agenticActivityNodeName, AIAgentFactoryAsync aiAgentFactoryAsync, ReactiveStructuredActivityExternalsBuildAction? buildAction = null)
        => activityBuilder.AddStructuredActivity(agenticActivityNodeName, b =>
        {
            b
                .AddInitial(b => b
                    .AddControlFlow<AIAgentActionNode>()
                )
                .AddAcceptEventAction<string>(c =>
                    {
                        c.Output(c.Event);
                        return Task.CompletedTask;
                    }, b => b
                        .AddFlow<string, AIAgentActionNode>(b => b.SetWeight(0))
                )
                .AddAcceptEventAction<AgenticMessage>(c =>
                    {
                        c.Output(c.Event);
                        return Task.CompletedTask;
                    }, b => b
                        .AddFlow<AgenticMessage, AIAgentActionNode>(b => b.SetWeight(0))
                )
                .AddAction<AIAgentActionNode>(b => b
                    .AddConfiguration(aiAgentFactoryAsync)
                );
            
            buildAction?.Invoke(b);
        });
    
    /// <summary>Adds an agentic structured activity using the default node name and an asynchronous factory.</summary>
    /// <param name="activityBuilder">The activity builder receiving the agentic node.</param>
    /// <param name="aiAgentFactoryAsync">Asynchronously creates the Microsoft Agent Framework agent.</param>
    /// <param name="buildAction">Optional configuration for the structured activity's external inputs and outputs.</param>
    /// <returns>The activity builder for further configuration.</returns>
    public static IActivityBuilder AddAgenticActivity(this IActivityBuilder activityBuilder, AIAgentFactoryAsync aiAgentFactoryAsync, ReactiveStructuredActivityExternalsBuildAction? buildAction = null)
        => AddAgenticActivity(activityBuilder, ActivityNode<AIAgentActionNode>.Name, aiAgentFactoryAsync, buildAction);
    
    /// <summary>Adds a named agentic structured activity that creates the specified typed agent.</summary>
    /// <typeparam name="TAIAgent">The Stateflows agent implementation type.</typeparam>
    /// <param name="activityBuilder">The activity builder receiving the agentic node.</param>
    /// <param name="agenticActivityNodeName">The structured node name.</param>
    /// <param name="buildAction">Optional configuration for the structured activity's external inputs and outputs.</param>
    /// <returns>The activity builder for further configuration.</returns>
    public static IActivityBuilder AddAgenticActivity<TAIAgent>(this IActivityBuilder activityBuilder, string agenticActivityNodeName, ReactiveStructuredActivityExternalsBuildAction? buildAction = null)
        where TAIAgent : class, IAIAgent
        => activityBuilder.AddStructuredActivity<AgenticActivity<TAIAgent>>(agenticActivityNodeName, b =>
        {
            b
                .AddInitial(b => b
                    .AddControlFlow<AIAgentActionNode<TAIAgent>>()
                )
                .AddInput(b => b
                    .AddFlow<string, AIAgentActionNode<TAIAgent>>(b => b.SetWeight(0))
                    .AddFlow<AgenticMessage, AIAgentActionNode<TAIAgent>>(b => b.SetWeight(0))
                )
                .AddAcceptEventAction<string>(c =>
                    {
                        c.Output(c.Event);
                        return Task.CompletedTask;
                    }, b => b
                        .AddFlow<string, AIAgentActionNode<TAIAgent>>(b => b.SetWeight(0))
                )
                .AddAcceptEventAction<AgenticMessage>(c =>
                    {
                        c.Output(c.Event);
                        return Task.CompletedTask;
                    }, b => b
                        .AddFlow<AgenticMessage, AIAgentActionNode<TAIAgent>>(b => b.SetWeight(0))
                )
                .AddAction<AIAgentActionNode<TAIAgent>>();
            
            buildAction?.Invoke(b);
        });

    /// <summary>Adds an agentic structured activity for the specified typed agent using its default node name.</summary>
    /// <typeparam name="TAIAgent">The Stateflows agent implementation type.</typeparam>
    /// <param name="activityBuilder">The activity builder receiving the agentic node.</param>
    /// <param name="buildAction">Optional configuration for the structured activity's external inputs and outputs.</param>
    /// <returns>The activity builder for further configuration.</returns>
    public static IActivityBuilder AddAgenticActivity<TAIAgent>(this IActivityBuilder activityBuilder, ReactiveStructuredActivityExternalsBuildAction? buildAction = null)
        where TAIAgent : class, IAIAgent
        => activityBuilder.AddAgenticActivity<TAIAgent>(ActivityNode<AgenticActivity<TAIAgent>>.Name, buildAction);
    
    
    
    
    /// <summary>Adds a named agentic structured activity to an overridden activity using a synchronous factory.</summary>
    /// <param name="activityBuilder">The overridden activity builder receiving the agentic node.</param>
    /// <param name="agenticActivityNodeName">The structured node name.</param>
    /// <param name="aiAgentFactory">Creates the Microsoft Agent Framework agent.</param>
    /// <param name="buildAction">Optional configuration for the overridden structured activity's external inputs and outputs.</param>
    /// <returns>The overridden activity builder for further configuration.</returns>
    public static IOverridenActivityBuilder AddAgenticActivity(this IOverridenActivityBuilder activityBuilder, string agenticActivityNodeName, AIAgentFactory aiAgentFactory, OverridenReactiveStructuredActivityExternalsBuildAction? buildAction = null)
        => AddAgenticActivity(activityBuilder, agenticActivityNodeName, (sp, tools) => Task.FromResult(aiAgentFactory(sp, tools)), buildAction);
    
    /// <summary>Adds an agentic structured activity to an overridden activity using the default name and a synchronous factory.</summary>
    /// <param name="activityBuilder">The overridden activity builder receiving the agentic node.</param>
    /// <param name="aiAgentFactory">Creates the Microsoft Agent Framework agent.</param>
    /// <param name="buildAction">Optional configuration for the overridden structured activity's external inputs and outputs.</param>
    /// <returns>The overridden activity builder for further configuration.</returns>
    public static IOverridenActivityBuilder AddAgenticActivity(this IOverridenActivityBuilder activityBuilder, AIAgentFactory aiAgentFactory, OverridenReactiveStructuredActivityExternalsBuildAction? buildAction = null)
        => AddAgenticActivity(activityBuilder, ActivityNode<AIAgentActionNode>.Name, aiAgentFactory, buildAction);

    /// <summary>Adds a named agentic structured activity to an overridden activity using an asynchronous factory.</summary>
    /// <param name="activityBuilder">The overridden activity builder receiving the agentic node.</param>
    /// <param name="agenticActivityNodeName">The structured node name.</param>
    /// <param name="aiAgentFactoryAsync">Asynchronously creates the Microsoft Agent Framework agent.</param>
    /// <param name="buildAction">Optional configuration for the overridden structured activity's external inputs and outputs.</param>
    /// <returns>The overridden activity builder for further configuration.</returns>
    public static IOverridenActivityBuilder AddAgenticActivity(this IOverridenActivityBuilder activityBuilder,
        string agenticActivityNodeName, AIAgentFactoryAsync aiAgentFactoryAsync, OverridenReactiveStructuredActivityExternalsBuildAction? buildAction = null)
        => ((IActivityBuilder)activityBuilder).AddAgenticActivity(agenticActivityNodeName, aiAgentFactoryAsync, b => buildAction?.Invoke(b as IOverridenReactiveStructuredActivityExternalsBuilder)) as IOverridenActivityBuilder;
    
    /// <summary>Adds an agentic structured activity to an overridden activity using the default name and an asynchronous factory.</summary>
    /// <param name="activityBuilder">The overridden activity builder receiving the agentic node.</param>
    /// <param name="aiAgentFactoryAsync">Asynchronously creates the Microsoft Agent Framework agent.</param>
    /// <param name="buildAction">Optional configuration for the overridden structured activity's external inputs and outputs.</param>
    /// <returns>The overridden activity builder for further configuration.</returns>
    public static IOverridenActivityBuilder AddAgenticActivity(this IOverridenActivityBuilder activityBuilder, AIAgentFactoryAsync aiAgentFactoryAsync, OverridenReactiveStructuredActivityExternalsBuildAction? buildAction = null)
        => AddAgenticActivity(activityBuilder, ActivityNode<AIAgentActionNode>.Name, aiAgentFactoryAsync, buildAction);
    
    /// <summary>Adds a named typed-agent structured activity to an overridden activity.</summary>
    /// <typeparam name="TAIAgent">The Stateflows agent implementation type.</typeparam>
    /// <param name="activityBuilder">The overridden activity builder receiving the agentic node.</param>
    /// <param name="agenticActivityNodeName">The structured node name.</param>
    /// <param name="buildAction">Optional configuration for the overridden structured activity's external inputs and outputs.</param>
    /// <returns>The overridden activity builder for further configuration.</returns>
    public static IOverridenActivityBuilder AddAgenticActivity<TAIAgent>(this IOverridenActivityBuilder activityBuilder, string agenticActivityNodeName, OverridenReactiveStructuredActivityExternalsBuildAction? buildAction = null)
        where TAIAgent : class, IAIAgent
        => ((IActivityBuilder)activityBuilder).AddAgenticActivity<TAIAgent>(agenticActivityNodeName, b => buildAction?.Invoke(b as IOverridenReactiveStructuredActivityExternalsBuilder)) as IOverridenActivityBuilder;

    /// <summary>Adds a typed-agent structured activity to an overridden activity using its default node name.</summary>
    /// <typeparam name="TAIAgent">The Stateflows agent implementation type.</typeparam>
    /// <param name="activityBuilder">The overridden activity builder receiving the agentic node.</param>
    /// <param name="buildAction">Optional configuration for the overridden structured activity's external inputs and outputs.</param>
    /// <returns>The overridden activity builder for further configuration.</returns>
    public static IOverridenActivityBuilder AddAgenticActivity<TAIAgent>(this IOverridenActivityBuilder activityBuilder, OverridenReactiveStructuredActivityExternalsBuildAction? buildAction = null)
        where TAIAgent : class, IAIAgent
        => AddAgenticActivity<TAIAgent>(activityBuilder, ActivityNode<AIAgentActionNode<TAIAgent>>.Name, buildAction);
}