using Stateflows.Extensions.MicrosoftAgentFramework.Agents.Classes;
using Stateflows.MAF.AIAgents.Registration;
using Stateflows.StateMachines;

namespace Stateflows.MAF.AIAgents.Extensions;

/// <summary>Registers agentic states in state-machine definitions.</summary>
public static class StateMachineBuilderExtensions
{    
    /// <summary>Adds an initial state that creates an agent from an asynchronous factory.</summary>
    /// <param name="builder">The state-machine builder receiving the initial state.</param>
    /// <param name="stateName">The initial state name.</param>
    /// <param name="aiAgentFactoryAsync">Asynchronously creates the Microsoft Agent Framework agent.</param>
    /// <param name="buildAction">Optional configuration for the agent action.</param>
    /// <param name="stateBuildAction">Optional configuration for the hosted agent state.</param>
    /// <returns>The initialized state-machine builder.</returns>
    public static IInitializedStateMachineBuilder AddInitialAgenticState(this IStateMachineBuilder builder, string stateName, AIAgentFactoryAsync aiAgentFactoryAsync, AIAgentBuildAction? buildAction = null, Action<IAgenticStateBuilder>? stateBuildAction = null)
        => builder.AddInitialState(stateName, b =>
        {
            var behaviorStateBuilder = b.AddDoAIAgent(aiAgentFactoryAsync, buildAction);
            
            stateBuildAction?.Invoke(new AgenticStateBuilder(behaviorStateBuilder));
        });
    
    /// <summary>Adds an initial state for a typed agent using its inferred state name.</summary>
    /// <typeparam name="TAIAgent">The Stateflows agent implementation type.</typeparam>
    /// <param name="builder">The state-machine builder receiving the initial state.</param>
    /// <param name="stateBuildAction">Optional configuration for the hosted agent state.</param>
    /// <returns>The initialized state-machine builder.</returns>
    public static IInitializedStateMachineBuilder AddInitialAgenticState<TAIAgent>(this IStateMachineBuilder builder, Action<IAgenticStateBuilder>? stateBuildAction = null)
        where TAIAgent : class, IAIAgent
        => AddInitialAgenticState<TAIAgent>(builder, State<AgenticState<TAIAgent>>.Name, stateBuildAction);
    
    /// <summary>Adds a named initial state for a typed agent.</summary>
    /// <typeparam name="TAIAgent">The Stateflows agent implementation type.</typeparam>
    /// <param name="builder">The state-machine builder receiving the initial state.</param>
    /// <param name="stateName">The initial state name.</param>
    /// <param name="stateBuildAction">Optional configuration for the hosted agent state.</param>
    /// <returns>The initialized state-machine builder.</returns>
    public static IInitializedStateMachineBuilder AddInitialAgenticState<TAIAgent>(this IStateMachineBuilder builder, string stateName, Action<IAgenticStateBuilder>? stateBuildAction = null)
        where TAIAgent : class, IAIAgent
        => builder.AddInitialState<AgenticState<TAIAgent>>(stateName, b =>
        {
            var behaviorStateBuilder = b.AddDoAIAgent<TAIAgent>();
            
            stateBuildAction?.Invoke(new AgenticStateBuilder(behaviorStateBuilder));
        });
    
        
    /// <summary>Adds a state that creates an agent from an asynchronous factory.</summary>
    /// <param name="builder">The initialized state-machine builder receiving the state.</param>
    /// <param name="stateName">The new state name.</param>
    /// <param name="aiAgentFactoryAsync">Asynchronously creates the Microsoft Agent Framework agent.</param>
    /// <param name="buildAction">Optional configuration for the agent action.</param>
    /// <param name="stateBuildAction">Optional configuration for the hosted agent state.</param>
    /// <returns>The initialized state-machine builder.</returns>
    public static IInitializedStateMachineBuilder AddAgenticState(this IInitializedStateMachineBuilder builder, string stateName, AIAgentFactoryAsync aiAgentFactoryAsync, AIAgentBuildAction? buildAction = null, Action<IAgenticStateBuilder>? stateBuildAction = null)
        => builder.AddState(stateName, b =>
        {
            var behaviorStateBuilder = b.AddDoAIAgent(aiAgentFactoryAsync, buildAction);
            
            stateBuildAction?.Invoke(new AgenticStateBuilder(behaviorStateBuilder));
        });
    
    /// <summary>Adds a state for a typed agent using its inferred state name.</summary>
    /// <typeparam name="TAIAgent">The Stateflows agent implementation type.</typeparam>
    /// <param name="builder">The initialized state-machine builder receiving the state.</param>
    /// <param name="stateBuildAction">Optional configuration for the hosted agent state.</param>
    /// <returns>The initialized state-machine builder.</returns>
    public static IInitializedStateMachineBuilder AddAgenticState<TAIAgent>(this IInitializedStateMachineBuilder builder, Action<IAgenticStateBuilder>? stateBuildAction = null)
        where TAIAgent : class, IAIAgent
        => AddAgenticState<TAIAgent>(builder, State<AgenticState<TAIAgent>>.Name, stateBuildAction);

    /// <summary>Adds a named state for a typed agent.</summary>
    /// <typeparam name="TAIAgent">The Stateflows agent implementation type.</typeparam>
    /// <param name="builder">The initialized state-machine builder receiving the state.</param>
    /// <param name="stateName">The new state name.</param>
    /// <param name="stateBuildAction">Optional configuration for the hosted agent state.</param>
    /// <returns>The initialized state-machine builder.</returns>
    public static IInitializedStateMachineBuilder AddAgenticState<TAIAgent>(this IInitializedStateMachineBuilder builder, string stateName, Action<IAgenticStateBuilder>? stateBuildAction = null)
        where TAIAgent : class, IAIAgent
        => builder.AddState<AgenticState<TAIAgent>>(stateName, b =>
        {
            var behaviorStateBuilder = b.AddDoAIAgent<TAIAgent>();
            
            stateBuildAction?.Invoke(new AgenticStateBuilder(behaviorStateBuilder));
        });
}