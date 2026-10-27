using Stateflows.Extensions.MicrosoftAgentFramework.Agents.Classes;
using Stateflows.MAF.AIAgents.Registration;
using Stateflows.StateMachines;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.MAF.AIAgents.Extensions;

/// <summary>Registers agentic states in state-machine regions.</summary>
public static class RegionBuilderExtensions
{    
    /// <summary>Adds an initial region state that creates an agent from an asynchronous factory.</summary>
    /// <param name="builder">The region builder receiving the initial state.</param>
    /// <param name="stateName">The initial state name.</param>
    /// <param name="aiAgentFactoryAsync">Asynchronously creates the Microsoft Agent Framework agent.</param>
    /// <param name="buildAction">Optional configuration for the agent action.</param>
    /// <param name="stateBuildAction">Optional configuration for the hosted agent state.</param>
    /// <returns>The initialized region builder.</returns>
    public static IInitializedRegionBuilder AddInitialAgenticState(this IRegionBuilder builder, string stateName, AIAgentFactoryAsync aiAgentFactoryAsync, AIAgentBuildAction? buildAction = null, Action<IAgenticStateBuilder>? stateBuildAction = null)
        => builder.AddInitialState(stateName, b =>
        {
            var behaviorStateBuilder = b.AddDoAIAgent(aiAgentFactoryAsync, buildAction);
            
            stateBuildAction?.Invoke(new AgenticStateBuilder(behaviorStateBuilder));
        });
    
    /// <summary>Adds an initial region state for a typed agent using its inferred state name.</summary>
    /// <typeparam name="TAIAgent">The Stateflows agent implementation type.</typeparam>
    /// <param name="builder">The region builder receiving the initial state.</param>
    /// <param name="stateBuildAction">Optional configuration for the hosted agent state.</param>
    /// <returns>The initialized region builder.</returns>
    public static IInitializedRegionBuilder AddInitialAgenticState<TAIAgent>(this IRegionBuilder builder, Action<IAgenticStateBuilder>? stateBuildAction = null)
        where TAIAgent : class, IAIAgent
        => AddInitialAgenticState<TAIAgent>(builder, State<AgenticState<TAIAgent>>.Name, stateBuildAction);
    
    /// <summary>Adds a named initial region state for a typed agent.</summary>
    /// <typeparam name="TAIAgent">The Stateflows agent implementation type.</typeparam>
    /// <param name="builder">The region builder receiving the initial state.</param>
    /// <param name="stateName">The initial state name.</param>
    /// <param name="stateBuildAction">Optional configuration for the hosted agent state.</param>
    /// <returns>The initialized region builder.</returns>
    public static IInitializedRegionBuilder AddInitialAgenticState<TAIAgent>(this IRegionBuilder builder, string stateName, Action<IAgenticStateBuilder>? stateBuildAction = null)
        where TAIAgent : class, IAIAgent
        => builder.AddInitialState<AgenticState<TAIAgent>>(stateName, b =>
        {
            var behaviorStateBuilder = b.AddDoAIAgent<TAIAgent>();
            
            stateBuildAction?.Invoke(new AgenticStateBuilder(behaviorStateBuilder));
        });
    
        
    /// <summary>Adds a region state that creates an agent from an asynchronous factory.</summary>
    /// <param name="builder">The initialized region builder receiving the state.</param>
    /// <param name="stateName">The new state name.</param>
    /// <param name="aiAgentFactoryAsync">Asynchronously creates the Microsoft Agent Framework agent.</param>
    /// <param name="buildAction">Optional configuration for the agent action.</param>
    /// <param name="stateBuildAction">Optional configuration for the hosted agent state.</param>
    /// <returns>The initialized region builder.</returns>
    public static IInitializedRegionBuilder AddAgenticState(this IInitializedRegionBuilder builder, string stateName, AIAgentFactoryAsync aiAgentFactoryAsync, AIAgentBuildAction? buildAction = null, Action<IAgenticStateBuilder>? stateBuildAction = null)
        => builder.AddState(stateName, b =>
        {
            var behaviorStateBuilder = b.AddDoAIAgent(aiAgentFactoryAsync, buildAction);
            
            stateBuildAction?.Invoke(new AgenticStateBuilder(behaviorStateBuilder));
        });
    
    /// <summary>Adds a region state for a typed agent using its inferred state name.</summary>
    /// <typeparam name="TAIAgent">The Stateflows agent implementation type.</typeparam>
    /// <param name="builder">The initialized region builder receiving the state.</param>
    /// <param name="stateBuildAction">Optional configuration for the hosted agent state.</param>
    /// <returns>The initialized region builder.</returns>
    public static IInitializedRegionBuilder AddAgenticState<TAIAgent>(this IInitializedRegionBuilder builder, Action<IAgenticStateBuilder>? stateBuildAction = null)
        where TAIAgent : class, IAIAgent
        => builder.AddAgenticState<TAIAgent>(State<AgenticState<TAIAgent>>.Name, stateBuildAction);

    /// <summary>Adds a named region state for a typed agent.</summary>
    /// <typeparam name="TAIAgent">The Stateflows agent implementation type.</typeparam>
    /// <param name="builder">The initialized region builder receiving the state.</param>
    /// <param name="stateName">The new state name.</param>
    /// <param name="stateBuildAction">Optional configuration for the hosted agent state.</param>
    /// <returns>The initialized region builder.</returns>
    public static IInitializedRegionBuilder AddAgenticState<TAIAgent>(this IInitializedRegionBuilder builder, string stateName, Action<IAgenticStateBuilder>? stateBuildAction = null)
        where TAIAgent : class, IAIAgent
        => builder.AddState<AgenticState<TAIAgent>>(stateName, b =>
        {
            var behaviorStateBuilder = b.AddDoAIAgent<TAIAgent>();
            
            stateBuildAction?.Invoke(new AgenticStateBuilder(behaviorStateBuilder));
        });
}