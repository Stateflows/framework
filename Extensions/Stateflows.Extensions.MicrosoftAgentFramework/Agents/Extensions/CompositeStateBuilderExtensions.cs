using Stateflows.Extensions.MicrosoftAgentFramework.Agents.Classes;
using Stateflows.MAF.AIAgents.Registration;
using Stateflows.StateMachines;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.MAF.AIAgents.Extensions;

/// <summary>Registers agentic states in composite states.</summary>
public static class CompositeStateBuilderExtensions
{    
    /// <summary>Adds an initial composite state that creates an agent from an asynchronous factory.</summary>
    /// <param name="builder">The composite-state builder receiving the initial state.</param>
    /// <param name="stateName">The initial state name.</param>
    /// <param name="aiAgentFactoryAsync">Asynchronously creates the Microsoft Agent Framework agent.</param>
    /// <param name="buildAction">Optional configuration for the agent action.</param>
    /// <param name="stateBuildAction">Optional configuration for the hosted agent state.</param>
    /// <returns>The initialized composite-state builder.</returns>
    public static IInitializedCompositeStateBuilder AddInitialAgenticState(this ICompositeStateBuilder builder, string stateName, AIAgentFactoryAsync aiAgentFactoryAsync, AIAgentBuildAction? buildAction = null, Action<IAgenticStateBuilder>? stateBuildAction = null)
        => builder.AddInitialState(stateName, b =>
        {
            var behaviorStateBuilder = b.AddDoAIAgent(aiAgentFactoryAsync, buildAction);
            
            stateBuildAction?.Invoke(new AgenticStateBuilder(behaviorStateBuilder));
        });
    
    /// <summary>Adds an initial composite state for a typed agent using its inferred state name.</summary>
    /// <typeparam name="TAIAgent">The Stateflows agent implementation type.</typeparam>
    /// <param name="builder">The composite-state builder receiving the initial state.</param>
    /// <param name="stateBuildAction">Optional configuration for the hosted agent state.</param>
    /// <returns>The initialized composite-state builder.</returns>
    public static IInitializedCompositeStateBuilder AddInitialAgenticState<TAIAgent>(this ICompositeStateBuilder builder, Action<IAgenticStateBuilder>? stateBuildAction = null)
        where TAIAgent : class, IAIAgent
        => builder.AddInitialAgenticState<TAIAgent>(State<AgenticState<TAIAgent>>.Name, stateBuildAction);
    
    /// <summary>Adds a named initial composite state for a typed agent.</summary>
    /// <typeparam name="TAIAgent">The Stateflows agent implementation type.</typeparam>
    /// <param name="builder">The composite-state builder receiving the initial state.</param>
    /// <param name="stateName">The initial state name.</param>
    /// <param name="stateBuildAction">Optional configuration for the hosted agent state.</param>
    /// <returns>The initialized composite-state builder.</returns>
    public static IInitializedCompositeStateBuilder AddInitialAgenticState<TAIAgent>(this ICompositeStateBuilder builder, string stateName, Action<IAgenticStateBuilder>? stateBuildAction = null)
        where TAIAgent : class, IAIAgent
        => builder.AddInitialState<AgenticState<TAIAgent>>(stateName, b =>
        {
            var behaviorStateBuilder = b.AddDoAIAgent<TAIAgent>();
            
            stateBuildAction?.Invoke(new AgenticStateBuilder(behaviorStateBuilder));
        });
    
        
    /// <summary>Adds a composite state that creates an agent from an asynchronous factory.</summary>
    /// <param name="builder">The initialized composite-state builder receiving the state.</param>
    /// <param name="stateName">The new state name.</param>
    /// <param name="aiAgentFactoryAsync">Asynchronously creates the Microsoft Agent Framework agent.</param>
    /// <param name="buildAction">Optional configuration for the agent action.</param>
    /// <param name="stateBuildAction">Optional configuration for the hosted agent state.</param>
    /// <returns>The initialized composite-state builder.</returns>
    public static IInitializedCompositeStateBuilder AddAgenticState(this IInitializedCompositeStateBuilder builder, string stateName, AIAgentFactoryAsync aiAgentFactoryAsync, AIAgentBuildAction? buildAction = null, Action<IAgenticStateBuilder>? stateBuildAction = null)
        => builder.AddState(stateName, b =>
        {
            var behaviorStateBuilder = b.AddDoAIAgent(aiAgentFactoryAsync, buildAction);
            
            stateBuildAction?.Invoke(new AgenticStateBuilder(behaviorStateBuilder));
        });
    
    /// <summary>Adds a composite state for a typed agent using its inferred state name.</summary>
    /// <typeparam name="TAIAgent">The Stateflows agent implementation type.</typeparam>
    /// <param name="builder">The initialized composite-state builder receiving the state.</param>
    /// <param name="stateBuildAction">Optional configuration for the hosted agent state.</param>
    /// <returns>The initialized composite-state builder.</returns>
    public static IInitializedCompositeStateBuilder AddAgenticState<TAIAgent>(this IInitializedCompositeStateBuilder builder, Action<IAgenticStateBuilder>? stateBuildAction = null)
        where TAIAgent : class, IAIAgent
        => builder.AddAgenticState<TAIAgent>(State<AgenticState<TAIAgent>>.Name, stateBuildAction);

    /// <summary>Adds a named composite state for a typed agent.</summary>
    /// <typeparam name="TAIAgent">The Stateflows agent implementation type.</typeparam>
    /// <param name="builder">The initialized composite-state builder receiving the state.</param>
    /// <param name="stateName">The new state name.</param>
    /// <param name="stateBuildAction">Optional configuration for the hosted agent state.</param>
    /// <returns>The initialized composite-state builder.</returns>
    public static IInitializedCompositeStateBuilder AddAgenticState<TAIAgent>(this IInitializedCompositeStateBuilder builder, string stateName, Action<IAgenticStateBuilder>? stateBuildAction = null)
        where TAIAgent : class, IAIAgent
        => builder.AddState<AgenticState<TAIAgent>>(stateName, b =>
        {
            var behaviorStateBuilder = b.AddDoAIAgent<TAIAgent>();
            
            stateBuildAction?.Invoke(new AgenticStateBuilder(behaviorStateBuilder));
        });
}