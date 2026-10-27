using Stateflows.Activities;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Common.Interfaces;
using Stateflows.MAF.AIAgents.Classes;
using Stateflows.MAF.AIAgents.Registration;

namespace Stateflows.MAF.AIAgents.Extensions;

/// <summary>Builds agentic activities directly in reactive structured activity definitions.</summary>
public static class ReactiveStructuredActivityBuilderAgenticExtensions
{
    /// <summary>Adds a named agentic activity using a synchronous agent factory.</summary>
    /// <param name="activityBuilder">The reactive structured activity builder receiving the agentic content.</param>
    /// <param name="agenticActivityNodeName">The subactivity name.</param>
    /// <param name="aiAgentFactory">Creates the Microsoft Agent Framework agent.</param>
    /// <param name="buildAction">Optional configuration for external activity inputs and outputs.</param>
    /// <returns>The reactive structured activity builder for further configuration.</returns>
    public static IReactiveStructuredActivityBuilder AddAgenticActivity(this IReactiveStructuredActivityBuilder activityBuilder, string agenticActivityNodeName, AIAgentFactory aiAgentFactory, ReactiveStructuredActivityExternalsBuildAction? buildAction = null)
        => AddAgenticActivity(activityBuilder, agenticActivityNodeName, (sp, tools) => Task.FromResult(aiAgentFactory(sp, tools)), buildAction);
    
    /// <summary>Adds an agentic activity using the default name and a synchronous agent factory.</summary>
    /// <param name="activityBuilder">The reactive structured activity builder receiving the agentic content.</param>
    /// <param name="aiAgentFactory">Creates the Microsoft Agent Framework agent.</param>
    /// <param name="buildAction">Optional configuration for external activity inputs and outputs.</param>
    /// <returns>The reactive structured activity builder for further configuration.</returns>
    public static IReactiveStructuredActivityBuilder AddAgenticActivity(this IReactiveStructuredActivityBuilder activityBuilder, AIAgentFactory aiAgentFactory, ReactiveStructuredActivityExternalsBuildAction? buildAction = null)
        => AddAgenticActivity(activityBuilder, ActivityNode<AIAgentActionNode>.Name, aiAgentFactory, buildAction);

    /// <summary>Adds a named agentic activity using an asynchronous agent factory.</summary>
    /// <param name="activityBuilder">The reactive structured activity builder receiving the agentic content.</param>
    /// <param name="agenticActivityNodeName">The subactivity name.</param>
    /// <param name="aiAgentFactoryAsync">Asynchronously creates the Microsoft Agent Framework agent.</param>
    /// <param name="buildAction">Optional configuration for external activity inputs and outputs.</param>
    /// <returns>The reactive structured activity builder for further configuration.</returns>
    public static IReactiveStructuredActivityBuilder AddAgenticActivity(this IReactiveStructuredActivityBuilder activityBuilder,
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
    
    /// <summary>Adds an agentic activity using the default name and an asynchronous agent factory.</summary>
    /// <param name="activityBuilder">The reactive structured activity builder receiving the agentic content.</param>
    /// <param name="aiAgentFactoryAsync">Asynchronously creates the Microsoft Agent Framework agent.</param>
    /// <param name="buildAction">Optional configuration for external activity inputs and outputs.</param>
    /// <returns>The reactive structured activity builder for further configuration.</returns>
    public static IReactiveStructuredActivityBuilder AddAgenticActivity(this IReactiveStructuredActivityBuilder activityBuilder, AIAgentFactoryAsync aiAgentFactoryAsync, ReactiveStructuredActivityExternalsBuildAction? buildAction = null)
        => AddAgenticActivity(activityBuilder, ActivityNode<AIAgentActionNode>.Name, aiAgentFactoryAsync, buildAction);
    
    /// <summary>Adds a named agentic activity that creates the specified typed agent.</summary>
    /// <typeparam name="TAIAgent">The Stateflows agent implementation type.</typeparam>
    /// <param name="activityBuilder">The reactive structured activity builder receiving the agentic content.</param>
    /// <param name="agenticActivityNodeName">The subactivity name.</param>
    /// <param name="buildAction">Optional configuration for external activity inputs and outputs.</param>
    /// <returns>The reactive structured activity builder for further configuration.</returns>
    public static IReactiveStructuredActivityBuilder AddAgenticActivity<TAIAgent>(this IReactiveStructuredActivityBuilder activityBuilder, string agenticActivityNodeName, ReactiveStructuredActivityExternalsBuildAction? buildAction = null)
        where TAIAgent : class, IAIAgent
        => activityBuilder.AddStructuredActivity(agenticActivityNodeName, b =>
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

    /// <summary>Adds an agentic activity for the specified typed agent using its default name.</summary>
    /// <typeparam name="TAIAgent">The Stateflows agent implementation type.</typeparam>
    /// <param name="activityBuilder">The reactive structured activity builder receiving the agentic content.</param>
    /// <param name="buildAction">Optional configuration for external activity inputs and outputs.</param>
    /// <returns>The reactive structured activity builder for further configuration.</returns>
    public static IReactiveStructuredActivityBuilder AddAgenticActivity<TAIAgent>(this IReactiveStructuredActivityBuilder activityBuilder, ReactiveStructuredActivityExternalsBuildAction? buildAction = null)
        where TAIAgent : class, IAIAgent
        => AddAgenticActivity<TAIAgent>(activityBuilder, ActivityNode<AIAgentActionNode<TAIAgent>>.Name, buildAction);
    
    
    
    /// <summary>Adds a named agentic activity to an overridden reactive activity using a synchronous factory.</summary>
    /// <param name="activityBuilder">The overridden reactive structured activity builder receiving the agentic content.</param>
    /// <param name="agenticActivityNodeName">The subactivity name.</param>
    /// <param name="aiAgentFactory">Creates the Microsoft Agent Framework agent.</param>
    /// <param name="buildAction">Optional configuration for overridden external activity inputs and outputs.</param>
    /// <returns>The overridden reactive structured activity builder for further configuration.</returns>
    public static IOverridenReactiveStructuredActivityBuilder AddAgenticActivity(this IOverridenReactiveStructuredActivityBuilder activityBuilder, string agenticActivityNodeName, AIAgentFactory aiAgentFactory, OverridenReactiveStructuredActivityExternalsBuildAction? buildAction = null)
        => AddAgenticActivity(activityBuilder, agenticActivityNodeName, (sp, tools) => Task.FromResult(aiAgentFactory(sp, tools)), buildAction);
    
    /// <summary>Adds an agentic activity to an overridden reactive activity using the default name and a synchronous factory.</summary>
    /// <param name="activityBuilder">The overridden reactive structured activity builder receiving the agentic content.</param>
    /// <param name="aiAgentFactory">Creates the Microsoft Agent Framework agent.</param>
    /// <param name="buildAction">Optional configuration for overridden external activity inputs and outputs.</param>
    /// <returns>The overridden reactive structured activity builder for further configuration.</returns>
    public static IOverridenReactiveStructuredActivityBuilder AddAgenticActivity(this IOverridenReactiveStructuredActivityBuilder activityBuilder, AIAgentFactory aiAgentFactory, OverridenReactiveStructuredActivityExternalsBuildAction? buildAction = null)
        => AddAgenticActivity(activityBuilder, ActivityNode<AIAgentActionNode>.Name, aiAgentFactory, buildAction);

    /// <summary>Adds a named agentic activity to an overridden reactive activity using an asynchronous factory.</summary>
    /// <param name="activityBuilder">The overridden reactive structured activity builder receiving the agentic content.</param>
    /// <param name="agenticActivityNodeName">The subactivity name.</param>
    /// <param name="aiAgentFactoryAsync">Asynchronously creates the Microsoft Agent Framework agent.</param>
    /// <param name="buildAction">Optional configuration for overridden external activity inputs and outputs.</param>
    /// <returns>The overridden reactive structured activity builder for further configuration.</returns>
    public static IOverridenReactiveStructuredActivityBuilder AddAgenticActivity(this IOverridenReactiveStructuredActivityBuilder activityBuilder,
        string agenticActivityNodeName, AIAgentFactoryAsync aiAgentFactoryAsync, OverridenReactiveStructuredActivityExternalsBuildAction? buildAction = null)
        => ((IReactiveStructuredActivityBuilder)activityBuilder).AddAgenticActivity(agenticActivityNodeName, aiAgentFactoryAsync, b => buildAction(b as IOverridenReactiveStructuredActivityExternalsBuilder)) as IOverridenReactiveStructuredActivityBuilder;
    
    /// <summary>Adds an agentic activity to an overridden reactive activity using the default name and an asynchronous factory.</summary>
    /// <param name="activityBuilder">The overridden reactive structured activity builder receiving the agentic content.</param>
    /// <param name="aiAgentFactoryAsync">Asynchronously creates the Microsoft Agent Framework agent.</param>
    /// <param name="buildAction">Optional configuration for overridden external activity inputs and outputs.</param>
    /// <returns>The overridden reactive structured activity builder for further configuration.</returns>
    public static IOverridenReactiveStructuredActivityBuilder AddAgenticActivity(this IOverridenReactiveStructuredActivityBuilder activityBuilder, AIAgentFactoryAsync aiAgentFactoryAsync, OverridenReactiveStructuredActivityExternalsBuildAction? buildAction = null)
        => AddAgenticActivity(activityBuilder, ActivityNode<AIAgentActionNode>.Name, aiAgentFactoryAsync, buildAction);
    
    /// <summary>Adds a named typed-agent activity to an overridden reactive activity.</summary>
    /// <typeparam name="TAIAgent">The Stateflows agent implementation type.</typeparam>
    /// <param name="activityBuilder">The overridden reactive structured activity builder receiving the agentic content.</param>
    /// <param name="agenticActivityNodeName">The subactivity name.</param>
    /// <param name="buildAction">Optional configuration for overridden external activity inputs and outputs.</param>
    /// <returns>The overridden reactive structured activity builder for further configuration.</returns>
    public static IOverridenReactiveStructuredActivityBuilder AddAgenticActivity<TAIAgent>(this IOverridenReactiveStructuredActivityBuilder activityBuilder, string agenticActivityNodeName, OverridenReactiveStructuredActivityExternalsBuildAction? buildAction = null)
        where TAIAgent : class, IAIAgent
        => ((IReactiveStructuredActivityBuilder)activityBuilder).AddAgenticActivity<TAIAgent>(agenticActivityNodeName, b => buildAction(b as IOverridenReactiveStructuredActivityExternalsBuilder)) as IOverridenReactiveStructuredActivityBuilder;

    /// <summary>Adds a typed-agent activity to an overridden reactive activity using its default name.</summary>
    /// <typeparam name="TAIAgent">The Stateflows agent implementation type.</typeparam>
    /// <param name="activityBuilder">The overridden reactive structured activity builder receiving the agentic content.</param>
    /// <param name="buildAction">Optional configuration for overridden external activity inputs and outputs.</param>
    /// <returns>The overridden reactive structured activity builder for further configuration.</returns>
    public static IOverridenReactiveStructuredActivityBuilder AddAgenticActivity<TAIAgent>(this IOverridenReactiveStructuredActivityBuilder activityBuilder, OverridenReactiveStructuredActivityExternalsBuildAction? buildAction = null)
        where TAIAgent : class, IAIAgent
        => AddAgenticActivity<TAIAgent>(activityBuilder, ActivityNode<AIAgentActionNode<TAIAgent>>.Name, buildAction);
}